namespace GWGUI.MediaEngine.Representations.Flux;

/// <summary>Reconstruit les bits à partir des intervalles séparant les transitions de flux.</summary>
internal static class FluxTransitionDecoder
{
    /// <summary>Reconstruit un flux FM ou MFM avec estimation et adaptation de l'horloge.</summary>
    /// <param name="intervals">Intervalles de flux exprimés en ticks.</param>
    /// <param name="fm">Indique si l'estimation doit utiliser la distribution FM.</param>
    /// <returns>Flux de bits reconstruit.</returns>
    public static FluxBitstream DecodeAdaptiveFm(IReadOnlyList<uint> intervals)
    {
        ArgumentNullException.ThrowIfNull(intervals);
        return Reconstruct(intervals, FluxTimingEstimator.EstimateFmBitCell(intervals), 32);
    }

    /// <summary>Reconstruit un flux MFM avec estimation et adaptation de l'horloge.</summary>
    /// <param name="intervals">Intervalles de flux exprimés en ticks.</param>
    /// <returns>Flux de bits reconstruit.</returns>
    public static FluxBitstream DecodeAdaptiveMfm(IReadOnlyList<uint> intervals)
    {
        ArgumentNullException.ThrowIfNull(intervals);
        return Reconstruct(intervals, FluxTimingEstimator.EstimateNonFmBitCell(intervals), 32);
    }

    /// <summary>Reconstruit un flux FM ou MFM avec la PLL et une durée de cellule estimée.</summary>
    /// <param name="intervals">Intervalles de flux exprimés en ticks.</param>
    /// <param name="fm">Indique si l'estimation doit utiliser la distribution FM.</param>
    /// <returns>Flux de bits reconstruit.</returns>
    public static FluxBitstream DecodePllFm(IReadOnlyList<uint> intervals)
    {
        ArgumentNullException.ThrowIfNull(intervals);
        return ReconstructPll(intervals, FluxTimingEstimator.EstimateFmBitCell(intervals), 32);
    }

    /// <summary>Reconstruit un flux MFM avec la PLL et une durée de cellule estimée.</summary>
    /// <param name="intervals">Intervalles de flux exprimés en ticks.</param>
    /// <returns>Flux de bits reconstruit.</returns>
    public static FluxBitstream DecodePllMfm(IReadOnlyList<uint> intervals)
    {
        ArgumentNullException.ThrowIfNull(intervals);
        return ReconstructPll(intervals, FluxTimingEstimator.EstimateNonFmBitCell(intervals), 32);
    }

    /// <summary>Reconstruit un flux FM ou MFM avec la PLL et la durée de cellule fournie.</summary>
    /// <param name="intervals">Intervalles de flux exprimés en ticks.</param>
    /// <param name="bitCellTicks">Durée de cellule fournie, en ticks.</param>
    /// <returns>Flux de bits reconstruit.</returns>
    public static FluxBitstream DecodePll(IReadOnlyList<uint> intervals, double bitCellTicks)
    {
        ArgumentNullException.ThrowIfNull(intervals);
        return ReconstructPll(intervals, Math.Max(FluxDecodingParameters.MinimumBitCellTicks, bitCellTicks), 32);
    }

    /// <summary>Reconstruit un flux NRZI avec estimation et adaptation de l'horloge.</summary>
    /// <param name="intervals">Intervalles de flux exprimés en ticks.</param>
    /// <returns>Flux de bits reconstruit.</returns>
    public static FluxBitstream DecodeNrzi(IReadOnlyList<uint> intervals)
    {
        ArgumentNullException.ThrowIfNull(intervals);
        return Reconstruct(intervals, FluxTimingEstimator.EstimateNrziBitCell(intervals), 64);
    }

    /// <summary>Reconstruit un flux NRZI avec la durée de cellule fournie et sans adaptation de l'horloge.</summary>
    /// <param name="intervals">Intervalles de flux exprimés en ticks.</param>
    /// <param name="bitCellTicks">Durée de cellule fournie, en ticks.</param>
    /// <returns>Flux de bits reconstruit.</returns>
    public static FluxBitstream DecodeNrzi(IReadOnlyList<uint> intervals, double bitCellTicks)
    {
        ArgumentNullException.ThrowIfNull(intervals);
        return Reconstruct(intervals, Math.Max(FluxDecodingParameters.MinimumBitCellTicks, bitCellTicks), 64, adaptClock: false);
    }

    /// <summary>Reconstruit un flux NRZI doublé avec estimation et adaptation de l'horloge.</summary>
    /// <param name="intervals">Intervalles de flux exprimés en ticks.</param>
    /// <returns>Flux de bits reconstruit.</returns>
    public static FluxBitstream DecodeAdaptiveDoubledNrzi(IReadOnlyList<uint> intervals)
    {
        ArgumentNullException.ThrowIfNull(intervals);
        return Reconstruct(intervals, FluxTimingEstimator.EstimateNonFmBitCell(intervals), 64);
    }

