using System.Globalization;
using GWGUI.MediaEngine.Representations.Flux;

namespace GWGUI.MediaEngine.Formats.Floppy.Scp;

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

        var revolutionCounts = image.Tracks.Select(track => track.Revolutions.Count).Distinct().ToArray();
        if (revolutionCounts.Length != 1 || revolutionCounts[0] == 0 || revolutionCounts[0] > byte.MaxValue)
            throw new NotSupportedException("SCP requires the same non-zero revolution count on every track.");
        var resolutions = image.Tracks
            .SelectMany(track => track.Revolutions)
            .Select(revolution => revolution.ResolutionNanoseconds)
            .Distinct()
            .ToArray();
        if (resolutions.Length != 1)
            throw new NotSupportedException("SCP requires one timing resolution for every revolution.");
        var resolution = ResolveResolution(metadata, resolutions[0]);
        var resolutionNanoseconds = ScpFormatConstants.ResolutionStepNanoseconds *
                                    (resolution + ScpFormatConstants.ResolutionIndexOffset);
        if (resolutionNanoseconds != resolutions[0])
            throw new NotSupportedException(
                $"Flux timing resolution '{resolutions[0]}' nanoseconds cannot be represented by SCP.");

        var tracks = image.Tracks
            .Select(track => new ScpTrack(
                ScpFormatConstants.ToTrackNumber(track.Cylinder, track.Head),
                track.Cylinder,
                track.Head,
                track.Revolutions.Select(revolution => new ScpRevolution(
                    revolution.Flux,
                    checked((uint)revolution.Flux.FluxIntervals.Count))).ToArray()))
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
            checked((byte)revolutionCounts[0]),
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
        int resolutionNanoseconds)
    {
        var stored = ReadByte(metadata, ScpMetadataKeys.Resolution);
        if (stored.HasValue) return stored.Value;
        if (resolutionNanoseconds % ScpFormatConstants.ResolutionStepNanoseconds != 0)
            throw new NotSupportedException(
                $"Flux timing resolution '{resolutionNanoseconds}' nanoseconds is not an SCP increment.");
        var index = resolutionNanoseconds / ScpFormatConstants.ResolutionStepNanoseconds -
                    ScpFormatConstants.ResolutionIndexOffset;
        if (index is < byte.MinValue or > byte.MaxValue)
            throw new NotSupportedException(
                $"Flux timing resolution '{resolutionNanoseconds}' nanoseconds exceeds the SCP range.");
        return checked((byte)index);
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
