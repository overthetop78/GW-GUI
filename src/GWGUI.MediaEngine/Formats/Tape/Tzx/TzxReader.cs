using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading.Sources;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Sequential;

namespace GWGUI.MediaEngine.Formats.Tape.Tzx;

/// <summary>Reads and structurally validates TZX 1.20 blocks while retaining every complete payload.</summary>
public sealed class TzxReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { TapeImageFormatIds.Tzx }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Tape }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.Sequential }.ToFrozenSet();
    private static readonly byte[] HeaderSignature = [.. System.Text.Encoding.ASCII.GetBytes(TzxConstants.Signature), TzxConstants.EndOfTextMarker];

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => TzxFormat.Extensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [HeaderSignature];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentations;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId)) return false;
        if (context.Length < TzxConstants.HeaderSize) return false;
        var header = await context.ReadHeaderAsync(TzxConstants.HeaderSize, cancellationToken).ConfigureAwait(false);
        return header.Span[..HeaderSignature.Length].SequenceEqual(HeaderSignature);
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var source = new FileRandomAccessData(context.Source.PrimaryPath);
        await using var input = new FileStream(context.Source.PrimaryPath, FileMode.Open, FileAccess.Read, FileShare.Read,
            64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var header = new byte[TzxConstants.HeaderSize];
        await input.ReadExactlyAsync(header, cancellationToken).ConfigureAwait(false);
        if (!header.AsSpan(0, HeaderSignature.Length).SequenceEqual(HeaderSignature))
            throw new InvalidDataException("The TZX header signature is missing.");
        if (header[8] != TzxConstants.MajorVersion || header[9] > TzxConstants.MinorVersion)
            throw new NotSupportedException($"TZX version {header[8]}.{header[9]:D2} is newer than the supported 1.20 profile.");

        var blocks = new List<BlockInfo>();
        while (input.Position < input.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var id = checked((byte)input.ReadByte());
            var payloadOffset = input.Position;
            var info = await ReadBlockInfoAsync(input, id, blocks.Count, cancellationToken).ConfigureAwait(false);
            if (info.PayloadLength > input.Length - payloadOffset)
                throw new InvalidDataException($"TZX block 0x{id:x2} exceeds the image.");
            blocks.Add(info with { PayloadOffset = payloadOffset });
            input.Position = payloadOffset + info.PayloadLength;
        }
        ValidateControlFlow(blocks);

        var segments = blocks.Select(block => new SequentialMediaSegment(
            block.Index,
            SegmentKind(block.Id),
            block.PayloadLength == 0 ? null : block.PayloadLength,
            dataRange: block.PayloadLength == 0
                ? null
                : new MediaDataRange(block.PayloadOffset, block.PayloadLength, MediaDataRangeKind.Stored, source, block.PayloadOffset),
            metadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [TzxConstants.BlockIdMetadataKey] = block.Id.ToString("x2", System.Globalization.CultureInfo.InvariantCulture),
                ["payloadLength"] = block.PayloadLength.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["known"] = TzxFormat.SupportedBlockIds.Contains(block.Id).ToString(System.Globalization.CultureInfo.InvariantCulture)
            })).ToArray();

        return new MediaImageDocument(
            context.Source,
            TapeImageFormatIds.Tzx,
            MediaKind.Tape,
            new SequentialMediaImageRepresentation(context.Length, segments: segments),
            [],
            [],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["majorVersion"] = header[8].ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["minorVersion"] = header[9].ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["contextExtension"] = Path.GetExtension(context.Source.PrimaryPath).ToLowerInvariant()
            });
    }

    private static async Task<BlockInfo> ReadBlockInfoAsync(Stream input, byte id, int index, CancellationToken cancellationToken)
    {
        long length;
        short? jump = null;
        IReadOnlyList<short>? calls = null;
        switch (id)
        {
            case TzxConstants.StandardSpeedData:
                length = checked(4L + await ReadUInt16AtAsync(input, 2, cancellationToken).ConfigureAwait(false));
                break;
            case TzxConstants.TurboSpeedData:
                length = checked(18L + await ReadUInt24AtAsync(input, 15, cancellationToken).ConfigureAwait(false));
                break;
            case TzxConstants.PureTone: length = 4; break;
            case TzxConstants.PulseSequence:
                length = checked(1L + await ReadByteAtAsync(input, 0, cancellationToken).ConfigureAwait(false) * 2L);
                break;
            case TzxConstants.PureData:
                length = checked(10L + await ReadUInt24AtAsync(input, 7, cancellationToken).ConfigureAwait(false));
                break;
            case TzxConstants.DirectRecording:
                length = checked(8L + await ReadUInt24AtAsync(input, 5, cancellationToken).ConfigureAwait(false));
                break;
            case TzxConstants.CswRecording:
            case TzxConstants.GeneralizedData:
            case TzxConstants.StopIf48K:
            case TzxConstants.SetSignalLevel:
                length = checked(4L + await ReadUInt32AtAsync(input, 0, cancellationToken).ConfigureAwait(false));
                break;
            case TzxConstants.Pause: length = 2; break;
            case TzxConstants.GroupStart:
            case TzxConstants.TextDescription:
                length = checked(1L + await ReadByteAtAsync(input, 0, cancellationToken).ConfigureAwait(false));
                break;
            case TzxConstants.GroupEnd:
            case TzxConstants.LoopEnd:
            case TzxConstants.Return:
                length = 0;
                break;
            case TzxConstants.Jump:
                length = 2;
                jump = unchecked((short)await ReadUInt16AtAsync(input, 0, cancellationToken).ConfigureAwait(false));
                break;
            case TzxConstants.LoopStart: length = 2; break;
            case TzxConstants.CallSequence:
            {
                var count = await ReadUInt16AtAsync(input, 0, cancellationToken).ConfigureAwait(false);
                length = checked(2L + count * 2L);
                var targets = new short[count];
                for (var offset = 0; offset < count; offset++)
                    targets[offset] = unchecked((short)await ReadUInt16AtAsync(input, 2 + offset * 2, cancellationToken).ConfigureAwait(false));
                calls = targets;
                break;
            }
            case TzxConstants.Select:
            case TzxConstants.ArchiveInfo:
                length = checked(2L + await ReadUInt16AtAsync(input, 0, cancellationToken).ConfigureAwait(false));
                break;
            case TzxConstants.Message:
                length = checked(2L + await ReadByteAtAsync(input, 1, cancellationToken).ConfigureAwait(false));
                break;
            case TzxConstants.HardwareType:
                length = checked(1L + await ReadByteAtAsync(input, 0, cancellationToken).ConfigureAwait(false) * 3L);
                break;
            case TzxConstants.CustomInfo:
                length = checked(20L + await ReadUInt32AtAsync(input, 16, cancellationToken).ConfigureAwait(false));
                break;
            case TzxConstants.Glue: length = 9; break;
            default:
                length = checked(4L + await ReadUInt32AtAsync(input, 0, cancellationToken).ConfigureAwait(false));
                break;
        }
        return new BlockInfo(index, id, 0, length, jump, calls);
    }

    private static void ValidateControlFlow(IReadOnlyList<BlockInfo> blocks)
    {
        var loops = new Stack<int>();
        var hasCallSequence = false;
        foreach (var block in blocks)
        {
            if (block.Jump is { } jump) ValidateTarget(block.Index, jump, blocks.Count, "jump");
            if (block.Calls is { } calls)
            {
                hasCallSequence = true;
                foreach (var call in calls) ValidateTarget(block.Index, call, blocks.Count, "call");
            }
            if (block.Id == TzxConstants.LoopStart) loops.Push(block.Index);
            else if (block.Id == TzxConstants.LoopEnd && loops.Count == 0)
                throw new InvalidDataException("A TZX loop end has no matching loop start.");
            else if (block.Id == TzxConstants.LoopEnd) loops.Pop();
            else if (block.Id == TzxConstants.Return && !hasCallSequence)
                throw new InvalidDataException("A TZX return appears before any call sequence.");
        }
        if (loops.Count != 0) throw new InvalidDataException("A TZX loop start has no matching loop end.");
    }

    private static void ValidateTarget(int index, short relativeOffset, int count, string kind)
    {
        var target = index + relativeOffset;
        if (target < 0 || target >= count)
            throw new InvalidDataException($"A TZX {kind} from block {index} targets block {target}, outside the image.");
    }

    private static SequentialSegmentKind SegmentKind(byte id) => id switch
    {
        TzxConstants.StandardSpeedData or TzxConstants.TurboSpeedData or TzxConstants.PureData or
            TzxConstants.DirectRecording or TzxConstants.CswRecording or TzxConstants.GeneralizedData => SequentialSegmentKind.DataBlock,
        TzxConstants.PureTone or TzxConstants.PulseSequence => SequentialSegmentKind.Pulse,
        TzxConstants.Pause => SequentialSegmentKind.Silence,
        TzxConstants.GroupStart or TzxConstants.GroupEnd or TzxConstants.Glue => SequentialSegmentKind.TapeMark,
        _ => SequentialSegmentKind.Unknown
    };

    private static async ValueTask<byte> ReadByteAtAsync(Stream input, int relativeOffset, CancellationToken cancellationToken)
    {
        var position = input.Position;
        input.Position = position + relativeOffset;
        var buffer = new byte[1];
        await input.ReadExactlyAsync(buffer, cancellationToken).ConfigureAwait(false);
        input.Position = position;
        return buffer[0];
    }

    private static async ValueTask<ushort> ReadUInt16AtAsync(Stream input, int relativeOffset, CancellationToken cancellationToken)
    {
        var position = input.Position;
        input.Position = position + relativeOffset;
        var buffer = new byte[2];
        await input.ReadExactlyAsync(buffer, cancellationToken).ConfigureAwait(false);
        input.Position = position;
        return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
    }

    private static async ValueTask<uint> ReadUInt24AtAsync(Stream input, int relativeOffset, CancellationToken cancellationToken)
    {
        var position = input.Position;
        input.Position = position + relativeOffset;
        var buffer = new byte[3];
        await input.ReadExactlyAsync(buffer, cancellationToken).ConfigureAwait(false);
        input.Position = position;
        return (uint)(buffer[0] | buffer[1] << 8 | buffer[2] << 16);
    }

    private static async ValueTask<uint> ReadUInt32AtAsync(Stream input, int relativeOffset, CancellationToken cancellationToken)
    {
        var position = input.Position;
        input.Position = position + relativeOffset;
        var buffer = new byte[4];
        await input.ReadExactlyAsync(buffer, cancellationToken).ConfigureAwait(false);
        input.Position = position;
        return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
    }

    private sealed record BlockInfo(int Index, byte Id, long PayloadOffset, long PayloadLength, short? Jump, IReadOnlyList<short>? Calls);
}
