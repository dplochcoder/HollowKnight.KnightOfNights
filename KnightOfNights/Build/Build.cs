using KnightOfNights.Scripts.SharedLib;
using PurenailCore.CollectionUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using UnityEngine;
using JsonUtil = PurenailCore.SystemUtil.JsonUtil<KnightOfNights.KnightOfNightsMod>;

namespace KnightOfNights.Build;

internal record DebugData
{
    public string LocalAssetBundlesPath = "";
    public string LocalJsonPath = "";
    public string LocalUnityJsonPath = "";
    public string GitRootPath = "";

    private static DebugData? data;

    public static DebugData Get()
    {
        try { data ??= JsonUtil.DeserializeEmbedded<DebugData>("KnightOfNights.Resources.Data.debug.json"); }
        catch (Exception) { data = new(); }
        return data;
    }
}

public static class Build
{
    public static string InferGitRoot(string path)
    {
        var data = DebugData.Get();
        if (data.GitRootPath != "") return data.GitRootPath;

        var info = Directory.GetParent(path);
        while (info != null)
        {
            if (Directory.Exists(Path.Combine(info.FullName, ".git")))
            {
                return info.FullName;
            }
            info = Directory.GetParent(info.FullName);
        }

        return path;
    }

    private static void Parallelize(IEnumerable<Action> tasks)
    {
        List<Thread> threads = [];
        Wrapped<Exception?> exception = new(null);
        foreach (var task in tasks)
        {
            var copy = task;
            Thread t = new(() =>
            {
                try { copy(); }
                catch (Exception ex) { lock (exception) { exception.Value ??= ex; } }
            });
            threads.Add(t);
            t.Start();
        }

        threads.ForEach(t => t.Join());
        if (exception.Value != null) throw exception.Value;
    }

#if DEBUG
    private const bool RELEASE_MODE = false;
#else
    private const bool RELEASE_MODE = true;
#endif

    public static void Run()
    {
        var root = InferGitRoot(Directory.GetCurrentDirectory());

        GenerateShims(root);

        Parallelize([() => BuildProject(root, "UnityScriptShims", false), () => BuildProject(root, "KnightOfNights", RELEASE_MODE)]);

        CopyDlls(root);
    }

    private static void GenerateShims(string root)
    {
        // Debug data
        DebugData debugData = new()
        {
            LocalAssetBundlesPath = $"{root}/KnightOfNights/Unity/Assets/AssetBundles",
            LocalJsonPath = $"{root}/KnightOfNights/Resources/Data",
            LocalUnityJsonPath = $"{root}/KnightOfNights/Unity/Assets/Resources/Data",
            GitRootPath = root
        };
        JsonUtil.RewriteJsonFile(debugData, $"{root}/KnightOfNights/Resources/Data/debug.json");

        // Code generation.
        GenerateUnityShims(root);
        GenerateSceneNames(root);
    }

