using KnightOfNights.Scripts.SharedLib;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.Framework;

[Shim]
internal class CustomEnviroRegion : MonoBehaviour
{
    [ShimField] public CustomEnvironmentType EnvironmentType;

    private static readonly HashSet<CustomEnviroRegion> active = [];

    private void OnDisable() => active.Remove(this);

    private void OnTriggerEnter2D()
    {
        if (!active.Add(this)) return;

        var pd = PlayerData.instance;
        pd.SetInt(nameof(pd.environmentType), EnvironmentType.ToIntId());

        HeroController.instance.checkEnvironment();
    }

    private void OnTriggerExit2D()
    {
        if (!active.Remove(this) || active.Count > 0) return;

        var pd = PlayerData.instance;
        pd.SetInt(nameof(pd.environmentType), pd.GetInt(nameof(pd.environmentTypeDefault)));

        HeroController.instance.checkEnvironment();
    }
}
