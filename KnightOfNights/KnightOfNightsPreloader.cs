using ItemChanger;
using KnightOfNights.Scripts.InternalLib;
using PurenailCore.ModUtil;
using UnityEngine;

namespace KnightOfNights;

internal class KnightOfNightsPreloader : Preloader
{
    public static readonly KnightOfNightsPreloader Instance = new();

    [Preload(SceneNames.Mines_11, "Crystal Crawler")]
    public GameObject? CrystalCrawler { get; private set; }

    [ResourcePreload("dream_enter_pt_2")]
    public AudioClip? DreamEnterClip { get; private set; }

    [Preload(SceneNames.Fungus2_32, "Ring Holder/1")]
    public GameObject? ElderHuPancake { get; private set; }

    [PrefabPreload(SceneNames.Fungus2_32, "Elder_Hu_Ring_Appear")]
    public AudioClip? ElderHuRingClip { get; private set; }

    [Preload(SceneNames.Deepnest_40, "Warrior/Galien Hammer")]
    public GameObject? GalienAxe { get; private set; }

    [PrefabPreload(SceneNames.Deepnest_40, "Galien Mini Hammer")]
    public GameObject? GalienMiniAxe { get; private set; }

    [Preload(SceneNames.Crossroads_13, "_Enemies/Worm")]
    public GameObject? Goam { get; private set; }

    [PrefabPreload(SceneNames.Fungus1_35, "Shot Slug Spear")]
    public GameObject? GorbSpear { get; private set; }

    [PrefabPreload(SceneNames.Room_Colosseum_Gold, "mage_knight_projectile_shoot")]
    public AudioClip? MageShotClip { get; private set; }

    [PrefabPreload(SceneNames.Tutorial_01, "mage_knight_teleport")]
    public AudioClip? MageTeleportClip { get; private set; }

    [PrefabPreload(SceneNames.Deepnest_East_10, "Shot Markoth Nail")]
    public GameObject? MarkothNail { get; private set; }

    [PrefabPreload(SceneNames.Deepnest_East_10, "Markoth Shield/Shield")]
    public GameObject? MarkothShield { get; private set; }

    [PrefabPreload(SceneNames.Fungus1_35, "No Eyes Head")]
    public GameObject? NoEyesHead { get; private set; }

    [Preload(SceneNames.Town, "_Managers/PlayMaker Unity 2D")]
    public GameObject? PlayMaker { get; private set; }

    [Preload(SceneNames.RestingGrounds_08, "Ghost Battle Revek")]
    public GameObject? Revek { get; private set; }

    [Preload(SceneNames.Tutorial_01, "_Scenery/plat_float_07")]
    public GameObject? SmallPlatform { get; private set; }

    [ResourcePreload("Stun Effect")]
    public GameObject? StunEffect { get; private set; }

    [Preload(SceneNames.RestingGrounds_02_boss, "Warrior/Ghost Warrior Xero")]
    public GameObject? Xero { get; private set; }

    [Preload(SceneNames.RestingGrounds_02_boss, "Warrior/Ghost Warrior Xero/Sword 1")]
    public GameObject? XeroNail { get; private set; }

    public PhysicsMaterial2D? TerrainMaterial => SmallPlatform?.GetComponent<Collider2D>()?.sharedMaterial;
}
