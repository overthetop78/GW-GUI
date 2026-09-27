using System.Buffers.Binary;
using System.Collections.Frozen;
using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Images.Models.Sequential;
using GWGUI.MediaEngine.Images.Writing;
using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;

namespace GWGUI.MediaEngine.Images.Formats.Tape.Voc;

/// <summary>Writes mono unsigned 8-bit legacy VOC audio accepted by Caprice32.</summary>
public sealed class VocWriter : IMediaImageWriter
{
    private const ushort Version = 0x0114;
    private static readonly byte[] Signature = System.Text.Encoding.ASCII.GetBytes("Creative Voice File\u001a");
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { TapeImageFormatIds.Voc }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.Sequential }.ToFrozenSet();
    private static readonly IReadOnlySet<string> SupportedExtensions =
        new[] { DiskImageFileExtensions.Voc }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private readonly IAtomicImageFileWriter files;

    public VocWriter(IAtomicImageFileWriter? files = null) => this.files = files ?? new AtomicImageFileWriter();

    public string Id => MediaImageWriterIds.TapeVoc;
    public IReadOnlySet<string> FormatIds => SupportedFormatIds;
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentations;
    public IReadOnlySet<string> ProducedFileExtensions => SupportedExtensions;
    public bool ProducesMultipleFiles => false;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension) =>
        SupportedFormatIds.Contains(targetFormatId)
        && SupportedExtensions.Contains(targetExtension)
        && document.MediaKind == MediaKind.Tape
        && PcmSampleFunctions.TryGetProfile(document.Metadata, out _, out _, out _, out _)
        && TryGetRanges(document, out _);

    public async Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(outputPath).ToLowerInvariant();
        if (!CanWrite(document, targetFormatId, extension)
            || !PcmSampleFunctions.TryGetProfile(document.Metadata, out var channels, out var sampleRate, out var bitsPerSample, out var blockAlign)
            || !TryGetRanges(document, out var ranges))
            throw new InvalidDataException("The sequential document cannot be written as legacy PCM VOC.");

        var timeConstant = checked((byte)Math.Clamp((int)Math.Round(256 - 1_000_000d / sampleRate), 1, 255));
        var vocRate = 1_000_000d / (256 - timeConstant);
        var input = new List<int>();
        foreach (var range in ranges)
        {
            if (range.Length > int.MaxValue) throw new NotSupportedException("A PCM segment is too large for VOC conversion.");
            var bytes = new byte[checked((int)range.Length)];
            await range.Source!.ReadExactlyAsync(range.SourceOffset, bytes, cancellationToken).ConfigureAwait(false);
            input.AddRange(PcmSampleFunctions.ExtractChannel(bytes, channels, bitsPerSample, blockAlign, 0));
        }
        var outputCount = checked((int)Math.Round(input.Count * vocRate / sampleRate));
        var pcm = new byte[outputCount];
        var shift = bitsPerSample - 8;
        for (var index = 0; index < outputCount; index++)
        {
            var sourceIndex = Math.Min(input.Count - 1, (int)Math.Floor(index * sampleRate / vocRate));
            var value = shift >= 0 ? input[sourceIndex] >> shift : input[sourceIndex] << -shift;
            pcm[index] = (byte)Math.Clamp(value + 128, 0, 255);
        }

        await files.WriteAsync(outputPath, (output, token) => WriteVocAsync(output, pcm, timeConstant, token), cancellationToken)
            .ConfigureAwait(false);
        return [outputPath];
    }

    private static async Task WriteVocAsync(Stream output, byte[] pcm, byte timeConstant, CancellationToken cancellationToken)
    {
        if (pcm.Length > 0x00fffffd) throw new NotSupportedException("The VOC sound block exceeds its 24-bit length field.");
        var header = new byte[26];
        Signature.CopyTo(header, 0);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(20), checked((ushort)header.Length));
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(22), Version);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(24), unchecked((ushort)(~Version + 0x1234)));
        await output.WriteAsync(header, cancellationToken).ConfigureAwait(false);
        await output.WriteAsync(new[] { (byte)1 }, cancellationToken).ConfigureAwait(false);
        var blockLength = new byte[3];
        WriteUInt24(blockLength, pcm.Length + 2);
        await output.WriteAsync(blockLength, cancellationToken).ConfigureAwait(false);
        await output.WriteAsync(new[] { timeConstant, (byte)0 }, cancellationToken).ConfigureAwait(false);
        await output.WriteAsync(pcm, cancellationToken).ConfigureAwait(false);
        await output.WriteAsync(new byte[1], cancellationToken).ConfigureAwait(false);
    }

    private static bool TryGetRanges(MediaImageDocument document, out IReadOnlyList<MediaDataRange> ranges)
    {
        ranges = [];
        if (document.Representation is not SequentialMediaImageRepresentation { Segments: { Count: > 0 } segments }) return false;
        var selected = segments.Where(segment => segment.Kind == SequentialSegmentKind.Samples && segment.ChannelNumber is null or 0)
            .Select(segment => segment.DataRange)
            .ToArray();
        if (selected.Length == 0 || selected.Any(range => range is not { Kind: MediaDataRangeKind.Stored, Source: not null })) return false;
        ranges = selected.Select(range => range!).ToArray();
        return true;
    }

    private static void WriteUInt24(Span<byte> destination, int value)
    {
        destination[0] = (byte)value;
        destination[1] = (byte)(value >> 8);
        destination[2] = (byte)(value >> 16);
    }
}