    /// <summary>Reconstruit un flux NRZI doublé avec la durée de cellule fournie et sans adaptation de l'horloge.</summary>
    /// <param name="intervals">Intervalles de flux exprimés en ticks.</param>
    /// <param name="bitCellTicks">Durée de cellule fournie, en ticks.</param>
    /// <returns>Flux de bits reconstruit.</returns>
    public static FluxBitstream DecodeDoubledNrzi(IReadOnlyList<uint> intervals, double bitCellTicks)
    {
        ArgumentNullException.ThrowIfNull(intervals);
        return Reconstruct(intervals, Math.Max(FluxDecodingParameters.MinimumBitCellTicks, bitCellTicks), 64, adaptClock: false);
    }

    /// <summary>Reconstruit les bits en quantifiant chaque intervalle avec une cellule éventuellement adaptée.</summary>
    /// <param name="intervals">Intervalles de flux exprimés en ticks.</param>
    /// <param name="initialCell">Durée initiale d'une cellule, en ticks.</param>
    /// <param name="maximumCells">Nombre maximal de cellules représentées par un intervalle.</param>
    /// <param name="adaptClock">Indique si la durée de cellule doit suivre les observations valides.</param>
    /// <returns>Bits reconstruits et durée moyenne de cellule.</returns>
    public static FluxBitstream Reconstruct(IReadOnlyList<uint> intervals, double initialCell, int maximumCells, bool adaptClock = true)
    {
        ArgumentNullException.ThrowIfNull(intervals);
        var currentCell = initialCell; var accumulatedCell = 0d; var samples = 0; var bits = new List<bool>(intervals.Count * 4);
        for (var index = 0; index < intervals.Count; index++)
        {
            var interval = intervals[index]; var cells = Math.Clamp((int)Math.Round(interval / currentCell), 1, maximumCells);
            for (var zero = 1; zero < cells; zero++) bits.Add(false); bits.Add(true);
            if (index == 0 || !adaptClock) continue;
            var observedCell = interval / (double)cells;
            if (observedCell >= currentCell * FluxDecodingParameters.MinimumAcceptedSampleRatio && observedCell <= currentCell * FluxDecodingParameters.MaximumAcceptedSampleRatio) currentCell += (observedCell - currentCell) * FluxDecodingParameters.ClockAdaptationCoefficient;
            accumulatedCell += currentCell; samples++;
        }
        return new(bits.ToArray(), samples == 0 ? initialCell : accumulatedCell / samples);
    }

    /// <summary>Reconstruit les bits avec la boucle à verrouillage de phase utilisée par les décodeurs ISO.</summary>
    /// <param name="intervals">Intervalles de flux exprimés en ticks.</param>
    /// <param name="centre">Durée centrale de la cellule, en ticks.</param>
    /// <param name="maximumCells">Nombre maximal de cellules représentées par un intervalle.</param>
    /// <returns>Bits reconstruits et durée moyenne de cellule.</returns>
    public static FluxBitstream ReconstructPll(IReadOnlyList<uint> intervals, double centre, int maximumCells)
    {
        ArgumentNullException.ThrowIfNull(intervals);
        var clock = centre;
        var minimum = centre * FluxDecodingParameters.MinimumPllClockRatio;
        var maximum = centre * FluxDecodingParameters.MaximumPllClockRatio;
        var ticks = 0d;
        var accumulatedClock = 0d;
        var samples = 0;
        var bits = new List<bool>(intervals.Count * 4);

        foreach (var interval in intervals)
        {
            ticks += interval;
            if (ticks < clock * FluxDecodingParameters.HalfCycle) continue;

            var zeros = 0;
            while (zeros < maximumCells - 1)
            {
                ticks -= clock;
                if (ticks < clock * FluxDecodingParameters.HalfCycle) break;
                zeros++;
                bits.Add(false);
            }
            bits.Add(true);

            var correctedTicks = ticks * FluxDecodingParameters.PllPhaseRetention;
            if (zeros <= FluxDecodingParameters.MaximumZerosForDirectPllCorrection) clock += ticks * FluxDecodingParameters.PllCorrectionCoefficient;
            else clock += (centre - clock) * FluxDecodingParameters.PllCorrectionCoefficient;
            clock = Math.Clamp(clock, minimum, maximum);
            ticks = correctedTicks;
            accumulatedClock += clock;
            samples++;
        }

        return new(bits.ToArray(), samples == 0 ? centre : accumulatedClock / samples);
    }
}
