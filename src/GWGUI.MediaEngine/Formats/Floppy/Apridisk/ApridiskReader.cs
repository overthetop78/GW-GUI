using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.Formats.Floppy.Apridisk;

/// <summary>Lit les conteneurs ApriDisk produits pour les ACT Apricot PC/Xi.</summary>
public sealed class ApridiskReader : IMediaImageReader
{
    internal const int HeaderSize = 128;
    internal const int BlockSize = 512;
    internal const int Cylinders = 70;
    internal const int Heads = 1;
    internal const int SectorsPerTrack = 9;
    internal const int BlockCount = Cylinders * Heads * SectorsPerTrack;

    private const uint SectorRecord = 0xE31D0001;
    private const ushort Uncompressed = 0x9E90;
    private const ushort Compressed = 0x3E5A;
    private const int RecordHeaderSize = 16;
    private static readonly byte[] Signature = "ACT Apricot disk image\u001a\u0004"u8.ToArray();
    private static readonly IReadOnlySet<string> SupportedFormatIds = new[] { DiskImageFormatIds.ApricotPcXi315 }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions = new[] { DiskImageFileExtensions.Dsk }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlyList<ReadOnlyMemory<byte>> SupportedSignatures = [Signature];
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Floppy }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations = new[] { MediaRepresentationKind.Sectors }.ToFrozenSet();
    private readonly Func<string, CancellationToken, Task<byte[]>> readBytes;

    public ApridiskReader() : this(File.ReadAllBytesAsync) { }

    internal ApridiskReader(Func<string, CancellationToken, Task<byte[]>> readBytes) =>
        this.readBytes = readBytes ?? throw new ArgumentNullException(nameof(readBytes));

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => SupportedExtensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => SupportedSignatures;
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentations;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default) =>
        Read(await readBytes(path, cancellationToken).ConfigureAwait(false), cancellationToken);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId)) return false;
        if (!SupportedExtensions.Contains(context.Extension) || context.Length < HeaderSize) return false;
        var signature = await context.ReadHeaderAsync(Signature.Length, cancellationToken).ConfigureAwait(false);
        return signature.Span.SequenceEqual(Signature);
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var bytes = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        return MediaImageDocumentFactory.CreateFloppySector(context.Source, Read(bytes.Span, cancellationToken));
    }

    internal static SectorImage Read(ReadOnlySpan<byte> data, CancellationToken cancellationToken = default)
    {
        if (data.Length < HeaderSize || !data[..Signature.Length].SequenceEqual(Signature))
            throw new InvalidDataException("The image does not contain an ACT Apricot ApriDisk header.");

        var sectors = new Dictionary<int, SectorBlock>();
        var offset = HeaderSize;
        while (offset < data.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureAvailable(data, offset, RecordHeaderSize);
            var type = BinaryPrimitives.ReadUInt32LittleEndian(data[offset..]);
            var compression = BinaryPrimitives.ReadUInt16LittleEndian(data[(offset + 4)..]);
            var headerLength = BinaryPrimitives.ReadUInt16LittleEndian(data[(offset + 6)..]);
            var payloadLength = BinaryPrimitives.ReadUInt32LittleEndian(data[(offset + 8)..]);
            var head = data[offset + 12];
            var sector = data[offset + 13];
            var cylinder = BinaryPrimitives.ReadUInt16LittleEndian(data[(offset + 14)..]);
            if (headerLength < RecordHeaderSize || payloadLength > int.MaxValue)
                throw new InvalidDataException($"Invalid ApriDisk record at byte {offset}.");
            var recordLength = checked((int)headerLength + (int)payloadLength);
            EnsureAvailable(data, offset, recordLength);

            if (type == SectorRecord)
            {
                if (cylinder >= Cylinders || head >= Heads || sector is < 1 or > SectorsPerTrack)
                    throw new InvalidDataException($"ApriDisk sector address {cylinder}/{head}/{sector} is outside the ACT Apricot PC/Xi geometry.");
                var payload = data.Slice(offset + headerLength, (int)payloadLength);
                var bytes = DecodeSector(payload, compression);
                var logical = (cylinder * Heads + head) * SectorsPerTrack + sector - 1;
                sectors[logical] = new(logical, new(cylinder, head, sector), bytes);
            }
            offset += recordLength;
        }

        if (sectors.Count != BlockCount)
            throw new InvalidDataException($"The ApriDisk image contains {sectors.Count} usable sectors instead of {BlockCount}.");
        return new(DiskImageFormatIds.ApricotPcXi315, BlockSize, Cylinders, Heads, SectorsPerTrack, sectors.Values, capacity: BlockCount * BlockSize, logicalBlockCount: BlockCount);
    }

    private static byte[] DecodeSector(ReadOnlySpan<byte> payload, ushort compression)
    {
        if (compression == Uncompressed)
        {
            if (payload.Length != BlockSize) throw new InvalidDataException($"An uncompressed ApriDisk sector contains {payload.Length} bytes instead of {BlockSize}.");
            return payload.ToArray();
        }
        if (compression != Compressed || payload.Length % 3 != 0)
            throw new InvalidDataException($"Unsupported ApriDisk sector compression 0x{compression:X4}.");
        var result = new byte[BlockSize];
        var written = 0;
        for (var index = 0; index < payload.Length; index += 3)
        {
            var count = BinaryPrimitives.ReadUInt16LittleEndian(payload[index..]);
            if (count == 0 || written > result.Length - count)
                throw new InvalidDataException("An ApriDisk compressed sector expands beyond 512 bytes.");
            result.AsSpan(written, count).Fill(payload[index + 2]);
            written += count;
        }
        if (written != result.Length) throw new InvalidDataException($"An ApriDisk compressed sector expands to {written} bytes instead of {BlockSize}.");
        return result;
    }

    private static void EnsureAvailable(ReadOnlySpan<byte> data, int offset, int count)
    {
        if (offset < 0 || count < 0 || offset > data.Length - count)
            throw new InvalidDataException($"The ApriDisk record at byte {offset} is truncated.");
    }
}
