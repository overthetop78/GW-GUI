using System.IO;
using System.IO.Compression;
using System.Buffers.Binary;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;

namespace GWGUI.MediaEngine.Images.Formats.Tape.Csw;

/// <summary>Écrit les durées des impulsions en CSW 2.00 Z-RLE par l'écrivain atomique partagé.</summary>
/// <remarks>CSW conserve les passages par zéro et leur cadence, et non l'amplitude analogique. Les pauses sans
/// transition deviennent une impulsion longue ; aucune fréquence de modulation de machine n'est imposée ici.</remarks>
public sealed class CswWriter(IAtomicImageFileWriter? files = null) : IMediaImageWriter
{
    private readonly IAtomicImageFileWriter _files = files ?? new AtomicImageFileWriter();
    public string Id => MediaImageWriterIds.TapeCsw;
    public IReadOnlySet<string> FormatIds { get; } = new HashSet<string> { TapeImageFormatIds.Csw };
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds { get; } = new HashSet<MediaRepresentationKind> { MediaRepresentationKind.Sequential };
    public IReadOnlySet<string> ProducedFileExtensions { get; } = new HashSet<string> { ".csw" };
    public bool ProducesMultipleFiles => false;
    public bool CanWrite(MediaImageDocument document, string format, string extension) => document.MediaKind == MediaKind.Tape && document.Representation.RepresentationKind == MediaRepresentationKind.Sequential && FormatIds.Contains(format) && ProducedFileExtensions.Contains(extension.ToLowerInvariant()) && PcmSampleFunctions.TryGetProfile(document.Metadata, out _, out _, out _, out _);
    public async Task<IReadOnlyList<string>> WriteAsync(MediaImageDocument document, string path, string format, CancellationToken token = default)
    {
        if (!CanWrite(document, format, Path.GetExtension(path))) throw new InvalidDataException("A PCM cassette document is required.");
        var native = await CassettePcmFunctions.ChannelsAsync(document, token).ConfigureAwait(false);
        var samples = native.Channels[0];
        using var pulses = new MemoryStream();
        var high = samples.Length > 0 && samples[0] >= 0;
        uint length = 0, count = 0;
        void Pulse()
        {
            if (length <= 255) pulses.WriteByte((byte)length);
            else { pulses.WriteByte(0); Span<byte> extended = stackalloc byte[4]; BinaryPrimitives.WriteUInt32LittleEndian(extended, length); pulses.Write(extended); }
            length = 0; count++;
        }
        for (var index = 0; index < samples.Length; index++)
        {
            if ((index & 0xffff) == 0) token.ThrowIfCancellationRequested();
            var sample = samples[index];
            var current = sample == 0 ? high : sample > 0;
            if (current != high && length > 0) { Pulse(); high = current; }
            length++;
        }
        if (length > 0) Pulse();
        var header = new byte[52]; CswReader.Signature.CopyTo(header, 0); header[22] = 0x1a; header[23] = 2;
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(25), checked((uint)native.Rate));
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(29), count); header[33] = 2;
        header[34] = samples.Length > 0 && samples[0] >= 0 ? (byte)1 : (byte)0;
        System.Text.Encoding.ASCII.GetBytes("GW GUI", header.AsSpan(36));
        await _files.WriteAsync(path, async (stream, cancellation) =>
        {
            await stream.WriteAsync(header, cancellation).ConfigureAwait(false);
            await using var compressed = new ZLibStream(stream, CompressionLevel.Optimal, true);
            pulses.Position = 0; await pulses.CopyToAsync(compressed, cancellation).ConfigureAwait(false);
        }, token).ConfigureAwait(false);
        return [path];
    }
}
