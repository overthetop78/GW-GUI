using System.Collections.Frozen;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Acorn.FileCore;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Acorn.Adfs;

/// <summary>Lit les catalogues Acorn ADFS fondés sur une carte FileCore.</summary>
public sealed class AcornAdfsFileSystemReader : IFileSystemReader
{
    /// <inheritdoc />
    public string Id => Definitions.FileSystemIds.AcornAdfs;

    /// <inheritdoc />
    public IReadOnlySet<string> CatalogFormatIds { get; } = new[] { DiskImageFormatIds.AcornAdfs800 }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public bool CanRead(SectorImage image) => CatalogFormatIds.Contains(image.FormatId) && image.BlockSize == AcornAdfsLayout.BlockSize && image.BlockCount == AcornAdfsLayout.ImageBlockCount && TryCreateLayout(image, out var layout) && AcornAdfsDirectoryReader.TryRead(image, layout.RootAddress, layout, out _);

    /// <inheritdoc />
    public FileSystemVolume Read(SectorImage image)
    {
        if (!CatalogFormatIds.Contains(image.FormatId) || image.BlockSize != AcornAdfsLayout.BlockSize || image.BlockCount != AcornAdfsLayout.ImageBlockCount || !TryCreateLayout(image, out var layout) || !AcornAdfsDirectoryReader.TryRead(image, layout.RootAddress, layout, out _)) throw AcornAdfsExceptions.UnsupportedImage(image.BlockSize, image.BlockCount);
        var warnings = new List<string>();
        var root = AcornAdfsDirectoryReader.Read(image, layout.RootAddress, layout, new HashSet<int>(), warnings, 0);
        return new(layout.VolumeName.Length == 0 ? root.Name : layout.VolumeName, Definitions.FileSystemDisplayNames.AcornAdfs, image.Capacity, layout.FreeBytes, null, null, root.Children, warnings);
    }

    /// <summary>Crée le résolveur new-map ou old-map applicable à l'image.</summary>
    private static bool TryCreateLayout(SectorImage image, out IFileCoreAddressResolver layout)
    {
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