    private static void BuildProject(string root, string project, bool release)
    {
        Process process = new()
        {
            StartInfo = new()
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "dotnet",
                Arguments = $@"build ""{Path.Combine(root, project, $"{project}.csproj")}"" --configuration {(release ? "Release" : "Debug")}",
                UseShellExecute = false
            }
        };
        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0) throw new Exception($"Failed to build {project}");
    }

    private static string ReadLocalOverrides(string root)
    {
        XmlDocument doc = new();
        doc.Load(Path.Combine(root, "LocalOverrides.targets"));

        var node = doc.DocumentElement.SelectSingleNode("/Project/PropertyGroup/HollowKnightRefs");
        return node.InnerText.Trim();
    }

    private static void CopyDlls(string root)
    {
        CopyFile(Path.Combine(root, "UnityScriptShims/bin/Debug/net472/KnightOfNights.dll"), Path.Combine(root, "KnightOfNights/Unity/Assets/Assemblies/KnightOfNights.dll"));

        var managed = ReadLocalOverrides(root);
        var modFolder = Path.Combine(managed, "Mods/KnightOfNights");
        if (!Directory.Exists(modFolder)) Directory.CreateDirectory(modFolder);

        List<string> exts = ["dll", "pdb"];
        foreach (var ext in exts) CopyFile(Path.Combine(root, $"KnightOfNights/bin/{(RELEASE_MODE ? "Release" : "Debug")}/net472/KnightOfNights.{ext}"), Path.Combine(managed, $"Mods/KnightOfNights/KnightOfNights.{ext}"));
    }

    private static void CopyFile(string src, string dst)
    {
        try
        {
            if (File.Exists(dst)) File.Delete(dst);
            File.Copy(src, dst);
        }
        catch (Exception ex) { Console.WriteLine($"Failed to copy {src} -> {dst}: {ex}"); }
    }

    private static string GenerateDirectory(string dir, Action<string> generator)
    {
        string gen = dir;
        string gen2 = $"{dir}.tmp";
        if (Directory.Exists(gen2)) Directory.Delete(gen2, true);
        Directory.CreateDirectory(gen2);

        generator(gen2);

        // On success, swap the dirs.
        if (Directory.Exists(gen)) Directory.Delete(gen, true);
        Directory.Move(gen2, gen);
        return gen;
    }

    private static void DirectoryCopy(string sourceDir, string destDir, bool recursive)
    {
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), true);
        }

        if (recursive)
        {
            foreach (var subdir in Directory.GetDirectories(sourceDir))
            {
                string childDir = Path.GetFileName(subdir);
                DirectoryCopy(subdir, Path.Combine(destDir, childDir), true);
            }
        }
    }

    private static void GenerateUnityShimsImpl(string root)
    {
        typeof(Build).Assembly.GetTypes().Where(t => t.IsDefined(typeof(Shim), false))
            .ForEach(type =>
            {
                try { GenerateShimFile(type, root); }
                catch (Exception e) { throw new Exception($"Failed to generate {type.Name}", e); }
            });
    }

    private static string GenerateUnityShims(string root)
    {
        var path = $"{root}/UnityScriptShims/Scripts/Generated";
        return GenerateDirectory(path, GenerateUnityShimsImpl);
    }

    private static void ValidateType(Type type)
    {
        if (type.Assembly.GetName().Name == "Assembly-CSharp") throw new ArgumentException($"Cannot reference Assembly-CSharp type {type.Name} directly");
        type.GenericTypeArguments.ForEach(ValidateType);
    }

    private static void GenerateShimFile(Type type, string dir)
    {
        string ns = type.Namespace;
        string origNs = ns;
        if (ns == "KnightOfNights.Scripts") ns = "";
        else if (ns.ConsumePrefix("KnightOfNights.Scripts.", out var trimmed)) ns = trimmed;

        string pathDir = ns.Length == 0 ? $"{dir}" : $"{dir}/{ns.Replace('.', '/')}";
        string path = $"{pathDir}/{type.Name}.cs";

        var baseType = type.GetCustomAttribute<Shim>()?.baseType ?? typeof(MonoBehaviour);
        string header;
        List<string> fieldStrs = [];
        List<string> attrStrs = [];
        if (type.IsEnum)
        {
            header = $"enum {type.Name}";
            foreach (var v in type.GetEnumValues()) fieldStrs.Add($"{v},");
        }
        else if (type.IsInterface)
        {
            header = $"interface {type.Name}";
        }
        else
        {
            foreach (var rc in type.GetCustomAttributes<RequireComponent>())
            {
                if (rc.m_Type0 != null) attrStrs.Add(RequireComponentStr(origNs, rc.m_Type0));
                if (rc.m_Type1 != null) attrStrs.Add(RequireComponentStr(origNs, rc.m_Type1));
                if (rc.m_Type2 != null) attrStrs.Add(RequireComponentStr(origNs, rc.m_Type2));
            }

            header = $"class {type.Name} : {PrintType(origNs, baseType)}";
            foreach (var interfaceType in type.GetInterfaces())
            {
                if (interfaceType.GetCustomAttribute<Shim>() == null) continue;
                header = $"{header}, {PrintType(origNs, interfaceType)}";
            }
            foreach (var f in type.GetFields().Where(f => f.IsDefined(typeof(ShimField), true)))
            {
                List<string> fattrStrs = [];
                foreach (var h in f.GetCustomAttributes<HeaderAttribute>()) fattrStrs.Add($"[UnityEngine.Header(\"{h.header}\")]");
                foreach (var t in f.GetCustomAttributes<TooltipAttribute>()) fattrStrs.Add($"[UnityEngine.TooltipAttribute(\"{t.tooltip}\")]");

                var fAttr = f.GetCustomAttribute<ShimField>();
                var defaultValue = fAttr.DefaultValue;
                string dv = defaultValue != null ? $" = {defaultValue}" : "";
                fieldStrs.Add($"{JoinIndented(fattrStrs, 8)}public {PrintType(origNs, f.FieldType)} {f.Name}{dv};");
            }

            foreach (var m in type.GetMethods().Where(m => m.IsDefined(typeof(ShimMethod), true)))
            {
                if (m.ReturnType != typeof(void)) throw new ArgumentException($"Method {m.Name} must return void");

                var paramsStr = string.Join(", ", m.GetParameters().Select(p => $"{PrintType(origNs, p.ParameterType)} {p.Name}"));
                fieldStrs.Add($"public void {m.Name}({paramsStr}) {{ }}");
            }
        }

        var usings = "";
        var content = $@"{usings}namespace {origNs}
{{
    {JoinIndented(attrStrs, 4)}public {header}
    {{
        {JoinIndented(fieldStrs, 8)}
    }}
}}";

        WriteSourceCode(path, content);
    }

    private static string RequireComponentStr(string ns, Type type)
    {
        ValidateType(type);
        return $"[UnityEngine.RequireComponent(typeof({PrintType(ns, type)}))]";
    }

    private static void GenerateSceneNames(string root)
    {
        List<string> sceneNames = [];
        foreach (var path in Directory.EnumerateFiles($"{root}/KnightOfNights/Unity/Assets/Scenes"))
        {
            var ext = Path.GetExtension(path);
            if (ext != ".unity") continue;

            sceneNames.Add(Path.GetFileNameWithoutExtension(path));
        }

        sceneNames.Sort();
        sceneNames.Dedup();
        WriteSourceCode($"{root}/KnightOfNights/IC/SummitSceneNames.cs", $@"namespace KnightOfNights.IC;

internal static class SummitSceneNames
{{
    {JoinIndented([.. sceneNames.Select(n => $"public const string {n} = \"{n}\";")], 4)}
}}");
    }

    private static void WriteSourceCode(string path, string content)
    {
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        File.WriteAllText(path, content.Replace("\r\n", "\n"));
    }

    private static string Pad(string src, int indent)
    {
        var splits = src.Split('\n');
        for (int i = 1; i < splits.Length; i++) splits[i] = $"{new string(' ', indent)}{splits[i]}";
        return string.Join("", splits);
    }

    private static string JoinIndented(List<string> list, int indent) => string.Join("", list.Select(s => $"{Pad(s, indent)}\n{new string(' ', indent)}"));

    private static string PrintType(string ns, Type t)
    {
        ValidateType(t);
        string s = PrintTypeImpl(ns, t);

        if (s.ConsumePrefix($"{ns}.", out string trimmed)) return trimmed;
        else return s;
    }

    private static string PrintTypeImpl(string ns, Type t)
    {
        if (!t.IsGenericType)
        {
            if (t == typeof(bool)) return "bool";
            if (t == typeof(int)) return "int";
            if (t == typeof(float)) return "float";
            if (t == typeof(string)) return "string";

            return t.FullName;
        }

        string baseName = t.FullName;
        baseName = baseName.Substring(0, baseName.IndexOf('`'));
        List<string> types = [.. t.GenericTypeArguments.Select(t => PrintType(ns, t))];
        return $"{baseName}<{string.Join(", ", types)}>";
    }
}