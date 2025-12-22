using Benchwarp;
using KnightOfNights.Scripts.SharedLib;
using KnightOfNights.Scripts.SharedLib.Data;
using PurenailCore.CollectionUtil;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.IC;

[PlandoSubmodule]
internal class BenchesModule : AbstractModule<BenchesModule>
{
    public Dictionary<string, HashSet<string>> VisitedCustomBenches = [];

    private readonly HashMultimap<string, string> MaybeVisitedCustomBenches = [];
    private readonly List<Bench> CustomBenches = [];

    internal void RevealBenches()
    {
        bool changed = false;
        foreach (var area in MaybeVisitedCustomBenches.Keys)
        {
            var names = VisitedCustomBenches.GetOrAddNew(area);
            foreach (var name in MaybeVisitedCustomBenches.Get(area)) changed |= names.Add(name);
        }

        if (changed) UpdateBenchwarp();
    }

    protected override void InitializeInternal() => SceneDataModule.GetDeferred().Do(InitializeInternal);

    private void InitializeInternal(SceneDataModule sceneData)
    {
        foreach (var (scene, data) in sceneData.GetForAllScenes<CustomBenchData>())
        {
            MaybeVisitedCustomBenches.Add(data.AreaName, data.MenuName);
            CustomBenches.Add(new(
                data.MenuName,
                data.AreaName,
                scene,
                data.RespawnMarkerName,
                1,
                GlobalEnums.MapZone.PEAK,
                "",
                Vector3.zero));
        }
        CustomBenches.SortBy(b => b.name);

        Events.BenchSuppressors += HideWhitePalaceBenches;
        Events.BenchSuppressors += HideUnvisitedBenches;
        Events.BenchInjectors += InjectBenches;
    }

    protected override void UnloadInternal()
    {
        Events.BenchSuppressors -= HideWhitePalaceBenches;
        Events.BenchSuppressors -= HideUnvisitedBenches;
        Events.BenchInjectors -= InjectBenches;
    }

    private bool HideWhitePalaceBenches(Bench bench) => bench.areaName == "Palace";

    private bool HideUnvisitedBenches(Bench bench)
    {
        if (!MaybeVisitedCustomBenches.Contains(bench.areaName, bench.name)) return false;
        if (!VisitedCustomBenches.TryGetValue(bench.areaName, out var benches)) return true;
        return !benches.Contains(bench.name);
    }

    private IEnumerable<Bench> InjectBenches() => CustomBenches;

    internal void VisitBench(string areaName, string menuName)
    {
        if (!VisitedCustomBenches.GetOrAddNew(areaName).Add(menuName)) return;
        UpdateBenchwarp();
    }

    private void UpdateBenchwarp()
    {
        Bench.RefreshBenchList();
        TopMenu.RebuildMenu();
    }

    protected override BenchesModule Self() => this;
}
