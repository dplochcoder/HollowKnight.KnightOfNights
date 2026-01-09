using ItemChanger;
using KnightOfNights.IC;
using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

[Shim]
internal class DeathAnimWarp : MonoBehaviour
{
    internal const string MARKER_NAME = "door_RevekWarpMarker";

    [ShimMethod]
    public void WarpToCrown()
    {
        FallenGuardianModule.Get()?.DefeatedBoss = true;
        GameManager.instance.BeginSceneTransition(new()
        {
            SceneName = SceneNames.Mines_34,
            EntryGateName = MARKER_NAME,
            EntryDelay = 0f,
            Visualization = GameManager.SceneLoadVisualizations.Dream,
            PreventCameraFadeOut = true,
            WaitForSceneTransitionCameraFade = false,
            AlwaysUnloadUnusedAssets = false
        });
    }
}
