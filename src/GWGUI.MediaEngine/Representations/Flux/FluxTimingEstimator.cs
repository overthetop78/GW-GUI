namespace GWGUI.MediaEngine.Representations.Flux;

/// <summary>Estime la durée des cellules de bits à partir des intervalles de transitions de flux.</summary>
internal static class FluxTimingEstimator
{
    /// <summary>Estime la durée d'une cellule à partir de la distribution propre au codage FM.</summary>
    /// <param name="intervals">Intervalles de flux exprimés en ticks.</param>
    /// <returns>Durée estimée d'une cellule, en ticks.</returns>
    public static double EstimateFmBitCell(IReadOnlyList<uint> intervals) => EstimateBitCell(intervals, FluxTimingMode.Fm);

    /// <summary>Estime la durée d'une cellule sans appliquer la distribution propre au codage FM.</summary>
    /// <param name="intervals">Intervalles de flux exprimés en ticks.</param>
    /// <returns>Durée estimée d'une cellule, en ticks.</returns>
    public static double EstimateNonFmBitCell(IReadOnlyList<uint> intervals) => EstimateBitCell(intervals, FluxTimingMode.NonFm);

    /// <summary>Estime la durée d'une cellule FM ou MFM depuis les intervalles observés.</summary>
    /// <param name="intervals">Intervalles de flux exprimés en ticks.</param>
    /// <param name="fm">Indique si l'estimation doit appliquer la distribution FM.</param>
    /// <returns>Durée estimée d'une cellule, en ticks.</returns>
    public static double EstimateBitCell(IReadOnlyList<uint> intervals, FluxTimingMode mode)
    {
        if (intervals.Count == 0) return FluxDecodingParameters.FallbackBitCellTicks;
        var samples = mode == FluxTimingMode.Fm ? intervals : intervals.Skip(1);
        var sorted = samples.Where(x => x > 0).Order().ToArray(); if (sorted.Length == 0) sorted = intervals.Where(x => x > 0).Order().ToArray(); if (sorted.Length == 0) return FluxDecodingParameters.FallbackBitCellTicks;
        if (mode == FluxTimingMode.Fm)
        {
            return Math.Max(FluxDecodingParameters.MinimumBitCellTicks, SelectLowPercentile(sorted));
        }
        var robustLower = SelectLowerClusterMedian(sorted);
        return Math.Max(1, robustLower / FluxDecodingParameters.RobustIntervalToBitCellDivisor);
    }

    /// <summary>Estime la durée d'une cellule NRZI depuis les intervalles observés.</summary>
    /// <param name="intervals">Intervalles de flux exprimés en ticks.</param>
    /// <returns>Durée estimée d'une cellule NRZI, en ticks.</returns>
    public static double EstimateNrziBitCell(IReadOnlyList<uint> intervals)
    {
        if (intervals.Count == 0) return FluxDecodingParameters.FallbackBitCellTicks;
        var sorted = intervals.Skip(1).Where(value => value > 0).Order().ToArray();
        if (sorted.Length == 0) sorted = intervals.Where(value => value > 0).Order().ToArray();
        if (sorted.Length == 0) return FluxDecodingParameters.FallbackBitCellTicks;
        var percentile = Math.Clamp(sorted.Length / 50, 0, sorted.Length - 1);
        return Math.Max(1, sorted[percentile]);
    }

    /// <summary>Sélectionne un intervalle du percentile bas afin que les transitions FM les plus courtes déterminent la cellule et évitent un verrouillage sur deux cellules.</summary>
    /// <param name="sorted">Intervalles strictement positifs classés par ordre croissant.</param>
    /// <returns>Intervalle représentatif du percentile bas.</returns>
    private static uint SelectLowPercentile(IReadOnlyList<uint> sorted)
    {
        var percentile = Math.Min(sorted.Count / FluxDecodingParameters.LowPercentileDivisor, sorted.Count - 1);
        return sorted[percentile];
    }

    private static uint SelectLowerClusterMedian(IReadOnlyList<uint> sorted)
    {
        var sampleLength = Math.Max(1, sorted.Count / FluxDecodingParameters.LowerClusterDivisor);
        var lowerCluster = sorted.Take(sampleLength).ToArray();
        return lowerCluster[lowerCluster.Length / FluxDecodingParameters.MedianDivisor];
    }
}
