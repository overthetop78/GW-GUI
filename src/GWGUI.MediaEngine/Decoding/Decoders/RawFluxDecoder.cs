using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Flux;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Analyse un flux brut afin d'en signaler les anomalies temporelles.</summary>
public sealed class RawFluxDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique de l'analyse brute.</summary>
    public string Id => RawFluxAnalysisDefinitions.CodecId;

    /// <summary>Obtient le nom affiché de l'analyse brute.</summary>
    public string DisplayName => RawFluxAnalysisDefinitions.CodecDisplayName;

    /// <summary>Analyse des intervalles exprimés en ticks SCP sans produire de secteurs ni d'octets décodés ; la cellule estimée du résultat est également exprimée en ticks SCP.</summary>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var medianTicks = FluxTimingEstimator.EstimateNonFmBitCell(revolution.FluxIntervals);
        var anomalies = new List<FluxStructure>();
        var bitOffset = 0;
        for (var index = 0; index < revolution.FluxIntervals.Count; index++)
        {
            var intervalTicks = revolution.FluxIntervals[index];
            var cellCount = ConvertToCellCount(intervalTicks, medianTicks);
            var anomaly = Classify(intervalTicks, medianTicks, index);
            if (anomaly != RawFluxAnomalyKind.None) anomalies.Add(CreateStructure(anomaly, bitOffset, cellCount));
            bitOffset += cellCount;
        }
        return new(Id, DisplayName, RawFluxAnalysisDefinitions.Confidence, medianTicks, anomalies, []);
    }

    /// <summary>Convertit un intervalle exprimé en ticks SCP en un nombre borné de cellules.</summary>
    internal static int ConvertToCellCount(uint intervalTicks, double bitCellTicks) => Math.Clamp((int)Math.Round(intervalTicks / bitCellTicks), RawFluxAnalysisDefinitions.MinimumCellCount, RawFluxAnalysisDefinitions.MaximumCellCount);

    /// <summary>Classe un intervalle ; le premier intervalle ne peut pas être une impulsion courte puisqu'il débute à l'index.</summary>
    internal static RawFluxAnomalyKind Classify(uint intervalTicks, double bitCellTicks, int index)
    {
        if (intervalTicks > bitCellTicks * RawFluxAnalysisDefinitions.LongIntervalMultiplier) return RawFluxAnomalyKind.LongInterval;
        return index > 0 && intervalTicks < bitCellTicks * RawFluxAnalysisDefinitions.ShortPulseRatio ? RawFluxAnomalyKind.ShortPulse : RawFluxAnomalyKind.None;
    }

    /// <summary>Crée la structure décrivant une anomalie temporelle.</summary>
    private static FluxStructure CreateStructure(RawFluxAnomalyKind anomaly, int bitOffset, int cellCount)
    {
        var description = anomaly == RawFluxAnomalyKind.LongInterval ? RawFluxAnalysisDefinitions.LongIntervalDescription : RawFluxAnalysisDefinitions.ShortPulseDescription;
        return new(FluxStructureKind.TimingAnomaly, bitOffset, cellCount, FluxStructureDescriptions.UnclassifiedMark(RawFluxAnalysisDefinitions.StructureDescriptionName, FluxStructureKind.TimingAnomaly, null, description));
    }
}
