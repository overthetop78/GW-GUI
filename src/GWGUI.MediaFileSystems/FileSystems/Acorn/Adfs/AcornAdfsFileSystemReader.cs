using System.Collections.Frozen;
using GWGUI.MediaFileSystems.FileSystems.Acorn.FileCore;
using GWGUI.MediaFileSystems.Constants;
using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaFileSystems.FileSystems.Acorn.Adfs;
/// <summary>Lit les catalogues Acorn ADFS fondés sur une carte FileCore.</summary>
public sealed partial class AcornAdfsFileSystemReader : IFileSystemReader
{
    /// <inheritdoc/>
    public string Id => Definitions.FileSystemIds.AcornAdfs;
    /// <inheritdoc/>
    public IReadOnlySet<string> CatalogFormatIds { get; } = new[]
    {
        MediaImageFormatIds.AcornAdfs800
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public bool CanRead(IMediaSectorImage image) => (image.BlockSize == AcornAdfsLayout.FileCoreUnitSize || CatalogFormatIds.Contains(image.FormatId) && image.BlockSize == AcornAdfsLayout.BlockSize) && TryCreateLayout(image,
        out var layout) && AcornAdfsDirectoryReader.TryRead(image, layout.RootAddress, layout, out _);
    /// <inheritdoc/>
    public FileSystemVolume Read(IMediaSectorImage image)
    {
        if (!CanRead(image) || !TryCreateLayout(image, out var layout))
            throw AcornAdfsExceptions.UnsupportedImage(image.BlockSize, image.BlockCount);
        var warnings = new List<string>();
        var root = AcornAdfsDirectoryReader.Read(image, layout.RootAddress, layout, new HashSet<int>(), warnings, 0);
        return new(layout.VolumeName.Length == 0 ? root.Name : layout.VolumeName, Definitions.FileSystemIds.AcornAdfs, image.Capacity, layout.FreeBytes, null, null, root.Children, warnings);
    }

    /// <summary>Crée le résolveur new-map ou old-map applicable à l'image.</summary>
    private static bool TryCreateLayout(IMediaSectorImage image, out IFileCoreAddressResolver layout)
    {
        if (image.BlockSize == AcornAdfsLayout.FileCoreUnitSize)
        {
            if (!image.TryGetBlock(0, out var first) || !image.TryGetBlock(1, out var second)) { layout = null!; return false; }
            var interleaved = image.Heads == 2 && image.SectorsPerTrack == 16;
            layout = new AcornFileCoreOldMap(first.Data.Concat(second.Data).ToArray(), image.Capacity, 2, interleaved ? image.Cylinders : 0, interleaved ? image.Heads : 1, interleaved ? image.SectorsPerTrack : 0);
            return true;
        }
        if (AcornFileCoreNewMap.TryCreate(image, out var map) && map is not null)
        {
            layout = map;
            return true;
        }

        if (!image.TryGetBlock(0, out var firstBlock))
        {
            layout = null!;
            return false;
        }

        layout = new AcornFileCoreOldMap(firstBlock.Data.ToArray(), image.Capacity);
        return true;
    }
}
