using GlobalEnums;
using ItemChanger;
using KnightOfNights.IC;
using KnightOfNights.Scripts.SharedLib;
using System.Collections;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

[Shim]
internal class DeathAnimWarp : MonoBehaviour
{
    internal const string MARKER_NAME = "RevekWarpMarker";

    [ShimMethod]
    public void WarpToCrown()
    {
        FallenGuardianModule.Get()?.DefeatedBoss = true;

        var pd = PlayerData.instance;
        pd.SetString(nameof(pd.respawnScene), SceneNames.Mines_34);
        pd.SetString(nameof(pd.respawnMarkerName), MARKER_NAME);
        pd.SetInt(nameof(pd.respawnType), 0);
        pd.mapZone = MapZone.PEAK;

        GameManager.instance.StartCoroutine(Respawn());
    }

    // Port of Benchwarp's ChangeScene.Respawn() but simplified, with save aspects removed and visualization changed.
    private static IEnumerator Respawn()
    {
        UIManager.instance.UIClosePauseMenu();

        // Set some stuff which would normally be set by LoadSave
        HeroController.instance.AffectedByGravity(false);
        HeroController.instance.transitionState = HeroTransitionState.EXITING_SCENE;
        HeroController.instance.cState.nearBench = false;
        GameManager.instance.cameraCtrl.FadeOut(CameraFadeType.LEVEL_TRANSITION);

        yield return new WaitForSecondsRealtime(0.5f);

        // Actually respawn the character
        GameManager.instance.SetPlayerDataBool(nameof(PlayerData.atBench), false);
        HeroController.instance.cState.superDashing = false;
        HeroController.instance.cState.spellQuake = false;
        StartRespawn(PlayerData.instance.GetString(nameof(PlayerData.respawnScene)));

        yield return new WaitWhile(() => GameManager.instance.IsInSceneTransition);

        EventRegister.SendEvent("UPDATE BLUE HEALTH"); // checks if hp is adjusted for Joni's blessing

        // Revert pause menu timescale
        Time.timeScale = 1f;
        GameManager.instance.FadeSceneIn();

        // We have to set the game non-paused because TogglePauseMenu sucks and UIClosePauseMenu doesn't do it for us.
        GameManager.instance.isPaused = false;

        // Restore various things normally handled by exiting the pause menu. None of these are necessary afaik
        GameCameras.instance.ResumeCameraShake();
        HeroController.instance.UnPause();
        MenuButtonList.ClearAllLastSelected();

        // This allows the next pause to stop the game correctly
        TimeController.GenericTimeScale = 1f;

        // Restores audio to normal levels. Unfortunately, some warps pop atm when music changes over
        GameManager.instance.actorSnapshotUnpaused.TransitionTo(0f);
        GameManager.instance.ui.AudioGoToGameplay(.2f);
    }

    private static void StartRespawn(string scene)
    {
        GameManager.instance.RespawningHero = true;
        GameManager.instance.BeginSceneTransition(new()
        {
            PreventCameraFadeOut = true,
            WaitForSceneTransitionCameraFade = false,
            EntryGateName = "",
            SceneName = scene,
            Visualization = GameManager.SceneLoadVisualizations.Dream,
            AlwaysUnloadUnusedAssets = true,
            IsFirstLevelForPlayer = false
        });
    }
}
