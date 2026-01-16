using ItemChanger;
using KnightOfNights.Scripts.FallenGuardian;
using KnightOfNights.Scripts.SharedLib;
using Modding;
using Newtonsoft.Json;
using PurenailCore.ICUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KnightOfNights.IC;

internal class CrownTransition : ITransition
{
    [JsonIgnore]
    public string SceneName => (FallenGuardianModule.Get()?.DefeatedBoss ?? false) ? SceneNames.Mines_34 : SummitSceneNames.Summit_EntryHall;

    [JsonIgnore]
    public string GateName => "bot1";
}

[PlandoSubmodule]
internal class FallenGuardianModule : AbstractModule<FallenGuardianModule>
{
    private const string PREFIX = "KnightOfNights.Unity.Assets.AssetBundles.";

    private static string AssetBundleName(string sceneName) => sceneName.Replace("_", "").ToLower();

    private readonly Dictionary<string, AssetBundle?> sceneBundles = [];
    private SceneLoaderModule? coreModule;

    public bool CompletedBossIntro = false;
    public bool DefeatedBoss = false;
    public bool VisitedSummit = false;

    protected override void InitializeInternal()
    {
        ItemChangerMod.AddTransitionOverride(new(SceneNames.Mines_25, "top1"), new CrownTransition());
        Events.AddSceneChangeEdit(SceneNames.Mines_34, SpawnRespawnMarker);
        ModHooks.LanguageGetHook += LanguageGetHook;
        ModHooks.GetPlayerBoolHook += GetVisitedSummit;
        ModHooks.SetPlayerBoolHook += SetVisitedSummit;

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

    protected override void UnloadInternal()
    {
        Events.RemoveSceneChangeEdit(SceneNames.Mines_34, SpawnRespawnMarker);
        ModHooks.LanguageGetHook -= LanguageGetHook;
        ModHooks.GetPlayerBoolHook -= GetVisitedSummit;
        ModHooks.SetPlayerBoolHook -= SetVisitedSummit;

        sceneBundles.Values.ForEach(v => v?.Unload(true));
        coreModule!.RemoveOnBeforeSceneLoad(OnBeforeSceneLoad);
        coreModule.RemoveOnUnloadScene(OnUnloadScene);
    }

    private void SpawnRespawnMarker(Scene scene)
    {
        GameObject obj = new(DeathAnimWarp.MARKER_NAME);
        obj.transform.position = new(50f, 56f, 0f);
        obj.tag = "RespawnPoint";
        obj.AddComponent<RespawnMarker>().respawnFacingRight = false;
    }

    internal const string REVEK_KEY = "REVEK_BOSS";
    internal const string SUMMIT_KEY = "SUMMIT_AREA";

    private string LanguageGetHook(string key, string sheetTitle, string orig) => key switch
    {
        $"{REVEK_KEY}_SUPER" => "Fallen Guardian",
        $"{REVEK_KEY}_MAIN" => "Revek",
        $"{REVEK_KEY}_SUB" => "",
        $"{SUMMIT_KEY}_SUPER" => "",
        $"{SUMMIT_KEY}_MAIN" => "The Summit",
        $"{SUMMIT_KEY}_SUB" => "",
        _ => orig
    };

    private bool GetVisitedSummit(string name, bool orig) => name switch
    {
        nameof(VisitedSummit) => VisitedSummit,
        _ => orig
    };

    private bool SetVisitedSummit(string name, bool value) => name switch
    {
        nameof(VisitedSummit) => VisitedSummit = value,
        _ => value
    };

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
        if (sceneBundles[assetBundleName] != null)
        {
            cb();
            yield break;
        }

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
