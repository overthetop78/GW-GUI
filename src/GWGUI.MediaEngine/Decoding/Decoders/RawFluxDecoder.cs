using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Analyse un flux brut afin d'en signaler les anomalies temporelles.</summary>
public sealed class RawFluxDecoder : IFluxDecoder
{
    public string Id => FluxCodecIds.Raw; public string DisplayName => FluxCodecDisplayNames.Raw;
    /// <summary>Analyse les intervalles d'une révolution sans supposer de format sectoriel.</summary>
    /// <param name="revolution">Révolution SCP à analyser.</param><returns>Résultat contenant les anomalies temporelles détectées.</returns>
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var median = FluxTimingEstimator.EstimateNonFmBitCell(revolution.FluxIntervals);
        var anomalies = new List<FluxStructure>();
        var bitOffset = 0;
        for (var index = 0; index < revolution.FluxIntervals.Count; index++)
        {
            var interval = revolution.FluxIntervals[index];
            var bitLength = Math.Clamp((int)Math.Round(interval / median), 1, 64);
            if (interval > median * 10) anomalies.Add(new(FluxStructureKind.TimingAnomaly, bitOffset, bitLength, FluxStructureDescriptions.UnclassifiedMark("Raw Flux", FluxStructureKind.TimingAnomaly, null, "exceptionally long flux interval")));
            else if (index > 0 && interval < median * .55) anomalies.Add(new(FluxStructureKind.TimingAnomaly, bitOffset, bitLength, FluxStructureDescriptions.UnclassifiedMark("Raw Flux", FluxStructureKind.TimingAnomaly, null, "exceptionally short flux pulse")));
            bitOffset += bitLength;
        }
        return new(Id, DisplayName, .05, median, anomalies, []);
    }
}
