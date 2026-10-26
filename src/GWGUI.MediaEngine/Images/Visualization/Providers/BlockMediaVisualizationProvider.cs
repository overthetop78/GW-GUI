using System.Collections.Frozen;
using System.Globalization;
using System.IO;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces.Visualization;
using GWGUI.MediaEngine.Images.Formats.Cartridge.Console;
using GWGUI.MediaEngine.Images.Models.Blocks;
using GWGUI.MediaEngine.Images.Reading.Blocks;

namespace GWGUI.MediaEngine.Images.Visualization.Providers;

/// <summary>Builds progressive visualization data from the declared 64-bit ranges of block media.</summary>
public sealed class BlockMediaVisualizationProvider : IMediaVisualizationProvider
{
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedKinds = new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();

    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedKinds;

    public bool CanProvide(MediaImageDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return document.Representation is BlockMediaImageRepresentation;
    }

    public MediaVisualizationDescriptor CreateDescriptor(MediaImageDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (document.Representation is not BlockMediaImageRepresentation blocks)
            throw new NotSupportedException($"Representation '{document.Representation.RepresentationKind}' is not a block representation.");

        var visualLength = ResolveVisualLength(document, blocks);
        if (document.MediaKind == MediaKind.HardDisk && blocks.Geometry is { } geometry)
        {
            var surfaces = Enumerable.Range(0, geometry.Heads).ToArray();
            var tracks = new List<MediaVisualizationElement>();
            for (long cylinder = 0; cylinder < geometry.Cylinders; cylinder++)
                foreach (var surface in surfaces)
                    tracks.Add(new((cylinder * geometry.Heads + surface) * geometry.SectorsPerTrack, surface, geometry.SectorsPerTrack));
            if (geometry.LogicalSectorCount < blocks.LogicalBlockCount) tracks.Add(new(geometry.LogicalSectorCount, null, blocks.LogicalBlockCount - geometry.LogicalSectorCount));
            return new(MediaRepresentationKind.Blocks, surfaces, MediaVisualizationProgressUnit.BlockRange, MediaVisualizationDirection.Ascending, tracks);
        }
        var boundaries = new SortedSet<long> { 0, visualLength };
        foreach (var range in blocks.Ranges)
        {
            if (range.Address >= visualLength) continue;
            boundaries.Add(range.Address);
            boundaries.Add(Math.Min(visualLength, checked(range.Address + range.Length)));
        }
        foreach (var volume in document.Volumes)
        {
            if (volume.Start >= visualLength) continue;
            boundaries.Add(volume.Start);
            var end = checked(volume.Start + volume.Length);
            boundaries.Add(Math.Min(visualLength, end));
        }
        var points = boundaries.ToArray();
        var elements = new MediaVisualizationElement[points.Length - 1];
        for (var index = 0; index < elements.Length; index++)
            elements[index] = new MediaVisualizationElement(points[index] / blocks.LogicalBlockSize, 0, (points[index + 1] - points[index] + blocks.LogicalBlockSize - 1) / blocks.LogicalBlockSize);
        return new(MediaRepresentationKind.Blocks, [0], MediaVisualizationProgressUnit.BlockRange, MediaVisualizationDirection.Ascending, elements);
    }

    /// <summary>Vérifie les octets d'une plage LBA avec un tampon borné ; une annulation reste propagée.</summary>
    /// <remarks>La disponibilité dans le catalogue ne remplace pas une lecture effective de la source.</remarks>
    public static async Task<bool> ReadElementAsync(MediaImageDocument document, MediaVisualizationElement element, CancellationToken token = default)
    {
        if (document.Representation is not BlockMediaImageRepresentation blocks) throw new ArgumentException("A block image is required.", nameof(document));
        var address = checked(element.Position * blocks.LogicalBlockSize);
        var length = checked((element.Length ?? 1) * blocks.LogicalBlockSize);
        if (address > blocks.Capacity || length > blocks.Capacity - address) throw new ArgumentOutOfRangeException(nameof(element));
        var buffer = new byte[(int)Math.Min(64 * 1024L, length)];
        try
        {
            for (long offset = 0; offset < length;)
            {
                token.ThrowIfCancellationRequested();
                var count = (int)Math.Min(buffer.Length, length - offset);
                await BlockMediaDataReader.ReadExactlyAsync(blocks, address + offset, buffer.AsMemory(0, count), token).ConfigureAwait(false);
                offset += count;
            }
            return true;
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException) { return false; }
    }

    private static long ResolveVisualLength(MediaImageDocument document, BlockMediaImageRepresentation blocks)
    {
        if (document.MediaKind != MediaKind.Cartridge
            || !document.Metadata.TryGetValue(ConsoleCartridgeConstants.StoredLength, out var text)
            || !long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var storedLength)
            || storedLength <= 0 || storedLength > blocks.Capacity)
            return blocks.Capacity;

        return storedLength;
    }

}
