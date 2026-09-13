using GWGUI.App.Contracts.Rendering.Scp;
using GWGUI.App.Enums.Rendering.Scp;
using GWGUI.MediaEngine.Decoding;
using SkiaSharp;

using GWGUI.MediaEngine.Formats.Floppy.Scp;

namespace GWGUI.App.Rendering.Scp;

public sealed partial class SkiaScpRenderer
{
    private PreparedScpTrack PrepareTrack(ScpTrack track, string? decoderId, CancellationToken cancellationToken)
    {
        var shortTransitionCount = 0;
        var longTransitionCount = 0;
        var normalFluxCount = 0;
        var preparedRevolutions = new List<PreparedScpRevolution>(track.Revolutions.Count);
        foreach (var revolution in track.Revolutions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var decodedRevolution = decoderId is null
                ? null
                : _decoders.Decode(decoderId, revolution.Flux);
            var prepared = PrepareRevolution(revolution, decodedRevolution, cancellationToken);
            preparedRevolutions.Add(prepared.Revolution);
            shortTransitionCount += prepared.ShortTransitions;
            longTransitionCount += prepared.LongTransitions;
            normalFluxCount += prepared.NormalTransitions;
        }

        preparedRevolutions = ApplyFluxAgreementQuality(track.Revolutions, preparedRevolutions);

        var structureArcs = new List<PreparedScpArc>();
        var best = decoderId is null
            ? null
            : _decoders.DecodeBest(track.Revolutions.Select(item => item.Flux).ToArray(), decoderId);
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
            preparedRevolutions,
            BuildSynthesis(preparedRevolutions),
            structureArcs,
            Classify(decodedResult, shortTransitionCount, longTransitionCount, normalFluxCount),
            sectors.Count(sector => sector.IntegrityValid == true),
            sectors.Count(sector => sector.IntegrityValid == false),
            sectors.Count(sector => sector.IntegrityValid is null),
            preparedRevolutions.Any(item => item.FluxArcs.Count > 0));
    }

    private static (PreparedScpRevolution Revolution, int ShortTransitions, int LongTransitions, int NormalTransitions) PrepareRevolution(
        ScpRevolution revolution,
        FluxDecodeResult? decoded,
        CancellationToken cancellationToken)
    {
        var intervals = revolution.FluxIntervals;
        if (intervals.Count == 0) return (new PreparedScpRevolution([], 0), 0, 0, 0);

        var sampleStep = Math.Max(1, intervals.Count / 720);
        var total = intervals.Sum(interval => (double)interval);
        if (total <= 0) return (new PreparedScpRevolution([], 0), 0, 0, 0);

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
            var color = intervals[index] < median * .65 ? new SKColor(68, 151, 143) : intervals[index] > median * 1.8 ? new SKColor(72, 115, 154) : new SKColor(55, 137, 101);
            if (color == new SKColor(68, 151, 143)) shortTransitionCount++;
            else if (color == new SKColor(72, 115, 154)) longTransitionCount++;
            else normalFluxCount++;
            fluxArcs.Add(new((float)(elapsed / total * 360 - 90), Math.Max(.08f, (float)(span / total * 360)), color));
            elapsed += span;
        }

        return (new PreparedScpRevolution(fluxArcs, QualityFor(decoded, intervals, median)), shortTransitionCount, longTransitionCount, normalFluxCount);
    }

    private static PreparedScpRevolution BuildSynthesis(IReadOnlyList<PreparedScpRevolution> revolutions)
    {
        if (revolutions.Count == 0) return new([], 0);
        var sampleCount = revolutions.Max(item => item.FluxArcs.Count);
        if (sampleCount == 0) return new([], 0);
        var arcs = new PreparedScpArc[sampleCount];
        for (var index = 0; index < sampleCount; index++)
        {
            var colors = revolutions
                .Where(item => item.FluxArcs.Count > 0)
                .Select(item => item.FluxArcs[Math.Min(item.FluxArcs.Count - 1, index * item.FluxArcs.Count / sampleCount)].Color)
                .ToArray();
            var color = colors.GroupBy(item => item).OrderByDescending(group => group.Count()).First().Key;
            arcs[index] = new PreparedScpArc(-90f + index * 360f / sampleCount, 360f / sampleCount, color);
        }
        return new(arcs, revolutions.Average(item => item.Quality));
    }

    private static double QualityFor(FluxDecodeResult? decoded, IReadOnlyList<uint> intervals, uint median)
    {
        if (decoded is not null)
        {
            if (decoded.Sectors.Count == 0)
                return Math.Clamp(decoded.Confidence, 0, 1);
            var valid = decoded.Sectors.Count(item => item.IntegrityValid == true);
            var unknown = decoded.Sectors.Count(item => item.IntegrityValid is null);
            return Math.Clamp((valid + unknown * .35) / decoded.Sectors.Count, 0, 1);
        }

        return intervals.Count == 0 || median == 0 ? 0 : 1;
    }

    private static List<PreparedScpRevolution> ApplyFluxAgreementQuality(
        IReadOnlyList<ScpRevolution> source,
        IReadOnlyList<PreparedScpRevolution> prepared)
    {
        if (source.Count != prepared.Count || source.Count == 0) return prepared.ToList();
        if (source.Count == 1)
            return [prepared[0] with { Quality = source[0].FluxIntervals.Count > 0 ? 1 : 0 }];

        var durations = source.Select(item => (double)item.IndexTimeTicks).Order().ToArray();
        var counts = source.Select(item => (double)item.FluxIntervals.Count).Order().ToArray();
        var medianDuration = durations[durations.Length / 2];
        var medianCount = counts[counts.Length / 2];
        var result = new List<PreparedScpRevolution>(prepared.Count);
        for (var index = 0; index < prepared.Count; index++)
        {
            var revolution = source[index];
            if (revolution.FluxIntervals.Count == 0 || medianDuration <= 0 || medianCount <= 0)
            {
                result.Add(prepared[index] with { Quality = 0 });
                continue;
            }

            var durationDeviation = Math.Abs(revolution.IndexTimeTicks - medianDuration) / medianDuration;
            var countDeviation = Math.Abs(revolution.FluxIntervals.Count - medianCount) / medianCount;
            var durationAgreement = Math.Clamp(1 - durationDeviation / .08, 0, 1);
            var countAgreement = Math.Clamp(1 - countDeviation / .18, 0, 1);
            result.Add(prepared[index] with { Quality = durationAgreement * .65 + countAgreement * .35 });
        }
        return result;
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
