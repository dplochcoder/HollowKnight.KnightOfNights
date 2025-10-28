using ItemChanger;
using KnightOfNights.Build;
using KnightOfNights.Scripts.SharedLib;
using PurenailCore.ICUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KnightOfNights.IC;

[PlandoSubmodule]
internal class FallenGuardianModule : AbstractModule<FallenGuardianModule>
{
    private static readonly Transition TARGET_TRANSITION = new("RevekFight", "bot1");

    private const string PREFIX = "KnightOfNights.Unity.Assets.AssetBundles.";

    private static string AssetBundleName(string sceneName) => sceneName.Replace("_", "").ToLower();

    private readonly Dictionary<string, AssetBundle?> sceneBundles = [];
    private SceneLoaderModule? coreModule;

    public override void Initialize()
    {
        base.Initialize();

        ItemChangerMod.AddTransitionOverride(new(SceneNames.Mines_25, "top1"), TARGET_TRANSITION);

        foreach (var str in typeof(FallenGuardianModule).Assembly.GetManifestResourceNames())
        {
            if (!str.StartsWith(PREFIX) || str.EndsWith(".manifest") || str.EndsWith("meta")) continue;
            string name = str.Substring(PREFIX.Length);
            if (name == "AssetBundles" || name == "scenes") continue;

            sceneBundles[name] = null;
        }

        coreModule = ItemChangerMod.Modules.GetOrAdd<SceneLoaderModule>();
        coreModule.AddOnBeforeSceneLoad(OnBeforeSceneLoad);
        coreModule.AddOnUnloadScene(OnUnloadScene);
    }

    public override void Unload()
    {
        sceneBundles.Values.ForEach(v => v?.Unload(true));
        coreModule!.RemoveOnBeforeSceneLoad(OnBeforeSceneLoad);
        coreModule.RemoveOnUnloadScene(OnUnloadScene);

        base.Unload();
    }

    private void OnBeforeSceneLoad(string sceneName, Action cb)
    {
        var assetBundleName = AssetBundleName(sceneName);
        if (!sceneBundles.ContainsKey(assetBundleName))
        {
            cb();
            return;
        }

        GameManager.instance.StartCoroutine(LoadSceneAsync(assetBundleName, cb));
    }

    private void OnUnloadScene(string prevSceneName, string nextSceneName)
    {
        if (nextSceneName == prevSceneName) return;

        var assetBundleName = AssetBundleName(prevSceneName);
        if (sceneBundles.TryGetValue(assetBundleName, out var assetBundle))
        {
            assetBundle?.Unload(true);
            sceneBundles[assetBundleName] = null;
        }
    }

    private AssetBundleCreateRequest LoadAssetAsync(string name)
    {
#if DEBUG
        try
        {
            KnightOfNightsMod.Log($"Loading {name} from disk");
            var debugData = DebugData.Get();
            return AssetBundle.LoadFromFileAsync($"{debugData.LocalAssetBundlesPath}/{name}");
        }
        catch (Exception e) { Console.WriteLine($"Failed to load {name} from local assets: {e}"); }
#endif

        StreamReader sr = new(typeof(KnightOfNightsMod).Assembly.GetManifestResourceStream($"{PREFIX}{name}"));
        return AssetBundle.LoadFromStreamAsync(sr.BaseStream);
    }

    private IEnumerator LoadSceneAsyncDebug(string assetBundleName, Action cb)
    {
        sceneBundles[assetBundleName]?.Unload(true);

        var sceneRequest = LoadAssetAsync(assetBundleName);
        yield return sceneRequest;
        sceneBundles[assetBundleName] = sceneRequest.assetBundle;
        cb();
    }

    private IEnumerator LoadSceneAsyncRelease(string assetBundleName, Action cb)
    {
        if (sceneBundles[assetBundleName] != null) yield break;

        var sceneRequest = LoadAssetAsync(assetBundleName);
        yield return sceneRequest;
        sceneBundles[assetBundleName] = sceneRequest.assetBundle;
        cb();
    }

    private IEnumerator LoadSceneAsync(string assetBundleName, Action cb)
    {
#if DEBUG
        return LoadSceneAsyncDebug(assetBundleName, cb);
#else
        return LoadSceneAsyncRelease(assetBundleName, cb);
#endif
    }

    protected override FallenGuardianModule Self() => this;
}
