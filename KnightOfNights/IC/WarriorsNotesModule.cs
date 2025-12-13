using ItemChanger;
using Modding;
using SFCore;
using System.Linq;

namespace KnightOfNights.IC;

[PlandoSubmodule]
internal class WarriorsNotesModule : AbstractModule<WarriorsNotesModule>
{
    private const string NAME_KEY = "INV_WARRIORS_NOTES_NAME";
    private const string DESC_KEY = "INV_WARRIORS_NOTES_DESC";

    internal static void HookItemHelper()
    {
        EmbeddedSprite sprite = new("notes");
        ItemHelper.AddNormalItem(sprite.Value, nameof(HasWarriorsNotes), NAME_KEY, DESC_KEY);
    }

    public bool HasWarriorsNotes;

    protected override WarriorsNotesModule Self() => this;

    private static void FillName(ref string value) => value = "Warriors Notes";

    private static bool HasWings => PlayerData.instance.GetBool(nameof(PlayerData.hasDoubleJump));

    private static bool AcquiredLoc(string locName)
    {
        if (!ItemChanger.Internal.Ref.Settings.Placements.TryGetValue(locName, out var p)) return false;
        return p.Items.Any(i => i.WasEverObtained());
    }

    private static string FillDesc(string sceneName)
    {
        if (sceneName == SceneNames.Abyss_05)
            return "What a silly place, such noise and unnecessary extravagance. I don't think I shall visit it this day.";
        if (sceneName == SceneNames.Abyss_20 && !HasWings && !AcquiredLoc(LocationNames.Monarch_Wings) && !AcquiredLoc(LocationNames.Boss_Essence_Lost_Kin))
            return "I do not think I even need the assistance here, if I keep the fat one alive. If I dispatch them though, I can time my ascent from the platform with the crawlid upon the wall.";
        if (sceneName == SceneNames.Crossroads_04 && !HasWings && !AcquiredLoc(LocationNames.Rancid_Egg_Blue_Lake))
            return "It is close, but I cannot reach the alcove from the charm keeper's shop. Perhaps from a higher vantage point, with my dash extended, I might reach it still.";
        if (sceneName == SceneNames.Crossroads_10 && !HasWings && !AcquiredLoc(LocationNames.Boss_Essence_Failed_Champion))
            return "I certainly cannot reach the ritual chamber from the right, and it appears just out of reach on the left. Perhaps with the bubble? I must strike it last, and keep my nail short to not burst it too soon.";
        if (sceneName == SceneNames.Crossroads_45)
            return "I have traversed high and low but I cannot find the heart, I am not sure it exists here. Her song shall reign on.";
        if (sceneName == SceneNames.Crossroads_50 && !HasWings && !AcquiredLoc(LocationNames.Rancid_Egg_Blue_Lake))
            return "The ledge is not so high, and the ceiling is low. I should descend from the high ground before I begin to parry, to more safely reach the alcove of the egg.";
        if (sceneName == SceneNames.Deepnest_38 && !HasWings && !AcquiredLoc(LocationNames.Soul_Totem_Deepnest_Vessel))
            return "This is a cruel test, spikes boundless and it is excruciating to time my aide while bouncing on the tunnelers and bottom-feeders below. Perhaps if I hover on the wall, using the left spikes, I can maintain height and better time my movement with the warrior. I mustn't parry thrice 'ere the first wall, I need them again to cross the second.";
        if (sceneName == SceneNames.Fungus1_09 && HasWings && !AcquiredLoc(LocationNames.Great_Slash))
            return "This is a mighty challenge, but I believe it feasible. I musn't parry thrice too soon, for there are multiple walls to cross. I must be precise, and use my soul to correct for the unfavorable cycles.";
        if (sceneName == SceneNames.Fungus1_13 && !HasWings)
        {
            if (!AcquiredLoc(LocationNames.Vessel_Fragment_Greenpath)) return "With light sacrifice, I can reach the last ledge, but the ascent beyond is slightly too high. Perhaps if I can coordinate with the mossy denizen on the wall, I can get enough extra height.";
            else if (!AcquiredLoc(LocationNames.Whispering_Root_Greenpath)) return "I must plan this carefully, I cannot use the warp so freely when I must first activate the root. I will surely need the mossy one's aide again.";
        }
        if (sceneName == SceneNames.Fungus1_35 && !PlayerData.instance.hasLantern)
            return "How you taunt me, sitting here, waiting. After I scour the ends of the earth to reveal you, your death shall be all the sweeter.";
        if (sceneName == SceneNames.Fungus2_01 && !HasWings && !AcquiredLoc(LocationNames.Mask_Shard_Queens_Station))
            return "If I hug the wall with dashes, I can parry thrice to achieve enough height. If I can only reach the lever from below, I can then enter from the outside...";
        if (sceneName == SceneNames.Fungus2_04 && HasWings && !AcquiredLoc(LocationNames.Wanderers_Journal_Fungal_Wastes_Thorns_Gauntlet))
            return "The thorns here are sparse, they do not quite eclipse the left corner. I think I need not that ground though, the platform is high enough for this climb.";
        if (sceneName == SceneNames.Fungus2_15 && !HasWings && !AcquiredLoc(LocationNames.Lifeblood_Cocoon_Mantis_Village))
            return "The platforms along the right wall give me height enough, I need only distance. The arena is not so wide to cross, I can surely reach the hidden alcove.";
        if (sceneName == SceneNames.Fungus2_21 && PlayerData.instance.hasCityKey)
            return "The thorns are quite low, I will have to fly close to the moat to avoid them. It is a short drop though, I can time it freely.";
        if (sceneName == SceneNames.Fungus2_23 && HasWings && !PlayerData.instance.brettaRescued)
            return "The ledge is so high, even with flight I cannot reach it. Perhaps the fungi can assist me with a starting boost; it helps that their attack patterns are quite predictable.";
        if (sceneName == SceneNames.Fungus3_21 && !HasWings && !AcquiredLoc(LocationNames.Whispering_Root_Greenpath))
            return "Accursed platforms, the leftmost is just slightly too high. I must time my descent carefully, I will need to parry before and after that accursed platform within the same sequence. The dash extender certainly does not help me here.";
        if (sceneName == SceneNames.Fungus3_22 && !HasWings && !AcquiredLoc(LocationNames.Whispering_Root_Greenpath))
            return "It is not difficult, but it is grueling, the traitorous fliers are a grand nuisance. I should dispatch them before making the final ascent.";
        if (sceneName == SceneNames.Fungus3_39 && !AcquiredLoc(LocationNames.Love_Key))
            return "It is an extraordinarily tight fit, but the ledge is close and so the drop is easy to time. I think extending my dash makes it somewhat easier.";
        if (sceneName == SceneNames.Fungus3_archive_02 && HasWings && !AcquiredLoc(LocationNames.Monomon))
            return "I cannot reach her from the right, the sea is too broad. Perhaps with careful timing from beneath the charged aquarium, I can attain enough height from the left side...";
        if (sceneName == SceneNames.GG_Lurker && HasWings && !AcquiredLoc(LocationNames.Simple_Key_Lurker))
            return "He is a coward, but I can reach him, and I can chase him. I need only maximize my height to and within the arena.";
        if (sceneName == SceneNames.Ruins2_01 && !HasWings)
            return "With the fliers, it may be possible, but their movement is so erratic. There must be another way to the queen's side.";
        if (sceneName == SceneNames.Ruins2_03 && HasWings && !AcquiredLoc(LocationNames.Geo_Chest_Watcher_Knights))
            return "It is just barely possible from the platform, I need only maximize my flight. Was it architected for this purpose all along?";
        if (sceneName == SummitSceneNames.Summit_BigWindClimb)
            return "I feel the air growing thin already, how much higher does this go? The wind is softer here, but the mountainside... it is not uniform at all, every ledge necessitates a different approach. This tests my mind as much as my mettle. After the fourth ledge, I must fly with the wind through the sluice, and coast to the alcove after.<br><br>The slope beyond... it's softer, I must be close to the summit. But how am I to ascend? There is no ledge, no floor, no wall, nothing to aid my climb. Am I meant to conquer the air itself?";
        if (sceneName == SummitSceneNames.Summit_EntryHall)
            return "So it begins. Warriors be with me, it will take much to reach the peak...";
        if (sceneName == SummitSceneNames.Summit_EntryPlain)
            return "The entryway tests my horizontal. I should use the wings, dash, then dash again immediately after each parry into another flap of the wings.";
        if (sceneName == SummitSceneNames.Summit_SpikeTunnels)
            return "More tunnels... more precise than the last. The first requires me to dash to and fro; I should summon the guardian in alternating strikes to facilitate this.<br><br>The second is a cruel challenge, there is not room enough to parry downwards freely. I could enter the tunnel early, stall, then my dash would be recharged for instant use after the first parry...";
        if (sceneName == SummitSceneNames.Summit_Tunnels)
            return "The tunnels test my vertical quite treacherously. I need to maximize my jumps from floor and wall alike before ascending through the first two tunnels.<br><br>For the third, I must be more resourceful. Three parries is not enough, I must find a way to forestall my flight after the second so I can ascend anew past the pincer.";
        if (sceneName == SummitSceneNames.Summit_WindCliffs)
            return "As the entryway, I must maximize my horizontal to cross this impossible mountain. I can stall past the peak and let the wind carry me to the ledge.<br><br>From the basin the only way forward is up, a treacherous climb against the wind... I must dash westwards at every opportunity to reach the alcove above.";
        if (sceneName == SceneNames.Town && HasWings)
            return "It is so tantalizing, but I think I cannot reach the great door; perhaps there is another way around. The lever I think I can reach, but even if I did, surely I cannot pass the crystal sea.";
        if (sceneName == SceneNames.Waterways_06 && !HasWings && !AcquiredLoc(LocationNames.Rancid_Egg_Waterways_Main))
            return "The sea is not so wide, I can cross it easily. Though I fear I will not be able to reach much on the other side, the tunnels above are quite tall.";
        if (sceneName == SceneNames.Waterways_07 && !HasWings && !AcquiredLoc(LocationNames.Rancid_Egg_Waterways_East))
            return "I have quite some time to drop towards the lower pool after I make the call, and the far ledge is not so high. I think I can at least reach the egg, I am not so sure of anything else.";

        return "";
    }

    private static void FillDesc(ref string value)
    {
        var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        var header = "A strange scroll awash with the glyphs of higher beings. They shift and change as the parchment moves within your grasp.";

        var desc = FillDesc(sceneName);
        if (desc.Length == 0) desc = "The notes appear indecipherable in this area, dissonant and frivolous. Perhaps in another room they will conspire to create meaning.";
        else desc = $"\"{desc}\"";

        value = $"{header}<br><br>{desc}";
    }

    private bool HookHasWarriorsNotes(string name, bool orig) => name == nameof(HasWarriorsNotes) ? HasWarriorsNotes : orig;

    public override void Initialize()
    {
        base.Initialize();

        Events.AddLanguageEdit(new(NAME_KEY), FillName);
        Events.AddLanguageEdit(new(DESC_KEY), FillDesc);
        ModHooks.GetPlayerBoolHook += HookHasWarriorsNotes;
    }

    public override void Unload()
    {
        Events.RemoveLanguageEdit(new(NAME_KEY), FillName);
        Events.RemoveLanguageEdit(new(DESC_KEY), FillDesc);
        ModHooks.GetPlayerBoolHook -= HookHasWarriorsNotes;

        base.Unload();
    }
}
