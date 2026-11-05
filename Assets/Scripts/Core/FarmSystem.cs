using System;
using System.Collections.Generic;

[Serializable]
public class FarmPlotState
{
    public int plotId;
    public string cropId;
    public int daysGrowing;
    public bool ready;
}

public static class FarmSystem
{
    public const string CarrotId = "carrot";

    public static List<FarmPlotState> CreateStartingPlots()
    {
        var plots = new List<FarmPlotState>();
        for (int i = 0; i < EconomyRules.StartingPlotCount; i++)
            plots.Add(new FarmPlotState { plotId = i });
        for (int i = 0; i < 2 && i < plots.Count; i++)
        {
            plots[i].cropId = CarrotId;
            plots[i].daysGrowing = 1;
        }
        return plots;
    }

    public static void EnsurePlots(GameState state)
    {
        if (state.farmPlots == null) state.farmPlots = CreateStartingPlots();
        while (state.farmPlots.Count < EconomyRules.StartingPlotCount)
            state.farmPlots.Add(new FarmPlotState { plotId = state.farmPlots.Count });
    }

    public static void GrowthTick(GameState state, CropSO[] crops = null)
    {
        EnsurePlots(state);
        foreach (var plot in state.farmPlots)
        {
            if (plot == null || string.IsNullOrEmpty(plot.cropId) || plot.ready) continue;
            plot.daysGrowing++;
            if (plot.daysGrowing >= GrowDays(plot.cropId, crops)) plot.ready = true;
        }
    }

    public static bool Plant(GameState state, int plotId, CropSO crop = null)
    {
        EnsurePlots(state);
        var plot = Find(state, plotId);
        if (plot == null || !string.IsNullOrEmpty(plot.cropId) ||
            state.actionPoints < EconomyRules.PlantActionCost) return false;
        state.actionPoints -= EconomyRules.PlantActionCost;
        plot.cropId = crop ? crop.id : CarrotId;
        plot.daysGrowing = 0;
        plot.ready = false;
        return true;
    }

    public static bool Harvest(GameState state, int plotId, CropSO[] crops = null)
    {
        EnsurePlots(state);
        var plot = Find(state, plotId);
        if (plot == null || !plot.ready || state.actionPoints < EconomyRules.HarvestActionCost) return false;
        state.actionPoints -= EconomyRules.HarvestActionCost;
        var crop = FindCrop(plot.cropId, crops);
        state.stock += crop ? crop.harvestYield : EconomyRules.CarrotYield;
        plot.cropId = null;
        plot.daysGrowing = 0;
        plot.ready = false;
        return true;
    }

    static FarmPlotState Find(GameState state, int id) => state.farmPlots.Find(p => p != null && p.plotId == id);

    static int GrowDays(string cropId, CropSO[] crops)
    {
        var crop = FindCrop(cropId, crops);
        return crop ? Math.Max(1, crop.growDays) : EconomyRules.CarrotGrowDays;
    }

    static CropSO FindCrop(string id, CropSO[] crops) => crops == null ? null : Array.Find(crops, c => c && c.id == id);
}
