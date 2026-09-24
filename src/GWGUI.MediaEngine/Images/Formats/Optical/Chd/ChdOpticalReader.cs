using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Globalization;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Models.Optical;

namespace GWGUI.MediaEngine.Images.Formats.Optical.Chd;

/// <summary>Reads autonomous, uncompressed CHD V5 CD and DVD images classified by optical metadata.</summary>
public sealed class ChdOpticalReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { OpticalImageFormatIds.Chd }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds =
        new[] { MediaKind.Optical }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.OpticalTracks }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => ChdOpticalFormat.Extensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [ChdOpticalFormat.Signature];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId))
            return false;
        try
        {
            _ = await ReadLayoutAsync(context, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (Exception exception) when (exception is InvalidDataException or NotSupportedException)
        {
            return false;
        }
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        var layout = await ReadLayoutAsync(context, cancellationToken).ConfigureAwait(false);
        var logicalData = new ChdUncompressedRandomAccessData(
            new FileRandomAccessData(context.Source.PrimaryPath),
            layout.LogicalBytes,
            layout.HunkBytes,
            layout.StoredHunks);
        var tracks = layout.IsDvd
            ? CreateDvdTracks(layout, logicalData)
            : CreateCdTracks(layout, logicalData);
        var logicalLength = tracks.Sum(track => checked(track.SectorCount * track.UserDataLength));
        return new MediaImageDocument(
            context.Source,
            OpticalImageFormatIds.Chd,
            MediaKind.Optical,
            new OpticalMediaImageRepresentation(logicalLength, tracks),
            [],
            [],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["version"] = ChdConstants.Version5.ToString(CultureInfo.InvariantCulture),
                ["compression"] = "none",
                ["hunkSize"] = layout.HunkBytes.ToString(CultureInfo.InvariantCulture),
                ["unitSize"] = layout.UnitBytes.ToString(CultureInfo.InvariantCulture),
                ["trackCount"] = tracks.Count.ToString(CultureInfo.InvariantCulture),
                ["mediaProfile"] = layout.IsDvd ? "DVD" : "CD"
            });
    }

    private static IReadOnlyList<OpticalTrackDescriptor> CreateDvdTracks(
        ChdLayout layout,
        ChdUncompressedRandomAccessData source)
    {
        if (layout.TrackMetadata.Count != 0)
            throw new InvalidDataException("A CHD cannot be classified as both CD and DVD.");
        if (layout.UnitBytes != OpticalSectorConstants.Data2048Size
            || layout.LogicalBytes % OpticalSectorConstants.Data2048Size != 0)
            throw new InvalidDataException("The CHD DVD data is not arranged as complete 2048-byte units.");
        return
        [
            new OpticalTrackDescriptor(
                1,
                1,
                OpticalTrackMode.DvdData2048,
                0,
                layout.LogicalBytes / OpticalSectorConstants.Data2048Size,
                OpticalSectorConstants.Data2048Size,
                0,
                OpticalSectorConstants.Data2048Size,
                source,
                0)
        ];
    }

    private static IReadOnlyList<OpticalTrackDescriptor> CreateCdTracks(
        ChdLayout layout,
        ChdUncompressedRandomAccessData source)
    {
        if (layout.TrackMetadata.Count == 0)
            throw new InvalidDataException("The CHD has no supported optical track metadata.");
        if (layout.UnitBytes != ChdConstants.CdFrameSize || layout.HunkBytes % ChdConstants.CdFrameSize != 0)
            throw new InvalidDataException("The CHD CD data is not arranged as 2448-byte frames.");

        var declarations = layout.TrackMetadata.OrderBy(track => track.TrackNumber).ToArray();
        if (declarations.Select(track => track.TrackNumber).Distinct().Count() != declarations.Length)
            throw new InvalidDataException("The CHD contains duplicate optical track numbers.");
        var tracks = new List<OpticalTrackDescriptor>(declarations.Length);
        long chdFrameOffset = 0;
        long physicalFrameOffset = 0;
        foreach (var declaration in declarations)
        {
            var dataFrames = checked(declaration.FrameCount - declaration.PregapFrames);
            var trackSourceOffset = checked((chdFrameOffset + declaration.PregapFrames) * ChdConstants.CdFrameSize);
            var firstSector = checked(physicalFrameOffset + declaration.PregapFrames);
            var (userOffset, userLength) = GetUserDataWindow(declaration.Mode, declaration.DataSize);
            var indexes = declaration.PregapFrames == 0
                ? new[] { new OpticalTrackIndex(1, 0) }
                : new[] { new OpticalTrackIndex(0, -declaration.PregapFrames), new OpticalTrackIndex(1, 0) };
            tracks.Add(new OpticalTrackDescriptor(
                1,
                declaration.TrackNumber,
                declaration.Mode,
                firstSector,
                dataFrames,
                ChdConstants.CdFrameSize,
                userOffset,
                userLength,
                source,
                trackSourceOffset,
                indexes,
                declaration.PregapFrames,
                declaration.PostgapFrames,
                subchannelSource: declaration.SubchannelSize == 0 ? null : source,
                subchannelSourceOffset: declaration.SubchannelSize == 0
                    ? 0
                    : checked(trackSourceOffset + declaration.DataSize),
                subchannelBytesPerSector: declaration.SubchannelSize,
                storedPregapSectors: declaration.PregapStored ? declaration.PregapFrames : 0,
                storedPregapSourceOffset: declaration.PregapStored
                    ? checked(chdFrameOffset * ChdConstants.CdFrameSize)
                    : null,
                subchannelStride: declaration.SubchannelSize == 0 ? 0 : ChdConstants.CdFrameSize));
            physicalFrameOffset = checked(physicalFrameOffset + declaration.FrameCount);
            chdFrameOffset = checked(chdFrameOffset + AlignTrackFrames(declaration.FrameCount));
        }
        if (checked(chdFrameOffset * ChdConstants.CdFrameSize) != layout.LogicalBytes)
            throw new InvalidDataException("The CHD track metadata does not cover the logical CD frames.");
        return tracks;
    }

    private static async ValueTask<ChdLayout> ReadLayoutAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        if (context.Length < ChdConstants.Version5HeaderSize)
            throw new InvalidDataException("The file is too short to contain a CHD V5 header.");
        var headerMemory = await context.ReadAsync(0, ChdConstants.Version5HeaderSize, cancellationToken).ConfigureAwait(false);
        var header = headerMemory.Span;
        if (!header[..ChdOpticalFormat.Signature.Length].SequenceEqual(ChdOpticalFormat.Signature.Span))
            throw new InvalidDataException("The CHD signature is missing.");
        if (BinaryPrimitives.ReadUInt32BigEndian(header[8..12]) != ChdConstants.Version5HeaderSize
            || BinaryPrimitives.ReadUInt32BigEndian(header[12..16]) != ChdConstants.Version5)
            throw new NotSupportedException("Only CHD V5 images are supported.");
        for (var offset = 16; offset < 32; offset += sizeof(uint))
        {
            if (BinaryPrimitives.ReadUInt32BigEndian(header[offset..(offset + 4)]) != ChdConstants.UncompressedCodec)
                throw new NotSupportedException("Compressed CHD optical codecs are not supported by the autonomous profile.");
        }
        if (!header[104..124].SequenceEqual(new byte[ChdConstants.Sha1Length]))
            throw new NotSupportedException("CHD optical images with a parent require explicit parent resolution.");

        var logicalBytes = ReadPositiveLong(header[32..40], "logical length");
        var mapOffset = ReadNonNegativeLong(header[40..48], "map offset");
        var metadataOffset = ReadNonNegativeLong(header[48..56], "metadata offset");
        var hunkBytes = ReadPositiveInt(header[56..60], "hunk size");
        var unitBytes = ReadPositiveInt(header[60..64], "unit size");
        if (logicalBytes % unitBytes != 0 || hunkBytes % unitBytes != 0)
            throw new InvalidDataException("The CHD logical and hunk sizes are not aligned to complete units.");
        var hunkCount = checked((logicalBytes + hunkBytes - 1) / hunkBytes);
        var mapLength = checked(hunkCount * ChdConstants.Version5MapEntrySize);
        if (hunkCount > int.MaxValue || mapOffset < ChdConstants.Version5HeaderSize
            || mapLength > int.MaxValue || mapOffset > context.Length - mapLength)
            throw new InvalidDataException("The CHD V5 map exceeds the file.");
        var mapMemory = await context.ReadAsync(mapOffset, (int)mapLength, cancellationToken).ConfigureAwait(false);
        var storedHunks = new uint[(int)hunkCount];
        for (var index = 0; index < storedHunks.Length; index++)
            storedHunks[index] = BinaryPrimitives.ReadUInt32BigEndian(mapMemory.Span.Slice(index * 4, 4));

        var metadata = await ReadOpticalMetadataAsync(context, metadataOffset, cancellationToken).ConfigureAwait(false);
        return new ChdLayout(logicalBytes, hunkBytes, unitBytes, storedHunks, metadata.Tracks, metadata.IsDvd);
    }

    private static async ValueTask<(IReadOnlyList<ChdOpticalTrackMetadata> Tracks, bool IsDvd)> ReadOpticalMetadataAsync(
        MediaRecognitionContext context,
        long firstOffset,
        CancellationToken cancellationToken)
    {
        if (firstOffset <= 0) throw new InvalidDataException("The CHD has no metadata.");
        var tracks = new List<ChdOpticalTrackMetadata>();
        var isDvd = false;
        var visited = new HashSet<long>();
        var offset = firstOffset;
        while (offset != 0)
        {
            if (!visited.Add(offset)) throw new InvalidDataException("The CHD metadata chain contains a loop.");
            if (offset < ChdConstants.Version5HeaderSize || offset > context.Length - ChdConstants.MetadataHeaderSize)
                throw new InvalidDataException("A CHD metadata header is outside the file.");
            var headerMemory = await context.ReadAsync(offset, ChdConstants.MetadataHeaderSize, cancellationToken).ConfigureAwait(false);
            var header = headerMemory.Span;
            var tag = BinaryPrimitives.ReadUInt32BigEndian(header[..4]);
            var length = checked((int)(BinaryPrimitives.ReadUInt32BigEndian(header[4..8]) & ChdConstants.MetadataLengthMask));
            var next = ReadNonNegativeLong(header[8..16], "metadata link");
            var dataOffset = checked(offset + ChdConstants.MetadataHeaderSize);
            if (length <= 0 || dataOffset > context.Length - length)
                throw new InvalidDataException("A CHD metadata record exceeds the file.");
            if (tag == ChdConstants.CdTrackMetadataTag || tag == ChdConstants.CdTrackMetadata2Tag)
            {
                var data = await context.ReadAsync(dataOffset, length, cancellationToken).ConfigureAwait(false);
                tracks.Add(ParseTrackMetadata(tag, System.Text.Encoding.ASCII.GetString(data.Span).TrimEnd('\0')));
            }
            else if (tag == ChdConstants.DvdMetadataTag)
            {
                isDvd = true;
            }
            offset = next;
        }
        if (tracks.Count == 0 && !isDvd)
            throw new InvalidDataException("The CHD metadata does not classify the image as optical media.");
        return (tracks, isDvd);
    }

    private static ChdOpticalTrackMetadata ParseTrackMetadata(uint tag, string text)
    {
        if (!text.StartsWith(ChdConstants.CdTrackMetadataPrefix, StringComparison.Ordinal))
            throw new InvalidDataException("A CHD optical track metadata record is invalid.");
        var values = text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(token => token.Split(':', 2))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0], parts => parts[1], StringComparer.Ordinal);
        var required = tag == ChdConstants.CdTrackMetadata2Tag
            ? new[] { "TRACK", "TYPE", "SUBTYPE", "FRAMES", "PREGAP", "PGTYPE", "PGSUB", "POSTGAP" }
            : new[] { "TRACK", "TYPE", "SUBTYPE", "FRAMES" };
        if (required.Any(key => !values.ContainsKey(key)))
            throw new InvalidDataException("A CHD optical track metadata record is incomplete.");

        var trackNumber = ParsePositiveInt(values["TRACK"], "track number");
        var frameCount = ParsePositiveLong(values["FRAMES"], "frame count");
        var (mode, dataSize) = ParseTrackType(values["TYPE"]);
        var subchannelSize = values["SUBTYPE"] switch
        {
            "NONE" => 0,
            "RW" or "RW_RAW" => OpticalSectorConstants.SubchannelSize,
            _ => throw new NotSupportedException($"Unsupported CHD subchannel type '{values["SUBTYPE"]}'.")
        };
        var pregap = tag == ChdConstants.CdTrackMetadata2Tag
            ? ParseNonNegativeLong(values["PREGAP"], "pregap")
            : 0;
        var postgap = tag == ChdConstants.CdTrackMetadata2Tag
            ? ParseNonNegativeLong(values["POSTGAP"], "postgap")
            : 0;
        var pregapStored = tag == ChdConstants.CdTrackMetadata2Tag && values["PGTYPE"].StartsWith('V');
        return new ChdOpticalTrackMetadata(
            trackNumber,
            mode,
            dataSize,
            subchannelSize,
            frameCount,
            pregap,
            postgap,
            pregapStored);
    }

    private static (OpticalTrackMode Mode, int DataSize) ParseTrackType(string type) => type switch
    {
        "MODE1" or "MODE1/2048" => (OpticalTrackMode.Mode1Data2048, OpticalSectorConstants.Data2048Size),
        "MODE1_RAW" or "MODE1/2352" => (OpticalTrackMode.Mode1Raw2352, OpticalSectorConstants.RawSectorSize),
        "MODE2" or "MODE2/2336" => (OpticalTrackMode.Mode2Data2336, OpticalSectorConstants.Mode2Data2336Size),
        "MODE2_FORM1" or "MODE2/2048" => (OpticalTrackMode.Mode2Form1Data2048, OpticalSectorConstants.Mode2Form1DataSize),
        "MODE2_FORM2" or "MODE2/2324" => (OpticalTrackMode.Mode2Form2Data2324, OpticalSectorConstants.Mode2Form2DataSize),
        "MODE2_FORM_MIX" => (OpticalTrackMode.Mode2FormMixData2336, OpticalSectorConstants.Mode2Data2336Size),
        "MODE2_RAW" or "MODE2/2352" or "CDI/2352" => (OpticalTrackMode.Mode2Raw2352, OpticalSectorConstants.RawSectorSize),
        "AUDIO" => (OpticalTrackMode.Audio, OpticalSectorConstants.AudioSectorSize),
        _ => throw new NotSupportedException($"Unsupported CHD optical track type '{type}'.")
    };

    private static (int Offset, int Length) GetUserDataWindow(OpticalTrackMode mode, int dataSize) => mode switch
    {
        OpticalTrackMode.Mode1Raw2352 => (OpticalSectorConstants.Mode1RawUserDataOffset, OpticalSectorConstants.Mode1RawUserDataLength),
        OpticalTrackMode.Mode2Raw2352 => (OpticalSectorConstants.Mode2RawUserDataOffset, OpticalSectorConstants.Mode2RawUserDataLength),
        _ => (0, dataSize)
    };

    private static long AlignTrackFrames(long frames) =>
        checked((frames + ChdConstants.CdTrackFrameAlignment - 1) / ChdConstants.CdTrackFrameAlignment
            * ChdConstants.CdTrackFrameAlignment);

    private static int ReadPositiveInt(ReadOnlySpan<byte> value, string name)
    {
        var result = BinaryPrimitives.ReadUInt32BigEndian(value);
        if (result == 0 || result > int.MaxValue) throw new InvalidDataException($"The CHD {name} is invalid.");
        return (int)result;
    }

    private static long ReadPositiveLong(ReadOnlySpan<byte> value, string name)
    {
        var result = BinaryPrimitives.ReadUInt64BigEndian(value);
        if (result == 0 || result > long.MaxValue) throw new InvalidDataException($"The CHD {name} is invalid.");
        return (long)result;
    }

    private static long ReadNonNegativeLong(ReadOnlySpan<byte> value, string name)
    {
        var result = BinaryPrimitives.ReadUInt64BigEndian(value);
        if (result > long.MaxValue) throw new InvalidDataException($"The CHD {name} is invalid.");
        return (long)result;
    }

    private static int ParsePositiveInt(string value, string name)
    {
        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var result) || result <= 0)
            throw new InvalidDataException($"The CHD {name} is invalid.");
        return result;
    }

    private static long ParsePositiveLong(string value, string name)
    {
        if (!long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var result) || result <= 0)
            throw new InvalidDataException($"The CHD {name} is invalid.");
        return result;
    }

    private static long ParseNonNegativeLong(string value, string name)
    {
        if (!long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var result) || result < 0)
            throw new InvalidDataException($"The CHD {name} is invalid.");
        return result;
    }

    private sealed record ChdLayout(
        long LogicalBytes,
        int HunkBytes,
        int UnitBytes,
        IReadOnlyList<uint> StoredHunks,
        IReadOnlyList<ChdOpticalTrackMetadata> TrackMetadata,
        bool IsDvd);
}
