using KnightOfNights.IC;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;

namespace KnightOfNights;

public class KnightOfNightsMod : Mod
{
    public static KnightOfNightsMod? Instance { get; private set; }

    private static readonly string VERSION = PurenailCore.ModUtil.VersionUtil.ComputeVersion<KnightOfNightsMod>();

    public override string GetVersion() => VERSION;

    public KnightOfNightsMod() : base("KnightOfNights")
    {
        Instance = this;
        KnightOfNightsBundleAPI.Load();

        ShadelessModule.HookItemHelper();
        RevekSongModule.HookItemHelper();
        WarriorsNotesModule.HookItemHelper();
    }

    private static void SetupDebug() => Debug.DebugInterop.Setup();

    public override List<(string, string)> GetPreloadNames() => KnightOfNightsPreloader.Instance.GetPreloadNames();

    public override (string, Func<IEnumerator>)[] PreloadSceneHooks() => KnightOfNightsPreloader.Instance.PreloadSceneHooks();

    public override void Initialize(Dictionary<string, Dictionary<string, UnityEngine.GameObject>> preloadedObjects)
    {
        KnightOfNightsPreloader.Instance.Initialize(preloadedObjects);

        On.UIManager.StartNewGame += OnStartNewGame;

        if (ModHooks.GetMod("DebugMod") is Mod) SetupDebug();
    }

    private void OnStartNewGame(On.UIManager.orig_StartNewGame orig, UIManager self, bool permadeath, bool bossRush)
    {
        orig(self, permadeath, bossRush);

#if DEBUG
        ItemChanger.ItemChangerMod.CreateSettingsProfile(false);
        ItemChanger.ItemChangerMod.Modules.Add<PlandoModule>();
#endif
    }

    public static new void Log(string msg) => (Instance as ILogger)!.Log(msg);

    public static new void LogError(string msg) => (Instance as ILogger)!.LogError(msg);

    public static void BUG(string msg) => (Instance as ILogger)!.LogError($"BUG: {msg}");
}
