namespace GWGUI.MediaEngine.Representations.Flux;

/// <summary>Estime la durée des cellules de bits à partir des intervalles de transitions de flux.</summary>
internal static class FluxTimingEstimator
{
    /// <summary>Estime la durée d'une cellule FM ou MFM depuis les intervalles observés.</summary>
    /// <param name="intervals">Intervalles de flux exprimés en ticks.</param>
    /// <param name="fm">Indique si l'estimation doit appliquer la distribution FM.</param>
    /// <returns>Durée estimée d'une cellule, en ticks.</returns>
    public static double EstimateBitCell(IReadOnlyList<uint> intervals, bool fm = false)
    {
        if (intervals.Count == 0) return 1;
        var samples = fm ? intervals : intervals.Skip(1);
        var sorted = samples.Where(x => x > 0).Order().ToArray(); if (sorted.Length == 0) sorted = intervals.Where(x => x > 0).Order().ToArray(); if (sorted.Length == 0) return 1;
        if (fm)
        {
            var percentile = Math.Clamp(sorted.Length / 50, 0, sorted.Length - 1);
            return Math.Max(1, sorted[percentile]);
        }
        var sampleLength = Math.Max(1, sorted.Length / 5); var lowerCluster = sorted.Take(sampleLength).ToArray(); var robustLower = lowerCluster[lowerCluster.Length / 2];
        return Math.Max(1, robustLower / 2d);
    }

    /// <summary>Estime la durée d'une cellule NRZI depuis les intervalles observés.</summary>
    /// <param name="intervals">Intervalles de flux exprimés en ticks.</param>
    /// <returns>Durée estimée d'une cellule NRZI, en ticks.</returns>
    public static double EstimateNrziBitCell(IReadOnlyList<uint> intervals)
    {
        if (intervals.Count == 0) return 1;
        var sorted = intervals.Skip(1).Where(value => value > 0).Order().ToArray();
        if (sorted.Length == 0) sorted = intervals.Where(value => value > 0).Order().ToArray();
        if (sorted.Length == 0) return 1;
        var percentile = Math.Clamp(sorted.Length / 50, 0, sorted.Length - 1);
        return Math.Max(1, sorted[percentile]);
    }
}
