using ItemChanger;
using ItemChanger.Extensions;
using PurenailCore.ModUtil;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace KnightOfNights;

internal class KnightOfNightsPreloader : Preloader
{
    public static readonly KnightOfNightsPreloader Instance = new();

    [ResourcePreload("Arrow Prompt")]
    public GameObject? ArrowPrompt { get; private set; }

    [Preload(SceneNames.Mines_11, "Crystal Crawler")]
    public GameObject? CrystalCrawler { get; private set; }

    [Preload(SceneNames.Ruins1_28, "Direction Pole Stag")]
    public GameObject? DirectionPoleStag { get; private set; }

    [ResourcePreload("dream_enter_pt_2")]
    public AudioClip? DreamEnterClip { get; private set; }

    [PrefabPreload(SceneNames.Fungus1_35, "DreamFight")]
    public MusicCue? DreamFightMusic { get; private set; }

    [Preload(SceneNames.Fungus2_32, "Ring Holder/1")]
    public GameObject? ElderHuPancake { get; private set; }

    [PrefabPreload(SceneNames.Fungus2_32, "Elder_Hu_Ring_Impact")]
    public AudioClip? ElderHuImpactClip { get; private set; }

    [Preload(SceneNames.Deepnest_40, "Warrior/Galien Hammer")]
    public GameObject? GalienAxe { get; private set; }

    [PrefabPreload(SceneNames.Deepnest_40, "Galien Mini Hammer")]
    public GameObject? GalienMiniAxe { get; private set; }

    [Preload(SceneNames.Fungus1_16_alt, "green_grass_3")]
    public GameObject? Grass { get; private set; }

    [Preload(SceneNames.Crossroads_13, "_Enemies/Worm")]
    public GameObject? Goam { get; private set; }

    [PrefabPreload(SceneNames.Fungus1_35, "Shot Slug Spear")]
    public GameObject? GorbSpear { get; private set; }

    [PrefabPreload(SceneNames.RestingGrounds_08, "hornet_parry_prepare")]
    public AudioClip? HornetParryClip { get; private set; }

    [PrefabPreload(SceneNames.Room_Colosseum_Gold, "mage_knight_projectile_shoot")]
    public AudioClip? MageShotClip { get; private set; }

    [PrefabPreload(SceneNames.Ruins1_24_boss, "mage_lord_strike_impact")]
    public AudioClip? MageStrikeImpactClip {  get; private set; }

    [PrefabPreload(SceneNames.Tutorial_01, "mage_knight_teleport")]
    public AudioClip? MageTeleportClip { get; private set; }

    [PrefabPreload(SceneNames.Deepnest_East_10, "Shot Markoth Nail")]
    public GameObject? MarkothNail { get; private set; }

    [PrefabPreload(SceneNames.Deepnest_East_10, "Markoth Shield")]
    public GameObject? MarkothShieldParent { get; private set; }

    public GameObject? MarkothShield => MarkothShieldParent?.FindChild("Shield");

    public PlayMakerFSM? NailClashTinkFSM => NailSlash?.LocateMyFSM("nail_clash_tink");

    [Preload(SceneNames.Ruins2_04, "Great Shield Zombie/Slash1")]
    public GameObject? NailSlash { get; private set; }

    [PrefabPreload(SceneNames.Fungus1_35, "No Eyes Head")]
    public GameObject? NoEyesHead { get; private set; }

    [ResourcePreload("Normal")]
    public AudioMixerSnapshot? NormalSnapshot { get; private set; }

    [Preload(SceneNames.Town, "_Managers/PlayMaker Unity 2D")]
    public GameObject? PlayMaker { get; private set; }

    [Preload(SceneNames.RestingGrounds_08, "Ghost Battle Revek")]
    public GameObject? Revek { get; private set; }

    [PrefabPreload(SceneNames.Room_Colosseum_02, "Col_miner_attack_02")]
    public AudioClip? RevekAttackClip1 { get; private set; }

    [PrefabPreload(SceneNames.Room_Colosseum_02, "Col_miner_attack_03")]
    public AudioClip? RevekAttackClip2 { get; private set; }

    [PrefabPreload(SceneNames.Room_Colosseum_02, "Col_miner_attack_04")]
    public AudioClip? RevekAttackClip3 { get; private set; }

    public List<AudioClip> RevekAttackClips => [RevekAttackClip1!, RevekAttackClip2!, RevekAttackClip3!];

    [PrefabPreload(SceneNames.Crossroads_10_boss, "Shockwave Wave")]
    public GameObject? Shockwave { get; private set; }

    [PrefabPreload(SceneNames.RestingGrounds_08, "brkn_wand_horizontal_dash_dash")]
    public AudioClip? SlashAttackClip { get; private set; }

    [Preload(SceneNames.Tutorial_01, "_Scenery/plat_float_07")]
    public GameObject? SmallPlatform { get; private set; }

    [Preload(SceneNames.Fungus2_10, "Soul Totem mini_horned")]
    public GameObject? SoulTotem { get; private set; }

    [ResourcePreload("Stun Effect")]
    public GameObject? StunEffect { get; private set; }

    [PrefabPreload(SceneNames.Fungus3_23_boss, "mega_mantis_tall_slash")]
    public GameObject? TraitorLordWave { get; private set; }

    [Preload(SceneNames.RestingGrounds_02_boss, "Warrior/Ghost Warrior Xero")]
    public GameObject? Xero { get; private set; }

    [Preload(SceneNames.RestingGrounds_02_boss, "Warrior/Ghost Warrior Xero/Sword 1")]
    public GameObject? XeroNail { get; private set; }

    public PhysicsMaterial2D? TerrainMaterial => SmallPlatform?.GetComponent<Collider2D>()?.sharedMaterial;
}
