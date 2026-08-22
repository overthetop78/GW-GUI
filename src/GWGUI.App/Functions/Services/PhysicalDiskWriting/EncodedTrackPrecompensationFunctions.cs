using GWGUI.App.Enums.Services.PhysicalDiskWriting;
using GWGUI.MediaEngine.Encoding;

namespace GWGUI.App.Functions.Services.PhysicalDiskWriting;

internal static class EncodedTrackPrecompensationFunctions
{
    public static IReadOnlyList<uint> Apply(
        EncodedDiskTrack track,
        PhysicalTrackEncoding encoding,
        double nanoseconds)
    {
        ArgumentNullException.ThrowIfNull(track);
        if (nanoseconds <= 0) return track.Track.Revolution.FluxIntervals;
        var adjustment = nanoseconds / EncodedTrackTiming.TickNanoseconds;
        if (adjustment >= track.BitCellTicks)
            throw new ArgumentOutOfRangeException(nameof(nanoseconds), "Precompensation must be shorter than one bit cell.");

        var bitTicks = Enumerable.Repeat((double)track.BitCellTicks, track.Track.Bits.Count).ToArray();
        if (encoding == PhysicalTrackEncoding.Mfm)
        {
            ApplyPattern(track.Track.Bits, bitTicks, [true, false, true, false, false], 2, -adjustment);
            ApplyPattern(track.Track.Bits, bitTicks, [false, false, true, false, true], 2, adjustment);
        }

        ApplyPattern(track.Track.Bits, bitTicks, [true, true, false], 1, -adjustment);
        ApplyPattern(track.Track.Bits, bitTicks, [false, true, true], 1, adjustment);
        return BuildIntervals(track.Track.Bits, bitTicks);
    }

    private static void ApplyPattern(
        IReadOnlyList<bool> bits,
        double[] bitTicks,
        ReadOnlySpan<bool> pattern,
        int adjustedIndex,
        double adjustment)
    {
        for (var start = 0; start <= bits.Count - pattern.Length; start++)
        {
            var matches = true;
            for (var offset = 0; offset < pattern.Length; offset++)
            {
                if (bits[start + offset] == pattern[offset]) continue;
                matches = false;
                break;
            }
            if (!matches) continue;
            bitTicks[start + adjustedIndex] += adjustment;
            bitTicks[start + adjustedIndex + 1] -= adjustment;
        }
    }

    private static IReadOnlyList<uint> BuildIntervals(IReadOnlyList<bool> bits, IReadOnlyList<double> bitTicks)
    {
        var intervals = new List<uint>();
        double elapsed = 0;
        for (var index = 0; index < bits.Count; index++)
        {
            elapsed += bitTicks[index];
            if (!bits[index]) continue;
            intervals.Add(checked((uint)Math.Round(elapsed)));
            elapsed = 0;
        }
        if (elapsed > 0) intervals.Add(checked((uint)Math.Round(elapsed)));
        return intervals.AsReadOnly();
    }
}
