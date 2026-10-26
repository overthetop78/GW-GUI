using System.IO;
using System.IO.Compression;
using System.Buffers.Binary;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Images.Reading.Recognition;

namespace GWGUI.MediaEngine.Images.Formats.Tape.Csw;

/// <summary>Lit les impulsions CSW 1.00/1.01 et 2.00, compressées RLE ou Z-RLE, avec leur polarité et cadence natives.</summary>
public sealed class CswReader : IMediaImageReader
{
    internal const int MaximumPcmSamples = 128 * 1024 * 1024;
    internal static readonly byte[] Signature = System.Text.Encoding.ASCII.GetBytes("Compressed Square Wave");
    public IReadOnlySet<string> FormatIds { get; } = new HashSet<string> { TapeImageFormatIds.Csw };
    public IReadOnlySet<string> Extensions { get; } = new HashSet<string> { ".csw" };
    public IReadOnlyList<ReadOnlyMemory<byte>> Signatures { get; } = [Signature];
    public IReadOnlySet<string> AssociatedFileExtensions { get; } = new HashSet<string>();
    public IReadOnlySet<MediaKind> MediaKinds { get; } = new HashSet<MediaKind> { MediaKind.Tape };
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds { get; } = new HashSet<MediaRepresentationKind> { MediaRepresentationKind.Sequential };
    public bool SupportsFormatId(string formatId) => FormatIds.Contains(formatId);
    public async ValueTask<bool> CanReadAsync(MediaRecognitionContext context, CancellationToken token)
    {
        if (context.RequestedFormatId is not null && !SupportsFormatId(context.RequestedFormatId)) return false;
        var header = await context.ReadHeaderAsync(32, token).ConfigureAwait(false);
        return header.Length >= 32 && header.Span[..22].SequenceEqual(Signature) && header.Span[22] == 0x1a && header.Span[23] is 1 or 2;
    }
    public async Task<MediaImageDocument> ReadAsync(MediaRecognitionContext context, CancellationToken token)
    {
        var bytes = await File.ReadAllBytesAsync(context.Source.PrimaryPath, token).ConfigureAwait(false);
        if (bytes.Length < 32 || !bytes.AsSpan(0, 22).SequenceEqual(Signature) || bytes[22] != 0x1a || bytes[23] is not (1 or 2)) throw new InvalidDataException("Invalid CSW header.");
        var version2 = bytes[23] == 2;
        if (version2 && bytes.Length < 52) throw new InvalidDataException("Truncated CSW header.");
        var rate = version2 ? checked((int)BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(25))) : BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(25));
        if (rate <= 0) throw new InvalidDataException("Invalid CSW sample rate.");
        var compression = bytes[version2 ? 33 : 27];
        var high = (bytes[version2 ? 34 : 28] & 1) != 0;
        var offset = version2 ? 52 + bytes[35] : 32;
        if (offset > bytes.Length) throw new InvalidDataException("Truncated CSW extension.");
        using var input = new MemoryStream(bytes, offset, bytes.Length - offset, false);
        using Stream pulses = compression switch { 1 => input, 2 when version2 => new ZLibStream(input, CompressionMode.Decompress, true), _ => throw new NotSupportedException("Unsupported CSW compression.") };
        var samples = new List<short>();
        var extended = new byte[4]; uint pulseCount = 0;
        while (true)
        {
            token.ThrowIfCancellationRequested();
            var value = pulses.ReadByte(); if (value < 0) break;
            uint length = (uint)value;
            if (value == 0) { await pulses.ReadExactlyAsync(extended, token).ConfigureAwait(false); length = BinaryPrimitives.ReadUInt32LittleEndian(extended); }
            if (length == 0 || length > MaximumPcmSamples - samples.Count) throw new InvalidDataException("Invalid or excessive CSW pulse length.");
            samples.AddRange(Enumerable.Repeat((short)(high ? 16000 : -16000), (int)length));
            high = !high; pulseCount++;
        }
        if (version2 && pulseCount != BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(29))) throw new InvalidDataException("The CSW pulse count does not match its header.");
        return CassettePcmFunctions.Document(context.Source, TapeImageFormatIds.Csw, new(samples.ToArray(), rate));
    }
}
