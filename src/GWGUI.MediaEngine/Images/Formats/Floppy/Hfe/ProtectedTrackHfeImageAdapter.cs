using System.Globalization;
using GWGUI.MediaEngine.Images.Conversion.Flux;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp;
using GWGUI.MediaEngine.Images.Models.Flux;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.Hfe;

/// <summary>Builds an HFE v1 container model from the common protected-track representation.</summary>
internal static class ProtectedTrackHfeImageAdapter
{
    public static HfeImage Create(
        ProtectedTrackImage image,
        IReadOnlyDictionary<string, string> metadata)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(metadata);
        var tracks = image.Tracks.Select(ToHfeTrack).ToArray();
        ValidateTrackLayout(tracks);
        var bitRates = tracks.Select(track => CalculateBitRate(track.BitCellTicks)).Distinct().ToArray();
        if (bitRates.Length != 1)
            throw new NotSupportedException("HFE v1 requires one bit rate for every track.");
        var bitRate = ReadUInt16(metadata, HfeMetadataKeys.BitRate) ?? bitRates[0];
        if (bitRate != bitRates[0])
            throw new InvalidDataException("The HFE metadata bit rate does not match the represented track timing.");
        return new HfeImage(
            ReadByte(metadata, HfeMetadataKeys.Revision) ?? HfeFormat.Revision,
            tracks.Max(track => track.Cylinder) + 1,
            tracks.Max(track => track.Head) + 1,
            ResolveEncoding(metadata),
            bitRate,
            tracks);
    }

    private static HfeTrack ToHfeTrack(ProtectedTrack track)
    {
        if (track.Bits is { Count: > 0 })
            return new HfeTrack(track.Cylinder, track.Head, track.Bits, ResolveBitCellTicks(track));
        if (track.Revolutions.Count != 1)
            throw new NotSupportedException("HFE v1 cannot preserve multiple separate flux revolutions.");

        var revolution = track.Revolutions[0];
        if (revolution.ResolutionNanoseconds % HfeFormat.TickNanoseconds != 0)
            throw new NotSupportedException("The flux timing resolution cannot be expressed in HFE ticks.");
        var scale = checked((uint)(revolution.ResolutionNanoseconds / HfeFormat.TickNanoseconds));
        var values = revolution.Flux.FluxIntervals
            .Append(revolution.Flux.IndexTimeTicks)
            .Select(value => checked(value * scale));
        var bitCellTicks = FluxBitCellConverter.GreatestCommonDivisor(values);
        if (bitCellTicks == 0) throw new InvalidDataException("The flux track does not contain representable timing.");
        var intervals = revolution.Flux.FluxIntervals.Select(value => checked(value * scale)).ToArray();
        var indexTime = checked(revolution.Flux.IndexTimeTicks * scale);
        var bits = FluxBitCellConverter.ToBits(intervals, indexTime, bitCellTicks);
        return new HfeTrack(track.Cylinder, track.Head, bits, bitCellTicks);
    }

    private static uint ResolveBitCellTicks(ProtectedTrack track)
    {
        if (track.Timing.Count != 1 ||
            track.Timing[0].BitOffset != 0 ||
            track.Timing[0].BitLength != track.Bits!.Count)
            throw new NotSupportedException("HFE v1 requires one uniform timing segment for each track.");
        var nanoseconds = track.Timing[0].BitCellNanoseconds;
        var ticks = nanoseconds / HfeFormat.TickNanoseconds;
        if (ticks < 1 || ticks > uint.MaxValue || ticks != Math.Truncate(ticks))
            throw new NotSupportedException("The track bit-cell timing cannot be expressed in HFE ticks.");
        return checked((uint)ticks);
    }

    private static void ValidateTrackLayout(IReadOnlyList<HfeTrack> tracks)
    {
        if (tracks.Count == 0) throw new InvalidDataException("HFE requires at least one track.");
        if (tracks.Any(track => track.Head >= HfeFormat.MaximumHeadCount))
            throw new NotSupportedException("HFE v1 supports at most two heads.");
        if (tracks.Any(track => track.Bits.Count % HfeFormat.BitsPerByte != 0))
            throw new NotSupportedException("HFE v1 would require padding at the end of a flux track.");
        for (var cylinder = 0; cylinder <= tracks.Max(track => track.Cylinder); cylinder++)
        {
            var sides = tracks.Where(track => track.Cylinder == cylinder).ToArray();
            if (sides.Length == 0) throw new NotSupportedException($"HFE v1 requires cylinder '{cylinder}'.");
            if (sides.Select(track => track.Bits.Count).Distinct().Count() != 1)
                throw new NotSupportedException("HFE v1 would require different padding between heads.");
        }
    }

    private static byte ResolveEncoding(IReadOnlyDictionary<string, string> metadata)
    {
        var hfeEncoding = ReadByte(metadata, HfeMetadataKeys.Encoding);
        if (hfeEncoding.HasValue) return hfeEncoding.Value;
        var scpDiskType = ReadByte(metadata, ScpMetadataKeys.DiskType);
        if (scpDiskType.HasValue) return ScpHfeEncodingResolver.Resolve(scpDiskType.Value);
        throw new NotSupportedException("The flux document does not declare an HFE encoding or an SCP disk type.");
    }

    private static ushort CalculateBitRate(uint bitCellTicks)
    {
        if (bitCellTicks == 0) throw new InvalidDataException("The HFE bit-cell duration cannot be zero.");
        var value = HfeFormat.NanosecondsPerSecond /
                    (HfeFormat.BitsPerDataBit * HfeFormat.TickNanoseconds * 1000L * bitCellTicks);
        if (value is < 1 or > ushort.MaxValue)
            throw new NotSupportedException($"The calculated HFE bit rate '{value}' is invalid.");
        return checked((ushort)value);
    }

    private static byte? ReadByte(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) &&
           byte.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;

    private static ushort? ReadUInt16(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) &&
           ushort.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
}
