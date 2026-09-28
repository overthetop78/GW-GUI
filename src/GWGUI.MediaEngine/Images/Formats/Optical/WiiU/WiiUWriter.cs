using System.Collections.Frozen;
using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Blocks;
using GWGUI.MediaEngine.Images.Writing;
using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;
using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaEngine.Images.Formats.Optical.WiiU;

/// <summary>Écrit une image Wii U logique complète au format WUD.</summary>
public sealed class WiiUWriter : IMediaImageWriter
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { DiskImageFormatIds.NintendoWiiU }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions =
        new[] { DiskImageFileExtensions.Wud }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();
    private readonly IAtomicImageFileWriter files;

    public WiiUWriter(IAtomicImageFileWriter? files = null) => this.files = files ?? new AtomicImageFileWriter();

    public string Id => MediaImageWriterIds.NintendoWiiU;
    public IReadOnlySet<string> FormatIds => SupportedFormatIds;
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentations;
    public IReadOnlySet<string> ProducedFileExtensions => SupportedExtensions;
    public bool ProducesMultipleFiles => false;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension)
    {
        ArgumentNullException.ThrowIfNull(document);
        return document.MediaKind == MediaKind.Optical
            && SupportedFormatIds.Contains(targetFormatId)
            && SupportedExtensions.Contains(NormalizeExtension(targetExtension))
            && document.Representation is BlockMediaImageRepresentation blocks
            && WiiUFormat.IsWudLengthCompatible(blocks.Capacity)
            && HasCompleteReadableCoverage(blocks);
    }

    public async Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        if (!CanWrite(document, targetFormatId, Path.GetExtension(outputPath))
            || document.Representation is not BlockMediaImageRepresentation blocks)
            throw new InvalidDataException("The Nintendo Wii U document cannot be written as a WUD image.");
        var readable = (IMediaBlockRepresentation)blocks;

        await files.WriteAsync(outputPath, async (output, token) =>
        {
            var buffer = new byte[256 * 1024];
            long offset = 0;
            while (offset < blocks.Capacity)
            {
                token.ThrowIfCancellationRequested();
                var count = (int)Math.Min(buffer.Length, blocks.Capacity - offset);
                await readable.ReadExactlyAsync(offset, buffer.AsMemory(0, count), token).ConfigureAwait(false);
                await output.WriteAsync(buffer.AsMemory(0, count), token).ConfigureAwait(false);
                offset += count;
            }
        }, cancellationToken).ConfigureAwait(false);
        return [outputPath];
    }

    private static bool HasCompleteReadableCoverage(BlockMediaImageRepresentation blocks)
    {
        if (blocks.Capacity <= 0 || blocks.Ranges.Count == 0)
            return false;
        long expected = 0;
        foreach (var range in blocks.Ranges.OrderBy(range => range.Address))
        {
            if (range.Address != expected || range.Length <= 0 || range.Kind == MediaDataRangeKind.Unavailable)
                return false;
            expected = checked(expected + range.Length);
        }
        return expected == blocks.Capacity;
    }

    private static string NormalizeExtension(string extension) =>
        extension.StartsWith(".", StringComparison.Ordinal)
            ? extension.ToLowerInvariant()
            : $".{extension.ToLowerInvariant()}";
}
