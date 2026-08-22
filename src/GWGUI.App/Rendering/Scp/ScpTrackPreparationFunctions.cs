using GWGUI.App.Contracts.Rendering.Scp;
using GWGUI.App.Enums.Rendering.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Containers.Scp;
using SkiaSharp;

namespace GWGUI.App.Rendering.Scp;

public sealed partial class SkiaScpRenderer
{
    private PreparedScpTrack PrepareTrack(ScpTrack track, ScpRevolution revolution, string? decoderId, CancellationToken cancellationToken)
    {
        var intervals = revolution.FluxIntervals;
        var sampleStep = Math.Max(1, intervals.Count / 720);
        var total = intervals.Sum(interval => (double)interval);
        var ordered = intervals.ToArray();
        Array.Sort(ordered);
        var median = ordered[ordered.Length / 2];
        var fluxArcs = new List<PreparedScpArc>(Math.Min(720, intervals.Count));
        var shortTransitionCount = 0;
        var longTransitionCount = 0;
        var normalFluxCount = 0;
        double elapsed = 0;
        for (var index = 0; index < intervals.Count; index += sampleStep)
        {
            cancellationToken.ThrowIfCancellationRequested();
            double span = 0;
            for (var sample = index; sample < Math.Min(index + sampleStep, intervals.Count); sample++) span += intervals[sample];
            var color = intervals[index] < median * .65 ? new SKColor(143, 104, 255) : intervals[index] > median * 1.8 ? new SKColor(83, 173, 255) : new SKColor(36, 179, 93);
            if (color == new SKColor(143, 104, 255)) shortTransitionCount++;
            else if (color == new SKColor(83, 173, 255)) longTransitionCount++;
            else normalFluxCount++;
            fluxArcs.Add(new((float)(elapsed / total * 360 - 90), Math.Max(.08f, (float)(span / total * 360)), color));
            elapsed += span;
        }

        var structureArcs = new List<PreparedScpArc>();
        var best = _decoders.DecodeBest(track.Revolutions.Select(item => item.Flux).ToArray(), decoderId);
        FluxDecodeResult? decodedResult = null;
        if (best is not null)
        {
            var decodedRevolution = track.Revolutions[best.RevolutionIndex];
            var decoded = best.Result;
            decodedResult = decoded;
            if (decoded.EstimatedBitCellTicks > 0)
            {
                var totalBits = Math.Max(1d, decodedRevolution.FluxIntervals.Sum(interval => (double)interval) / decoded.EstimatedBitCellTicks);
                structureArcs.AddRange(decoded.Structures.Select(structure => new PreparedScpArc(
                    (float)(structure.BitOffset / totalBits * 360 - 90),
                    Math.Max(.18f, (float)(structure.BitLength / totalBits * 360)),
                    StructureColor(structure.Kind))));
            }
        }
        var sectors = decodedResult?.Sectors ?? [];
        return new(
            fluxArcs,
            structureArcs,
            Classify(decodedResult, shortTransitionCount, longTransitionCount, normalFluxCount),
            sectors.Count(sector => sector.IntegrityValid == true),
            sectors.Count(sector => sector.IntegrityValid == false),
            sectors.Count(sector => sector.IntegrityValid is null),
            true);
    }

    internal static ScpTrackVisualState Classify(FluxDecodeResult? decoded, int shortTransitions, int longTransitions, int normalFlux)
    {
        if (decoded is not null)
        {
            var sectors = decoded.Sectors;
            if (sectors.Any(sector => sector.IntegrityValid == false))
                return ScpTrackVisualState.Anomaly;
            if (sectors.Count > 0 && sectors.All(sector => sector.IntegrityValid == true))
                return ScpTrackVisualState.NormalFlux;
            if (decoded.DecodedBytes.Count > 0 || decoded.Structures.Any(structure => structure.Kind is FluxStructureKind.DataAddressMark or FluxStructureKind.DeletedDataAddressMark or FluxStructureKind.AppleData or FluxStructureKind.FormatData))
                return ScpTrackVisualState.DecodedData;
            if (decoded.Structures.Any(structure => structure.Kind == FluxStructureKind.TimingAnomaly))
                return ScpTrackVisualState.LongTransition;
            if (decoded.Structures.Any(structure => structure.Kind is FluxStructureKind.IdAddressMark or FluxStructureKind.AppleAddress or FluxStructureKind.CommodoreHeader or FluxStructureKind.FormatHeader))
                return ScpTrackVisualState.Header;
            if (decoded.Structures.Count > 0)
                return ScpTrackVisualState.ShortTransition;
        }

        if (shortTransitions > normalFlux && shortTransitions >= longTransitions)
            return ScpTrackVisualState.ShortTransition;
        if (longTransitions > normalFlux)
            return ScpTrackVisualState.LongTransition;
        return ScpTrackVisualState.NormalFlux;
    }
}
