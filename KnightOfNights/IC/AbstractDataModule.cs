using KnightOfNights.Build;
using System;
using System.IO;

namespace KnightOfNights.IC;

using JsonUtil = PurenailCore.SystemUtil.JsonUtil<KnightOfNightsMod>;

internal abstract class AbstractDataModule<M, T> : AbstractModule<M> where M : AbstractDataModule<M, T>, new() where T : class
{
    private T? _data;
    protected T Data
    {
        get
        {
            Load();
            return _data!;
        }
    }

    protected virtual bool Unity() => false;

    private string RootPath(bool slash)
    {
        string path = Unity() ? "KnightOfNights.Unity.Assets.Resources.Data" : "KnightOfNights.Resources.Data";
        return slash ? path.Replace(".", "/") : path;
    }

    private string DebugPath()
    {
        var debugData = DebugData.Get();
        return Unity() ? debugData.LocalUnityJsonPath : debugData.LocalJsonPath;
    }

    private void Load()
    {
        if (_data != null) return;

#if DEBUG
        try
        {
            _data = JsonUtil.DeserializeFromPath<T>($"{DebugPath()}/{JsonName()}.json");
        }
        catch (Exception ex)
        {
            KnightOfNightsMod.BUG($"Failed to load {JsonName()}.json: {ex}");
        }
#endif
        _data ??= JsonUtil.DeserializeEmbedded<T>($"{RootPath(false)}.{JsonName()}.json");
        Update(_data);
    }

    internal static T LoadStatic()
    {
        M mod = new();
        mod.Load();
        return mod.Data;
    }

    protected virtual void Update(T data) { }

    public static M UpdateJson(string root)
    {
        M mod = new();
        mod.Load();

        var path = Path.Combine(root, $"{mod.RootPath(true)}/{mod.JsonName()}.json");
        File.Delete(path);
        JsonUtil.Serialize(mod.Data, path);

        return mod;
    }

    protected abstract string JsonName();

    protected override void InitializeInternal() => Load();
}