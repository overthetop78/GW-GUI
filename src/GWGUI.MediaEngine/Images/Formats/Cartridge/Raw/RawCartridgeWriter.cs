using System.Collections.Frozen;
using System.Globalization;
using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;
using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaEngine.Images.Formats.Cartridge.Raw;

/// <summary>Writes a raw cartridge representation without padding an incomplete final bank.</summary>
public sealed class RawCartridgeWriter : IMediaImageWriter
{
    private static readonly IReadOnlySet<string> SupportedExtensions = new[]
    {
        DiskImageFileExtensions.Bin, DiskImageFileExtensions.Rom,
        DiskImageFileExtensions.Nes, DiskImageFileExtensions.Sfc,
        DiskImageFileExtensions.Smc, DiskImageFileExtensions.N64,
        DiskImageFileExtensions.Z64, DiskImageFileExtensions.V64,
        DiskImageFileExtensions.Gb, DiskImageFileExtensions.Gbc,
        DiskImageFileExtensions.Gba, DiskImageFileExtensions.Nds,
        DiskImageFileExtensions.ThreeDs, DiskImageFileExtensions.Cia,
        DiskImageFileExtensions.ThreeDsx, DiskImageFileExtensions.Sms,
        DiskImageFileExtensions.Sg, DiskImageFileExtensions.Md,
        DiskImageFileExtensions.Gen, DiskImageFileExtensions.Gg,
        DiskImageFileExtensions.ThirtyTwoX, DiskImageFileExtensions.A26,
        DiskImageFileExtensions.A52, DiskImageFileExtensions.A78,
        DiskImageFileExtensions.Pce, DiskImageFileExtensions.Vb,
        DiskImageFileExtensions.Ws, DiskImageFileExtensions.Wsc,
        DiskImageFileExtensions.Cgb
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public string Id => MediaImageWriterIds.RawCartridge;
    public IReadOnlySet<string> FormatIds =>
        new[] { DiskImageFormatIds.RawCartridge }
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds =>
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();
    public IReadOnlySet<string> ProducedFileExtensions => SupportedExtensions;
    public bool ProducesMultipleFiles => false;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension)
    {
        ArgumentNullException.ThrowIfNull(document);
        return document.Representation is IMediaBlockRepresentation
            && FormatIds.Contains(targetFormatId)
            && SupportedExtensions.Contains(NormalizeExtension(targetExtension));
    }

    public async Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        if (!CanWrite(document, targetFormatId, Path.GetExtension(outputPath)))
            throw new NotSupportedException($"Writer '{Id}' does not accept the selected target.");
        if (document.Representation is not IMediaBlockRepresentation blocks)
            throw new NotSupportedException("A block representation is required for a raw cartridge.");
        if (!document.Metadata.TryGetValue("storedLength", out var storedText)
            || !long.TryParse(storedText, NumberStyles.None, CultureInfo.InvariantCulture, out var storedLength)
            || storedLength <= 0 || storedLength > blocks.Capacity)
            throw new InvalidDataException("The raw cartridge stored length is missing or invalid.");

        var atomic = new AtomicImageFileWriter();
        await atomic.WriteAsync(outputPath, async (stream, token) =>
        {
            var buffer = new byte[256 * 1024];
            long offset = 0;
            while (offset < storedLength)
            {
                token.ThrowIfCancellationRequested();
                var count = (int)Math.Min(buffer.Length, storedLength - offset);
                await blocks.ReadExactlyAsync(offset, buffer.AsMemory(0, count), token)
                    .ConfigureAwait(false);
                await stream.WriteAsync(buffer.AsMemory(0, count), token).ConfigureAwait(false);
                offset += count;
            }
        }, cancellationToken).ConfigureAwait(false);
        return [outputPath];
    }

    private static string NormalizeExtension(string extension) =>
        extension.StartsWith(".", StringComparison.Ordinal) ? extension.ToLowerInvariant() : $".{extension.ToLowerInvariant()}";
}
