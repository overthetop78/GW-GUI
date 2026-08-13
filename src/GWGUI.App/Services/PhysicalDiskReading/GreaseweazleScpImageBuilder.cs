using System.IO;
using GWGUI.Infrastructure.Hardware.Greaseweazle;
using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.App.Services.PhysicalDiskReading;

public static class GreaseweazleScpImageBuilder
{
    public static ScpImage Build(
        IReadOnlyDictionary<PhysicalDiskTrackAddress, (GreaseweazleFluxCapture Capture, GreaseweazleRotationLayout Layout)> tracks,
        PhysicalDiskReadOptions options)
    {
        ArgumentNullException.ThrowIfNull(tracks);
        ArgumentNullException.ThrowIfNull(options);
        if (tracks.Count == 0) throw new ArgumentException("At least one captured track is required.", nameof(tracks));

        var scpTracks = tracks.OrderBy(item => item.Key.Cylinder).ThenBy(item => item.Key.Head)
            .Select(item => BuildTrack(item.Key, item.Value.Capture, item.Value.Layout, options.Revolutions)).ToArray();
        var startTrack = scpTracks.Min(track => track.TrackNumber);
        var endTrack = scpTracks.Max(track => track.TrackNumber);
        var heads = ResolveHeads(scpTracks);
        var header = new ScpHeader(
            PhysicalDiskReadDefaults.ScpVersion,
            (byte)options.DiskType,
            checked((byte)options.Revolutions),
            startTrack,
            endTrack,
            ScpFlags.IndexAligned | ScpFlags.Writable | ScpFlags.ThirdPartyCreator,
            ScpBitCellEncoding.Default16Bit,
            heads,
            PhysicalDiskReadDefaults.ScpResolution,
            ScpFormatConstants.MissingChecksum);
        return new ScpImage(header, scpTracks, false, PhysicalDiskReadDefaults.InMemoryFileSize);
    }

    internal static ScpTrack BuildTrack(
        PhysicalDiskTrackAddress address,
        GreaseweazleFluxCapture capture,
        GreaseweazleRotationLayout layout,
        int revolutionCount)
    {
        if (layout.RevolutionTicks.Count < revolutionCount)
            throw new InvalidDataException("The normalized capture does not contain every requested revolution.");
        var endpoints = new List<ulong>(capture.FluxIntervals.Count);
        ulong elapsed = 0;
        foreach (var interval in capture.FluxIntervals)
        {
            elapsed += interval;
            endpoints.Add(elapsed);
        }

        var revolutions = new List<ScpRevolution>(revolutionCount);
        ulong start = layout.InitialIndexTicks;
        var endpointIndex = 0;
        while (endpointIndex < endpoints.Count && endpoints[endpointIndex] <= start) endpointIndex++;
        for (var index = 0; index < revolutionCount; index++)
        {
            var duration = layout.RevolutionTicks[index];
            var end = start + duration;
            var sourceIntervals = new List<uint>();
            var previous = start;
            while (endpointIndex < endpoints.Count && endpoints[endpointIndex] <= end)
            {
                sourceIntervals.Add(checked((uint)(endpoints[endpointIndex] - previous)));
                previous = endpoints[endpointIndex++];
            }
            var converted = ConvertIntervals(sourceIntervals, capture.SampleFrequency);
            var indexTime = ConvertTicks(duration, capture.SampleFrequency);
            revolutions.Add(new ScpRevolution(indexTime, checked((uint)converted.Count), converted));
            start = end;
        }

        var trackNumber = ScpFormatConstants.ToTrackNumber(address.Cylinder, address.Head);
        return new ScpTrack(trackNumber, address.Cylinder, address.Head, revolutions);
    }

    private static IReadOnlyList<uint> ConvertIntervals(IReadOnlyList<uint> intervals, uint sampleFrequency)
    {
        var result = new uint[intervals.Count];
        ulong sourceElapsed = 0;
        ulong targetElapsed = 0;
        for (var index = 0; index < intervals.Count; index++)
        {
            sourceElapsed += intervals[index];
            var convertedElapsed = ConvertTicks(sourceElapsed, sampleFrequency);
            result[index] = checked((uint)(convertedElapsed - targetElapsed));
            targetElapsed = convertedElapsed;
        }
        return result;
    }

    private static uint ConvertTicks(ulong ticks, uint sampleFrequency) => checked((uint)Math.Round(
        ticks * 1_000_000_000d /
        sampleFrequency /
        ScpFormatConstants.ResolutionStepNanoseconds));

    private static ScpHeadSelection ResolveHeads(IReadOnlyList<ScpTrack> tracks)
    {
        var head0 = tracks.Any(track => track.Head == 0);
        var head1 = tracks.Any(track => track.Head == 1);
        return (head0, head1) switch
        {
            (true, true) => ScpHeadSelection.Both,
            (true, false) => ScpHeadSelection.Side0,
            (false, true) => ScpHeadSelection.Side1,
            _ => throw new InvalidDataException("No supported disk head was captured.")
        };
    }
}
