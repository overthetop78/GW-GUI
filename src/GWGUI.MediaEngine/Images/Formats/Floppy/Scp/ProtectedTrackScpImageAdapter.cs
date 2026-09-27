using System.Globalization;
using GWGUI.MediaEngine.Images.Models.Flux;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.Scp;

/// <summary>Builds an SCP container model from the common protected-track representation.</summary>
internal static class ProtectedTrackScpImageAdapter
{
    public static ScpImage Create(
        ProtectedTrackImage image,
        IReadOnlyDictionary<string, string> metadata)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(metadata);
        if (image.Tracks.Count == 0) throw new InvalidDataException("SCP requires at least one flux track.");

        var revolutionCount = image.Tracks.Min(track => track.Revolutions.Count);
        if (revolutionCount == 0 || revolutionCount > byte.MaxValue)
            throw new NotSupportedException("SCP requires at least one revolution on every track.");
        var resolutions = image.Tracks
            .SelectMany(track => track.Revolutions)
            .Select(revolution => revolution.ResolutionNanoseconds)
            .Distinct()
            .ToArray();
        int? commonResolution = resolutions.Length == 1 ? resolutions[0] : null;
        var resolution = ResolveResolution(metadata, commonResolution);
        var resolutionNanoseconds = ScpFormatConstants.ResolutionStepNanoseconds *
                                    (resolution + ScpFormatConstants.ResolutionIndexOffset);

        var tracks = image.Tracks
            .Select(track => new ScpTrack(
                ScpFormatConstants.ToTrackNumber(track.Cylinder, track.Head),
                track.Cylinder,
                track.Head,
                track.Revolutions.Take(revolutionCount).Select(revolution => ConvertRevolution(
                    revolution,
                    resolutionNanoseconds)).ToArray()))
            .OrderBy(track => track.TrackNumber)
            .ToArray();
        var flags = ReadByte(metadata, ScpMetadataKeys.Flags) is { } rawFlags
            ? (ScpFlags)rawFlags
            : ScpFlags.IndexAligned | ScpFlags.ThirdPartyCreator |
              (image.WriteProtected ? ScpFlags.None : ScpFlags.Writable);
        flags &= ~(ScpFlags.Footer | ScpFlags.Extended);
        var header = new ScpHeader(
            ReadByte(metadata, ScpMetadataKeys.Version) ?? ScpWriterDefaults.Version,
            ReadByte(metadata, ScpMetadataKeys.DiskType) ?? (byte)ScpDiskType.Other720,
            checked((byte)revolutionCount),
            tracks.Min(track => track.TrackNumber),
            tracks.Max(track => track.TrackNumber),
            flags,
            (ScpBitCellEncoding)(ReadByte(metadata, ScpMetadataKeys.BitCellEncoding) ??
                                 (byte)ScpBitCellEncoding.Default16Bit),
            ResolveHeads(tracks),
            resolution,
            ScpFormatConstants.MissingChecksum);
        return new ScpImage(header, tracks, true, ScpWriterDefaults.InitialFileSize);
    }

    private static byte ResolveResolution(
        IReadOnlyDictionary<string, string> metadata,
        int? resolutionNanoseconds)
    {
        var stored = ReadByte(metadata, ScpMetadataKeys.Resolution);
        if (stored.HasValue) return stored.Value;
        if (resolutionNanoseconds is null ||
            resolutionNanoseconds.Value % ScpFormatConstants.ResolutionStepNanoseconds != 0)
            return ScpFormatConstants.InternalCaptureResolution;
        var index = resolutionNanoseconds.Value / ScpFormatConstants.ResolutionStepNanoseconds -
                    ScpFormatConstants.ResolutionIndexOffset;
        return index is < byte.MinValue or > byte.MaxValue
            ? ScpFormatConstants.InternalCaptureResolution
            : checked((byte)index);
    }

    private static ScpRevolution ConvertRevolution(
        TrackFluxRevolution source,
        int targetResolutionNanoseconds)
    {
        var intervals = source.Flux.FluxIntervals
            .Select(interval => ConvertTicks(interval, source.ResolutionNanoseconds, targetResolutionNanoseconds))
            .ToArray();
        var indexTime = ConvertTicks(
            source.Flux.IndexTimeTicks,
            source.ResolutionNanoseconds,
            targetResolutionNanoseconds);
        return new(new FluxRevolution(indexTime, intervals), checked((uint)intervals.Length));
    }

    private static uint ConvertTicks(uint ticks, int sourceResolution, int targetResolution)
    {
        var converted = ((ulong)ticks * checked((uint)sourceResolution) + (uint)(targetResolution / 2)) /
                        checked((uint)targetResolution);
        if (converted == 0) return 1;
        if (converted > uint.MaxValue)
            throw new NotSupportedException("Flux timing exceeds the SCP range.");
        return checked((uint)converted);
    }

    private static ScpHeadSelection ResolveHeads(IReadOnlyList<ScpTrack> tracks)
    {
        var heads = tracks.Select(track => track.Head).Distinct().Order().ToArray();
        if (heads.SequenceEqual([0])) return ScpHeadSelection.Side0;
        if (heads.SequenceEqual([1])) return ScpHeadSelection.Side1;
        if (heads.SequenceEqual([0, 1])) return ScpHeadSelection.Both;
        throw new NotSupportedException("SCP supports only floppy heads zero and one.");
    }

    private static byte? ReadByte(
        IReadOnlyDictionary<string, string> metadata,
        string key)
        => metadata.TryGetValue(key, out var value) &&
           byte.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
}
