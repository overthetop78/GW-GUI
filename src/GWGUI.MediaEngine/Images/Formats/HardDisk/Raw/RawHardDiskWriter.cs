using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;
using GWGUI.MediaEngine.Images.Models.Blocks;

namespace GWGUI.MediaEngine.Images.Formats.HardDisk.Raw;

/// <summary>Writes a complete block representation as a headerless hard disk image.</summary>
public sealed class RawHardDiskWriter : IMediaImageWriter
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { HardDiskImageFormatIds.Raw }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();
    private readonly IAtomicImageFileWriter files;

    public RawHardDiskWriter(IAtomicImageFileWriter? files = null)
    {
        this.files = files ?? new AtomicImageFileWriter();
    }

    public string Id => MediaImageWriterIds.HardDiskRaw;

    public IReadOnlySet<string> FormatIds => SupportedFormatIds;

    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentationKinds;

    public IReadOnlySet<string> ProducedFileExtensions => RawHardDiskFormat.Extensions;

    public bool ProducesMultipleFiles => false;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (!SupportedFormatIds.Contains(targetFormatId)
            || !RawHardDiskFormat.Extensions.Contains(targetExtension)
            || document.MediaKind != MediaKind.HardDisk
            || document.Representation is not BlockMediaImageRepresentation blocks)
            return false;

        return HasCompleteReadableCoverage(blocks);
    }

    public async Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(outputPath).ToLowerInvariant();
        if (!CanWrite(document, targetFormatId, extension)
            || document.Representation is not BlockMediaImageRepresentation blocks)
            throw new InvalidDataException("The media document cannot be written as a complete raw hard disk image.");

        await files.WriteAsync(
            outputPath,
            (output, token) => WriteRangesAsync(blocks, output, token),
            cancellationToken).ConfigureAwait(false);
        return [outputPath];
    }

    private static bool HasCompleteReadableCoverage(BlockMediaImageRepresentation blocks)
    {
        if (blocks.Capacity <= 0 || !RawHardDiskFormat.IsLengthCompatible(blocks.Capacity, blocks.LogicalBlockSize))
            return false;

        long cursor = 0;
        foreach (var range in blocks.Ranges.OrderBy(range => range.Address))
        {
            if (range.Address != cursor || range.Kind == MediaDataRangeKind.Unavailable)
                return false;
            cursor = checked(cursor + range.Length);
        }

        return cursor == blocks.Capacity;
    }

    private static async Task WriteRangesAsync(
        BlockMediaImageRepresentation blocks,
        Stream output,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[64 * DataSizeConstants.BytesPerKibibyte];
        foreach (var range in blocks.Ranges.OrderBy(range => range.Address))
        {
            long written = 0;
            while (written < range.Length)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var count = (int)Math.Min(buffer.Length, range.Length - written);
                var destination = buffer.AsMemory(0, count);
                if (range.Kind == MediaDataRangeKind.Stored)
                {
                    await range.Source!.ReadExactlyAsync(
                        checked(range.SourceOffset + written),
                        destination,
                        cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    destination.Span.Clear();
                }

                await output.WriteAsync(destination, cancellationToken).ConfigureAwait(false);
                written += count;
            }
        }
    }
}
