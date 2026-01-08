using ItemChanger.Extensions;
using KnightOfNights.Scripts.SharedLib;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.Framework;

[Shim]
public enum DarknessLevel
{
    Undark = -1,
    Default = 0,
    SemiDark = 1,
    Dark = 2
}

[Shim]
public class DarknessRegion : MonoBehaviour
{
    private static readonly HashSet<DarknessRegion> activeControls = [];

    private static int? prevDarkness;
    private static bool pendingUpdate = false;

    [ShimField] public DarknessLevel DarknessLevel;

    private static int? ComputeDarkness()
    {
        int? darkness = null;
        foreach (var control in activeControls)
        {
            int d = (int)control.DarknessLevel;
            if (darkness == null || d > darkness.Value) darkness = d;
        }
        return darkness;
    }

    private void Update()
    {
        if (pendingUpdate) UpdateDarkness();
    }

    private static void UpdateDarkness()
    {
        pendingUpdate = true;

        int? newDarkness = ComputeDarkness();
        if (newDarkness != prevDarkness)
        {
            var sceneManager = GameObject.FindGameObjectWithTag("SceneManager")?.GetComponent<SceneManager>();
            if (sceneManager == null) return;

            var fsm = HeroController.instance.gameObject.FindChild("Vignette").LocateMyFSM("Darkness Control");
            fsm.FsmVariables.GetFsmInt("Darkness Level").Value = newDarkness ?? sceneManager.darknessLevel;
            fsm.SendEvent("SCENE RESET");

            prevDarkness = newDarkness;
        }

        pendingUpdate = false;
    }

    private void Activate()
    {
        if (activeControls.Add(this)) UpdateDarkness();
    }

    private void Deactivate()
    {
        if (activeControls.Remove(this)) UpdateDarkness();
    }

    private void OnTriggerEnter2D(Collider2D hero) => Activate();

    private void OnTriggerExit2D(Collider2D hero) => Deactivate();

    private void OnTriggerStay2D(Collider2D hero) => Activate();

    private void OnDisable() => Deactivate();
}
