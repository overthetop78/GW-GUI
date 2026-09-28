using System.Collections.Frozen;
using System.Globalization;
using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Blocks;
using GWGUI.MediaEngine.Images.Writing;
using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;
using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaEngine.Images.Formats.Cartridge.Console;

/// <summary>Écrit les cartouches console à partir de banques contiguës.</summary>
public sealed class ConsoleCartridgeWriter : IMediaImageWriter
{
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> FormatExtensions =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [DiskImageFormatIds.NintendoNes] = Extensions(DiskImageFileExtensions.Nes),
            [DiskImageFormatIds.NintendoSnes] = Extensions(DiskImageFileExtensions.Sfc, DiskImageFileExtensions.Smc),
            [DiskImageFormatIds.NintendoN64] = Extensions(DiskImageFileExtensions.N64, DiskImageFileExtensions.Z64, DiskImageFileExtensions.V64),
            [DiskImageFormatIds.NintendoGameBoy] = Extensions(DiskImageFileExtensions.Gb),
            [DiskImageFormatIds.NintendoGameBoyColor] = Extensions(DiskImageFileExtensions.Gbc, DiskImageFileExtensions.Cgb),
            [DiskImageFormatIds.NintendoGameBoyAdvance] = Extensions(DiskImageFileExtensions.Gba),
            [DiskImageFormatIds.NintendoNds] = Extensions(DiskImageFileExtensions.Nds),
            [DiskImageFormatIds.NintendoGameWatch] = Extensions(DiskImageFileExtensions.Mgw),
            [DiskImageFormatIds.Nintendo3Ds] = Extensions(DiskImageFileExtensions.ThreeDs,
                DiskImageFileExtensions.Cia, DiskImageFileExtensions.ThreeDsx,
                DiskImageFileExtensions.Cci, DiskImageFileExtensions.Cxi,
                DiskImageFileExtensions.Axf, DiskImageFileExtensions.Elf,
                DiskImageFileExtensions.App),
            [DiskImageFormatIds.NintendoVirtualBoy] = Extensions(DiskImageFileExtensions.Vb),
            [DiskImageFormatIds.SegaSg1000] = Extensions(DiskImageFileExtensions.Sg),
            [DiskImageFormatIds.SegaMasterSystem] = Extensions(DiskImageFileExtensions.Sms),
            [DiskImageFormatIds.SegaMegaDrive] = Extensions(DiskImageFileExtensions.Md, DiskImageFileExtensions.Gen),
            [DiskImageFormatIds.SegaGameGear] = Extensions(DiskImageFileExtensions.Gg),
            [DiskImageFormatIds.SegaThirtyTwoX] = Extensions(DiskImageFileExtensions.ThirtyTwoX),
            [DiskImageFormatIds.NecPcEngine] = Extensions(DiskImageFileExtensions.Pce),
            [DiskImageFormatIds.Atari2600] = Extensions(DiskImageFileExtensions.A26),
            [DiskImageFormatIds.Atari5200] = Extensions(DiskImageFileExtensions.A52),
            [DiskImageFormatIds.Atari7800] = Extensions(DiskImageFileExtensions.A78),
            [DiskImageFormatIds.BandaiWonderSwan] = Extensions(DiskImageFileExtensions.Ws, DiskImageFileExtensions.Wsc)
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlySet<string> SupportedFormatIds =
        FormatExtensions.Keys.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions =
        FormatExtensions.Values.SelectMany(static extensions => extensions)
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();

    public string Id => MediaImageWriterIds.ConsoleCartridge;
    public IReadOnlySet<string> FormatIds => SupportedFormatIds;
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentations;
    public IReadOnlySet<string> ProducedFileExtensions => SupportedExtensions;
    public bool ProducesMultipleFiles => false;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension)
    {
        ArgumentNullException.ThrowIfNull(document);
        return document.Representation is IMediaBlockRepresentation blocks
            && blocks.Capacity > 0
            && SupportedFormatIds.Contains(targetFormatId)
            && SupportedExtensions.Contains(NormalizeExtension(targetExtension))
            && FormatExtensions[targetFormatId].Contains(NormalizeExtension(targetExtension))
            && document.Metadata.TryGetValue(ConsoleCartridgeConstants.StoredLength, out var length)
            && long.TryParse(length, NumberStyles.None, CultureInfo.InvariantCulture, out var storedLength)
            && storedLength > 0 && storedLength <= blocks.Capacity;
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
            throw new NotSupportedException("The selected console cartridge target is not supported.");
        if (document.Representation is not IMediaBlockRepresentation blocks
            || !document.Metadata.TryGetValue(ConsoleCartridgeConstants.StoredLength, out var text)
            || !long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var storedLength))
            throw new InvalidDataException("The console cartridge data length is missing or invalid.");

        var atomic = new AtomicImageFileWriter();
        await atomic.WriteAsync(outputPath, async (stream, token) =>
        {
            var buffer = new byte[256 * 1024];
            long offset = 0;
            while (offset < storedLength)
            {
                token.ThrowIfCancellationRequested();
                var count = (int)Math.Min(buffer.Length, storedLength - offset);
                await blocks.ReadExactlyAsync(offset, buffer.AsMemory(0, count), token).ConfigureAwait(false);
                await stream.WriteAsync(buffer.AsMemory(0, count), token).ConfigureAwait(false);
                offset += count;
            }
        }, cancellationToken).ConfigureAwait(false);
        return [outputPath];
    }

    private static string NormalizeExtension(string extension) =>
        extension.StartsWith(".", StringComparison.Ordinal)
            ? extension.ToLowerInvariant()
            : $".{extension.ToLowerInvariant()}";

    private static IReadOnlySet<string> Extensions(params string[] extensions) =>
        extensions.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
}
