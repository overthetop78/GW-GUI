using System.Globalization;
using GWGUI.Domain.Constants;
using GWGUI.Domain.Contracts;
using GWGUI.Domain.Enums;
using GWGUI.Domain.Functions;
using GWGUI.MediaEngine.Formats.Floppy.Scp;

namespace GWGUI.MediaEngine.Acquisition;

/// <summary>Reconstructs MediaEngine SCP data from neutral physical floppy flux units.</summary>
public sealed class FloppyFluxAcquisitionService
{
    public ScpImage CreateScpImage(MediaAcquisitionResult acquisition)
    {
        ArgumentNullException.ThrowIfNull(acquisition);
        if (acquisition.MediaKind != MediaKind.Floppy || acquisition.RepresentationKind != MediaRepresentationKind.Flux)
            throw new NotSupportedException("A floppy flux acquisition is required to construct an SCP image.");
        if (acquisition.DataUnits.Count == 0)
            throw new InvalidDataException("The physical acquisition contains no flux track.");

        var tracks = acquisition.DataUnits.Select(CreateTrack).OrderBy(track => track.TrackNumber).ToArray();
        if (tracks.Select(track => track.TrackNumber).Distinct().Count() != tracks.Length)
            throw new InvalidDataException("The physical acquisition contains a duplicate floppy track.");
        var revolutionCount = tracks[0].Revolutions.Count;
        if (revolutionCount is < ScpFormatConstants.MinimumRevolutionCount or > ScpFormatConstants.MaximumRevolutionCount
            || tracks.Any(track => track.Revolutions.Count != revolutionCount))
            throw new InvalidDataException("Every acquired floppy track must contain the same supported revolution count.");
        var diskType = ReadDiskType(acquisition.Metadata);
        var header = new ScpHeader(
            ScpFormatConstants.InternalCaptureVersion,
            (byte)diskType,
            checked((byte)revolutionCount),
            tracks.Min(track => track.TrackNumber),
            tracks.Max(track => track.TrackNumber),
            ScpFlags.IndexAligned | ScpFlags.Writable | ScpFlags.ThirdPartyCreator,
            ScpBitCellEncoding.Default16Bit,
            ResolveHeads(tracks),
            ScpFormatConstants.InternalCaptureResolution,
            ScpFormatConstants.MissingChecksum);
        return new ScpImage(header, tracks, false, 0);
    }

    public ScpTrack CreateTrack(MediaPhysicalDataUnit unit)
    {
        ArgumentNullException.ThrowIfNull(unit);
        if (!unit.Metadata.TryGetValue(MediaPhysicalMetadataKeys.Encoding, out var encoding)
            || !encoding.Equals(MediaPhysicalEncodingIds.FluxTrackV1, StringComparison.Ordinal))
            throw new InvalidDataException("The physical data unit is not a supported neutral flux track.");
        var cylinder = ReadInt(unit.Metadata, MediaPhysicalMetadataKeys.Cylinder);
        var head = ReadInt(unit.Metadata, MediaPhysicalMetadataKeys.Head);
        var trackData = MediaFluxTrackDataFunctions.Deserialize(unit.Data.Span);
        var resolutionNanoseconds = ScpFormatConstants.ResolutionStepNanoseconds
            * (ScpFormatConstants.InternalCaptureResolution + ScpFormatConstants.ResolutionIndexOffset);
        var revolutions = trackData.Revolutions.Select(revolution =>
        {
            var intervals = ConvertNanoseconds(revolution.FluxIntervalsNanoseconds, resolutionNanoseconds);
            var indexTime = ConvertNanoseconds(revolution.IndexTimeNanoseconds, resolutionNanoseconds);
            return new ScpRevolution(indexTime, checked((uint)intervals.Count), intervals);
        }).ToArray();
        return new ScpTrack(ScpFormatConstants.ToTrackNumber(cylinder, head), cylinder, head, revolutions);
    }

    private static IReadOnlyList<uint> ConvertNanoseconds(
        IReadOnlyList<uint> intervalsNanoseconds,
        int resolutionNanoseconds)
    {
        var result = new uint[intervalsNanoseconds.Count];
        ulong sourceElapsed = 0;
        ulong targetElapsed = 0;
        for (var index = 0; index < intervalsNanoseconds.Count; index++)
        {
            sourceElapsed += intervalsNanoseconds[index];
            var convertedElapsed = (ulong)Math.Round(sourceElapsed / (double)resolutionNanoseconds);
            if (convertedElapsed <= targetElapsed)
                throw new InvalidDataException("A neutral flux interval is shorter than the selected SCP resolution.");
            result[index] = checked((uint)(convertedElapsed - targetElapsed));
            targetElapsed = convertedElapsed;
        }
        return result;
    }

    private static uint ConvertNanoseconds(uint nanoseconds, int resolutionNanoseconds) =>
        checked((uint)Math.Max(1, Math.Round(nanoseconds / (double)resolutionNanoseconds)));

    private static int ReadInt(IReadOnlyDictionary<string, string> metadata, string key) =>
        metadata.TryGetValue(key, out var value)
        && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : throw new InvalidDataException($"Physical acquisition metadata '{key}' is missing or invalid.");

    private static ScpDiskType ReadDiskType(IReadOnlyDictionary<string, string> metadata)
    {
        if (!metadata.TryGetValue(MediaPhysicalMetadataKeys.DiskType, out var value))
            throw new InvalidDataException("Physical acquisition disk-type metadata is missing.");
        if (Enum.TryParse<ScpDiskType>(value, true, out var named)) return named;
        if (byte.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numeric)
            && Enum.IsDefined(typeof(ScpDiskType), numeric)) return (ScpDiskType)numeric;
        throw new InvalidDataException($"Physical acquisition disk type '{value}' is invalid.");
    }

    private static ScpHeadSelection ResolveHeads(IReadOnlyList<ScpTrack> tracks)
    {
        var head0 = tracks.Any(track => track.Head == 0);
        var head1 = tracks.Any(track => track.Head == 1);
        return (head0, head1) switch
        {
            (true, true) => ScpHeadSelection.Both,
            (true, false) => ScpHeadSelection.Side0,
            (false, true) => ScpHeadSelection.Side1,
            _ => throw new InvalidDataException("The physical acquisition contains no supported floppy head.")
        };
    }
}
