using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KnightOfNights.Scripts;

public static class KnightOfNightsBundleAPI
{
    private static AssetBundle? shared;
    private static readonly Dictionary<string, UnityEngine.Object> prefabs = [];

    static KnightOfNightsBundleAPI() => Load();

    private static bool loaded = false;
    internal static void Load()
    {
        if (loaded) return;
        loaded = true;

        shared = LoadCoreBundle();
        foreach (var obj in shared.LoadAllAssets()) prefabs[obj.name] = obj;
    }

    public static T LoadPrefab<T>(string name) where T : UnityEngine.Object
    {
        if (prefabs.TryGetValue(name, out var obj) && obj is T typed) return typed;
        throw new ArgumentException($"Unknown Prefab: {name}");
    }

    private const string BUNDLE = "KnightOfNights.Unity.Assets.AssetBundles.knightofnightsbundle";

    private static AssetBundle LoadCoreBundle()
    {
#if DEBUG
        try
        {
            KnightOfNightsMod.Log($"Loading {BUNDLE} from disk");
            var debugData = PurenailCore.SystemUtil.JsonUtil<KnightOfNightsMod>.DeserializeEmbedded<Build.DebugData>("KnightOfNights.Resources.Data.debug.json");
            var bundle = AssetBundle.LoadFromFile($"{debugData.LocalAssetBundlesPath}/{BUNDLE}");
            KnightOfNightsMod.Log($"Loading {BUNDLE} from disk: success!");
            return bundle;
        }
        catch (Exception e) { KnightOfNightsMod.BUG($"Failed to load {BUNDLE} from local assets: {e}"); }
#endif

        using StreamReader sr = new(typeof(KnightOfNightsBundleAPI).Assembly.GetManifestResourceStream(BUNDLE));
        return AssetBundle.LoadFromStream(sr.BaseStream);
    }
}
