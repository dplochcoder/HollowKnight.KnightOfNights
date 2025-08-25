using ItemChanger;
using PurenailCore.ModUtil;
using UnityEngine;

namespace KnightOfNights;

internal class KnightOfNightsPreloader : Preloader
{
    public static readonly KnightOfNightsPreloader Instance = new();

    [Preload(SceneNames.Mines_11, "Crystal Crawler")]
    public GameObject? CrystalCrawler { get; private set; }

    [Preload(SceneNames.Crossroads_13, "_Enemies/Worm")]
    public GameObject? Goam { get; private set; }

    [Preload(SceneNames.Town, "_Managers/PlayMaker Unity 2D")]
    public GameObject? PlayMaker { get; private set; }

    [Preload(SceneNames.Tutorial_01, "_Scenery/plat_float_07")]
    public GameObject? SmallPlatform { get; private set; }

    public PhysicsMaterial2D? TerrainMaterial => SmallPlatform?.GetComponent<Collider2D>()?.sharedMaterial;
}
