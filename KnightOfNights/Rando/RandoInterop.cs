using ItemChanger;
using KnightOfNights.IC;
using Modding;
using Newtonsoft.Json;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerMod.Logging;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using RandoPlus.GhostEssence;
using System;
using System.IO;

namespace KnightOfNights.Rando;

internal static class RandoInterop
{
    internal static void Setup()
    {
        Finder.DefineCustomItem(new RevekSongItem());

        ConnectionMenu.Setup();
        RCData.RuntimeLogicOverride.Subscribe(0f, DefineRefs);
        // Needs to be after GhostEssence, currently at 0.5f.
        // https://github.com/flibber-hk/HollowKnight.RandoPlus/blob/77b6a452380f9f4f453cfc30480f6d5eaab6e689/RandoPlus/GhostEssence/RequestMaker.cs#L18
        RequestBuilder.OnUpdate.Subscribe(1f, MaybeAddRevekSong);
        RandoController.OnCalculateHash += ModifyHash;
        RandoController.OnExportCompleted += OnExportCompleted;
        SettingsLog.AfterLogSettings += LogKnightOfNightsSettings;
    }

    private static void DefineRefs(GenerationSettings gs, LogicManagerBuilder lmb)
    {
        if (!KnightOfNightsMod.RS.IsEnabled) return;

        lmb.AddItem(new EmptyItem(RevekSongItem.ITEM_NAME));
    }

    private static void MaybeAddRevekSong(RequestBuilder rb)
    {
        rb.EditItemRequest(RevekSongItem.ITEM_NAME, info =>
        {
            info.getItemDef = () => new()
            {
                Name = RevekSongItem.ITEM_NAME,
                Pool = PoolNames.Skill,
                MajorItem = false,
                PriceCap = 1000
            };
        });

        switch (KnightOfNightsMod.RS.RevekSong)
        {
            case RevekSongRandoMode.Start:
                rb.AddToStart(RevekSongItem.ITEM_NAME);
                break;
            case RevekSongRandoMode.Vanilla:
                if (ModHooks.GetMod("RandoPlus") is not Mod) throw new ArgumentException("Must install RandoPlus to place RevekSong at VANILLA");
                rb.RemoveLocationByName(GhostNames.Ghost_Essence_Revek);
                break;
            case RevekSongRandoMode.Randomized:
                rb.AddItemByName(RevekSongItem.ITEM_NAME);
                break;
            case RevekSongRandoMode.Disabled:
                break;
        }
    }

    private static void OnExportCompleted(RandoController rc)
    {
        if (!KnightOfNightsMod.RS.IsEnabled) return;

        ItemChangerMod.Modules.GetOrAdd<RevekSongModule>();

        if (KnightOfNightsMod.RS.RevekSong == RevekSongRandoMode.Vanilla)
        {
            var placement = Finder.GetLocation(GhostNames.Ghost_Essence_Revek)!.Wrap();
            placement.Add(Finder.GetItem(RevekSongItem.ITEM_NAME)!);
            ItemChangerMod.AddPlacements([placement]);
        }
    }

    private static int ModifyHash(RandoController rc, int hash) => KnightOfNightsMod.RS.RevekSong == RevekSongRandoMode.Vanilla ? 666 : 0;

    private static void LogKnightOfNightsSettings(LogArguments args, TextWriter tw)
    {
        tw.WriteLine("Knight of Nights Settings:");

        using JsonTextWriter jtw = new(tw) { CloseOutput = false };
        RandomizerCore.Json.JsonUtil.GetNonLogicSerializer().Serialize(jtw, KnightOfNightsMod.RS);
        tw.WriteLine();
    }
}
