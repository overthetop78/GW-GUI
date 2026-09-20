using System.Buffers.Binary;
using System.Collections.Frozen;
using MediaSourceDescriptor = global::GWGUI.MediaEngine.Contracts.MediaSourceDescriptor;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading.Sources;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Optical;

namespace GWGUI.MediaEngine.Formats.Optical.Alcohol;

/// <summary>Reads validated autonomous version 1 CD MDS descriptors and their MDF track ranges.</summary>
public sealed class AlcoholMdsReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { OpticalImageFormatIds.AlcoholMds }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds =
        new[] { MediaKind.Optical }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.OpticalTracks }.ToFrozenSet();
    private static readonly IReadOnlyList<ReadOnlyMemory<byte>> SupportedSignatures =
        [AlcoholMdsFormat.HeaderSignature];

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => AlcoholMdsFormat.Extensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => SupportedSignatures;
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => AlcoholMdsFormat.AssociatedFileExtensions;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;

    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId)) return false;
        try
        {
            var header = await ReadHeaderAsync(context, cancellationToken).ConfigureAwait(false);
            return AlcoholMdsFormat.IsSupportedVersion(header.MajorVersion)
                && (header.MediumType & AlcoholMdsConstants.DvdMediumFlag) == 0;
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
        var header = await ReadHeaderAsync(context, cancellationToken).ConfigureAwait(false);
        if (!AlcoholMdsFormat.IsSupportedVersion(header.MajorVersion))
            throw new NotSupportedException($"Unsupported MDS version {header.MajorVersion}.{header.MinorVersion}.");
        if ((header.MediumType & AlcoholMdsConstants.DvdMediumFlag) != 0)
            throw new NotSupportedException("MDS DVD layer structures are not accepted until their layer layout is decoded without approximation.");

        var sessions = new List<AlcoholMdsSessionBlock>(header.SessionCount);
        for (var sessionIndex = 0; sessionIndex < header.SessionCount; sessionIndex++)
        {
            var offset = checked(header.SessionsOffset + sessionIndex * AlcoholMdsConstants.SessionBlockSize);
            var bytes = await ReadRangeAsync(context, offset, AlcoholMdsConstants.SessionBlockSize, cancellationToken).ConfigureAwait(false);
            sessions.Add(await ReadSessionAsync(context, bytes, cancellationToken).ConfigureAwait(false));
        }

        var sources = sessions.SelectMany(session => session.Tracks)
            .Select(track => track.DataPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToDictionary(path => path, path => new FileRandomAccessData(path), StringComparer.OrdinalIgnoreCase);
        var descriptors = new List<OpticalTrackDescriptor>();
        foreach (var session in sessions)
        {
            var ordered = session.Tracks.OrderBy(track => track.StartSector).ToArray();
            for (var index = 0; index < ordered.Length; index++)
            {
                var track = ordered[index];
                var source = sources[track.DataPath];
                var availableSectors = (source.Length - track.StartOffset) / track.SectorSize;
                if (track.StartOffset < 0 || track.StartOffset > source.Length || availableSectors <= 0)
                    throw new InvalidDataException($"MDS track {track.TrackNumber} starts outside its MDF file.");
                var next = ordered.Skip(index + 1).FirstOrDefault(candidate =>
                    candidate.DataPath.Equals(track.DataPath, StringComparison.OrdinalIgnoreCase)
                    && candidate.StartOffset > track.StartOffset);
                var sectorCount = next is null
                    ? availableSectors
                    : (next.StartOffset - track.StartOffset) / track.SectorSize;
                var sessionRemaining = session.EndSector - track.StartSector;
                if (sessionRemaining > 0) sectorCount = Math.Min(sectorCount, sessionRemaining);
                if (sectorCount <= 0 || track.StartOffset > source.Length - checked(sectorCount * track.SectorSize))
                    throw new InvalidDataException($"MDS track {track.TrackNumber} exceeds its MDF file.");

                var interleavedSubchannels = track.SubchannelMode != 0;
                var payloadSize = interleavedSubchannels
                    ? track.SectorSize - OpticalSectorConstants.SubchannelSize
                    : track.SectorSize;
                var mode = ResolveMode(track, payloadSize);
                var (userOffset, userLength) = ResolveUserWindow(mode);
                descriptors.Add(new OpticalTrackDescriptor(
                    session.Number,
                    track.TrackNumber,
                    mode,
                    track.StartSector,
                    sectorCount,
                    track.SectorSize,
                    userOffset,
                    userLength,
                    source,
                    track.StartOffset,
                    [new OpticalTrackIndex(1, 0)],
                    subchannelSource: interleavedSubchannels ? source : null,
                    subchannelSourceOffset: interleavedSubchannels
                        ? checked(track.StartOffset + payloadSize)
                        : 0,
                    subchannelBytesPerSector: interleavedSubchannels ? OpticalSectorConstants.SubchannelSize : 0,
                    subchannelStride: interleavedSubchannels ? track.SectorSize : 0));
            }
        }

        var associatedPaths = sources.Keys.ToArray();
        var sourceDescriptor = new MediaSourceDescriptor(
            context.Source.PrimaryPath,
            associatedPaths,
            context.Source.KnownLength,
            context.Source.RequestedFormatId);
        var representation = new OpticalMediaImageRepresentation(
            descriptors.Sum(track => checked(track.SectorCount * track.UserDataLength)),
            descriptors,
            associatedFiles: [context.Source.PrimaryPath, .. associatedPaths]);
        return new MediaImageDocument(
            sourceDescriptor,
            OpticalImageFormatIds.AlcoholMds,
            MediaKind.Optical,
            representation,
            [],
            [],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["version"] = $"{header.MajorVersion}.{header.MinorVersion}",
                ["mediumType"] = $"0x{header.MediumType:X4}",
                ["sessionCount"] = sessions.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["trackCount"] = descriptors.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)
            });
    }

    private static async Task<AlcoholMdsSessionBlock> ReadSessionAsync(
        MediaRecognitionContext context,
        ReadOnlyMemory<byte> memory,
        CancellationToken cancellationToken)
    {
        var bytes = memory.Span;
        var start = BinaryPrimitives.ReadInt32LittleEndian(bytes[0..4]);
        var end = BinaryPrimitives.ReadInt32LittleEndian(bytes[4..8]);
        var number = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(AlcoholMdsConstants.SessionNumberOffset, 2));
        var totalBlocks = bytes[AlcoholMdsConstants.SessionTotalBlocksOffset];
        var firstTrack = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(AlcoholMdsConstants.SessionFirstTrackOffset, 2));
        var lastTrack = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(AlcoholMdsConstants.SessionLastTrackOffset, 2));
        var blocksOffset = BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(AlcoholMdsConstants.SessionTrackBlocksOffset, 4));
        if (number == 0 || totalBlocks == 0 || firstTrack == 0 || lastTrack < firstTrack)
            throw new InvalidDataException("An MDS session block contains invalid counts or track numbers.");

        var tracks = new List<AlcoholMdsTrackBlock>();
        long? leadOut = null;
        for (var blockIndex = 0; blockIndex < totalBlocks; blockIndex++)
        {
            var offset = checked((long)blocksOffset + blockIndex * AlcoholMdsConstants.TrackBlockSize);
            var block = await ReadRangeAsync(context, offset, AlcoholMdsConstants.TrackBlockSize, cancellationToken).ConfigureAwait(false);
            var span = block.Span;
            var trackNumber = span[AlcoholMdsConstants.TrackNumberOffset];
            if (trackNumber == AlcoholMdsConstants.LeadOutTrackNumber)
            {
                leadOut = ToFrames(
                    span[AlcoholMdsConstants.TrackMinuteOffset],
                    span[AlcoholMdsConstants.TrackSecondOffset],
                    span[AlcoholMdsConstants.TrackFrameOffset]);
                continue;
            }
            var extraOffset = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(AlcoholMdsConstants.TrackExtraOffset, 4));
            if (extraOffset == 0) continue;
            var sectorSize = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(AlcoholMdsConstants.TrackSectorSizeOffset, 2));
            var footerOffset = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(AlcoholMdsConstants.TrackFooterOffset, 4));
            if (trackNumber == 0 || sectorSize == 0 || footerOffset == 0)
                throw new InvalidDataException("An MDS track block is missing its track, sector, or footer information.");
            var trackSession = span[AlcoholMdsConstants.TrackSessionOffset];
            var trackMode = span[AlcoholMdsConstants.TrackModeOffset];
            var subchannelMode = span[AlcoholMdsConstants.TrackSubchannelModeOffset];
            var adrControl = span[AlcoholMdsConstants.TrackAdrControlOffset];
            var startSector = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(AlcoholMdsConstants.TrackStartSectorOffset, 4));
            var startOffset = checked((long)BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(AlcoholMdsConstants.TrackStartOffsetOffset, 8)));
            var dataPath = await ReadDataPathAsync(context, footerOffset, cancellationToken).ConfigureAwait(false);
            tracks.Add(new AlcoholMdsTrackBlock(
                trackSession,
                trackNumber,
                trackMode,
                subchannelMode,
                adrControl,
                sectorSize,
                startSector,
                startOffset,
                footerOffset,
                dataPath));
        }

        var resolvedEnd = leadOut ?? end;
        if (resolvedEnd <= start && tracks.Count > 0)
            resolvedEnd = tracks.Max(track => track.StartSector) + 1;
        return new AlcoholMdsSessionBlock(number, start, resolvedEnd, firstTrack, lastTrack, tracks);
    }

    private static async Task<string> ReadDataPathAsync(
        MediaRecognitionContext context,
        long footerOffset,
        CancellationToken cancellationToken)
    {
        var footer = await ReadRangeAsync(context, footerOffset, AlcoholMdsConstants.TrackFooterSize, cancellationToken).ConfigureAwait(false);
        var nameOffset = BinaryPrimitives.ReadUInt32LittleEndian(footer.Span.Slice(AlcoholMdsConstants.FooterFileNameOffset, 4));
        var wide = BinaryPrimitives.ReadUInt32LittleEndian(footer.Span.Slice(AlcoholMdsConstants.FooterWideCharacterOffset, 4)) != 0;
        var name = await ReadNullTerminatedStringAsync(context, nameOffset, wide, cancellationToken).ConfigureAwait(false);
        var mdsDirectory = Path.GetDirectoryName(Path.GetFullPath(context.Source.PrimaryPath)) ?? Directory.GetCurrentDirectory();
        if (name.StartsWith("*.", StringComparison.Ordinal))
            return Path.GetFullPath(Path.ChangeExtension(context.Source.PrimaryPath, name[1..]));
        return Path.GetFullPath(Path.IsPathFullyQualified(name) ? name : Path.Combine(mdsDirectory, name));
    }

    private static async Task<string> ReadNullTerminatedStringAsync(
        MediaRecognitionContext context,
        long offset,
        bool wide,
        CancellationToken cancellationToken)
    {
        const int maximumBytes = 4096;
        if (offset < 0 || offset >= context.Length) throw new InvalidDataException("An MDS filename offset is outside the descriptor.");
        var count = (int)Math.Min(maximumBytes, context.Length - offset);
        var bytes = await context.ReadAsync(offset, count, cancellationToken).ConfigureAwait(false);
        if (wide)
        {
            var end = -1;
            for (var index = 0; index + 1 < bytes.Length; index += 2)
            {
                if (bytes.Span[index] == 0 && bytes.Span[index + 1] == 0) { end = index; break; }
            }
            if (end <= 0) throw new InvalidDataException("An MDS wide filename is missing or not terminated.");
            return System.Text.Encoding.Unicode.GetString(bytes.Span[..end]);
        }
        var terminator = bytes.Span.IndexOf((byte)0);
        if (terminator <= 0) throw new InvalidDataException("An MDS filename is missing or not terminated.");
        return System.Text.Encoding.Latin1.GetString(bytes.Span[..terminator]);
    }

    private static async ValueTask<MdsHeader> ReadHeaderAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        var memory = await ReadRangeAsync(context, 0, AlcoholMdsConstants.HeaderSize, cancellationToken).ConfigureAwait(false);
        var bytes = memory.Span;
        if (!bytes.Slice(AlcoholMdsConstants.SignatureOffset, AlcoholMdsConstants.SignatureLength)
            .SequenceEqual(AlcoholMdsFormat.HeaderSignature.Span))
            throw new InvalidDataException("The MDS signature is missing.");
        var sessionsOffset = BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(AlcoholMdsConstants.SessionsOffsetOffset, 4));
        var sessionCount = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(AlcoholMdsConstants.SessionCountOffset, 2));
        if (sessionCount == 0 || sessionsOffset > context.Length - checked(sessionCount * AlcoholMdsConstants.SessionBlockSize))
            throw new InvalidDataException("The MDS session table exceeds the descriptor.");
        return new MdsHeader(
            bytes[AlcoholMdsConstants.VersionOffset],
            bytes[AlcoholMdsConstants.VersionOffset + 1],
            BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(AlcoholMdsConstants.MediumTypeOffset, 2)),
            sessionCount,
            sessionsOffset);
    }

    private static async ValueTask<ReadOnlyMemory<byte>> ReadRangeAsync(
        MediaRecognitionContext context,
        long offset,
        int length,
        CancellationToken cancellationToken)
    {
        if (offset < 0 || length < 0 || offset > context.Length - length)
            throw new InvalidDataException("An MDS structure exceeds the descriptor file.");
        return await context.ReadAsync(offset, length, cancellationToken).ConfigureAwait(false);
    }

    private static OpticalTrackMode ResolveMode(AlcoholMdsTrackBlock track, int payloadSize)
    {
        var dataTrack = (track.AdrControl & 0x04) != 0;
        if (!dataTrack && payloadSize == OpticalSectorConstants.AudioSectorSize) return OpticalTrackMode.Audio;
        return payloadSize switch
        {
            OpticalSectorConstants.Data2048Size => OpticalTrackMode.Mode1Data2048,
            OpticalSectorConstants.Mode2Data2336Size => OpticalTrackMode.Mode2Data2336,
            OpticalSectorConstants.RawSectorSize when track.Mode == 2 => OpticalTrackMode.Mode2Raw2352,
            OpticalSectorConstants.RawSectorSize => OpticalTrackMode.Mode1Raw2352,
            _ => throw new NotSupportedException($"Unsupported MDS sector payload size {payloadSize}.")
        };
    }

    private static (int Offset, int Length) ResolveUserWindow(OpticalTrackMode mode) => mode switch
    {
        OpticalTrackMode.Audio => (0, OpticalSectorConstants.AudioSectorSize),
        OpticalTrackMode.Mode1Data2048 => (0, OpticalSectorConstants.Data2048Size),
        OpticalTrackMode.Mode1Raw2352 => (OpticalSectorConstants.Mode1RawUserDataOffset, OpticalSectorConstants.Mode1RawUserDataLength),
        OpticalTrackMode.Mode2Data2336 => (0, OpticalSectorConstants.Mode2Data2336Size),
        OpticalTrackMode.Mode2Raw2352 => (OpticalSectorConstants.Mode2RawUserDataOffset, OpticalSectorConstants.Mode2RawUserDataLength),
        _ => throw new NotSupportedException($"Unsupported MDS track mode '{mode}'.")
    };

    private static long ToFrames(byte minutes, byte seconds, byte frames)
    {
        if (seconds >= AlcoholMdsConstants.SecondsPerMinute || frames >= AlcoholMdsConstants.FramesPerSecond)
            throw new InvalidDataException("An MDS MSF address is invalid.");
        return checked((minutes * AlcoholMdsConstants.SecondsPerMinute + seconds) * AlcoholMdsConstants.FramesPerSecond + frames);
    }

    private sealed record MdsHeader(byte MajorVersion, byte MinorVersion, ushort MediumType, int SessionCount, long SessionsOffset);
}
