using System.Buffers.Binary;
using System.Collections.Frozen;
using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Sequential;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaEngine.Interfaces.Reading;

namespace GWGUI.MediaEngine.Images.Formats.Tape.Voc;

/// <summary>Reads the legacy 8-bit Creative Voice profile accepted by Caprice32 as CPC cassette audio.</summary>
public sealed class VocReader : IMediaImageReader
{
    private const int HeaderLength = 26;
    private static readonly byte[] Signature = System.Text.Encoding.ASCII.GetBytes("Creative Voice File\u001a");
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { TapeImageFormatIds.Voc }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions =
        new[] { DiskImageFileExtensions.Voc }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Tape }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.Sequential }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => SupportedExtensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [Signature];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentations;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId)) return false;
        if (context.Length < HeaderLength) return false;
        var header = await context.ReadHeaderAsync(Signature.Length, cancellationToken).ConfigureAwait(false);
        return header.Span.SequenceEqual(Signature);
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var bytes = await File.ReadAllBytesAsync(context.Source.PrimaryPath, cancellationToken).ConfigureAwait(false);
        if (bytes.Length < HeaderLength || !bytes.AsSpan(0, Signature.Length).SequenceEqual(Signature))
            throw new InvalidDataException("The Creative Voice signature is missing.");
        var offset = (int)BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(20));
        if (offset < HeaderLength || offset >= bytes.Length) throw new InvalidDataException("The VOC data offset is invalid.");

        byte? timeConstant = null;
        var samples = new List<byte>();
        var position = offset;
        var terminated = false;
        while (position < bytes.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var id = bytes[position++];
            if (id == 0)
            {
                terminated = true;
                break;
            }
            switch (id)
            {
                case 1:
                {
                    var length = ReadUInt24(bytes, position);
                    position += 3;
                    if (length < 2 || position + length > bytes.Length) throw new InvalidDataException("A VOC sound block exceeds the image.");
                    var currentRate = bytes[position++];
                    if (bytes[position++] != 0) throw new NotSupportedException("Only unsigned 8-bit PCM VOC blocks are supported.");
                    EnsureSampleRate(timeConstant, currentRate);
                    timeConstant ??= currentRate;
                    samples.AddRange(bytes.AsSpan(position, length - 2).ToArray());
                    position += length - 2;
                    break;
                }
                case 2:
                {
                    var length = ReadUInt24(bytes, position);
                    position += 3;
                    if (position + length > bytes.Length) throw new InvalidDataException("A VOC continuation block exceeds the image.");
                    samples.AddRange(bytes.AsSpan(position, length).ToArray());
                    position += length;
                    break;
                }
                case 3:
                {
                    if (position + 3 > bytes.Length) throw new InvalidDataException("A VOC silence block is incomplete.");
                    var count = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(position)) + 1;
                    var currentRate = bytes[position + 2];
                    position += 3;
                    EnsureSampleRate(timeConstant, currentRate);
                    timeConstant ??= currentRate;
                    samples.AddRange(Enumerable.Repeat((byte)128, count));
                    break;
                }
                case 4:
                    if (position + 2 > bytes.Length) throw new InvalidDataException("A VOC marker block is incomplete.");
                    position += 2;
                    break;
                case 5:
                {
                    var length = ReadUInt24(bytes, position);
                    position += 3;
                    if (position + length > bytes.Length) throw new InvalidDataException("A VOC text block exceeds the image.");
                    position += length;
                    break;
                }
                default:
                    throw new NotSupportedException($"VOC block type {id} is outside the legacy Caprice32-compatible profile.");
            }
        }
        if (timeConstant is null || samples.Count == 0) throw new InvalidDataException("The VOC image contains no PCM samples.");

        var sampleRate = checked((int)Math.Round(1_000_000d / (256 - timeConstant.Value)));
        var pcm = new byte[samples.Count * sizeof(short)];
        for (var index = 0; index < samples.Count; index++)
            BinaryPrimitives.WriteInt16LittleEndian(pcm.AsSpan(index * sizeof(short)), (short)((samples[index] - 128) << 8));
        var source = new MemoryRandomAccessData(pcm);
        var duration = TimeSpan.FromSeconds(samples.Count / (double)sampleRate);
        var segment = new SequentialMediaSegment(
            0,
            SequentialSegmentKind.Samples,
            samples.Count,
            TimeSpan.Zero,
            duration,
            channelNumber: 0,
            direction: SequentialTravelDirection.Forward,
            dataRange: new MediaDataRange(0, pcm.Length, MediaDataRangeKind.Stored, source, 0),
            metadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["channel"] = "0",
                ["interleaved"] = bool.TrueString,
                ["blockAlign"] = sizeof(short).ToString(System.Globalization.CultureInfo.InvariantCulture)
            });
        return new MediaImageDocument(
            context.Source,
            TapeImageFormatIds.Voc,
            MediaKind.Tape,
            new SequentialMediaImageRepresentation(pcm.Length, duration, [segment]),
            [],
            terminated ? [] : ["The VOC terminator is missing."],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["channels"] = "1",
                ["sampleRate"] = sampleRate.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["bitsPerSample"] = "16",
                ["blockAlign"] = sizeof(short).ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["vocTimeConstant"] = timeConstant.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)
            });
    }

    private static int ReadUInt24(ReadOnlySpan<byte> bytes, int offset)
    {
        if (offset < 0 || offset + 3 > bytes.Length) throw new InvalidDataException("A VOC block length is incomplete.");
        return bytes[offset] | bytes[offset + 1] << 8 | bytes[offset + 2] << 16;
    }

    private static void EnsureSampleRate(byte? expected, byte actual)
    {
        if (actual == 0 || expected is not null && expected != actual)
            throw new NotSupportedException("VOC sample-rate changes are outside the Caprice32-compatible profile.");
    }
}
