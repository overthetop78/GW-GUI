namespace GWGUI.Scp.Decoding;

public sealed class RawFluxDecoder : IFluxDecoder
{
    public string Id => "raw"; public string DisplayName => "Flux brut";
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var median = FluxBitstream.EstimateBitCell(revolution.FluxIntervals);
        var anomalies = new List<FluxStructure>();
        var bitOffset = 0;
        for (var index = 0; index < revolution.FluxIntervals.Count; index++)
        {
            var interval = revolution.FluxIntervals[index];
            var bitLength = Math.Clamp((int)Math.Round(interval / median), 1, 64);
            if (interval > median * 10) anomalies.Add(new(FluxStructureKind.TimingAnomaly, bitOffset, bitLength, "Intervalle de flux exceptionnellement long."));
            else if (index > 0 && interval < median * .55) anomalies.Add(new(FluxStructureKind.TimingAnomaly, bitOffset, bitLength, "Impulsion de flux exceptionnellement courte."));
            bitOffset += bitLength;
        }
        return new(Id, DisplayName, .05, median, anomalies, []);
    }
}
