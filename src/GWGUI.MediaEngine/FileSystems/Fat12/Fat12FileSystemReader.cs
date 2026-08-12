using System.Collections.Frozen;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Fat12;

/// <summary>Lit les volumes FAT12 contenus dans les images Atari ST, IBM PC et MSX.</summary>
public sealed class Fat12FileSystemReader : IFileSystemReader
{
    /// <summary>Obtient l'identifiant technique central FAT12.</summary>
    public string Id => Definitions.FileSystemIds.Fat12;
    /// <summary>Obtient l'ensemble immuable des formats associés explicitement à FAT12.</summary>
    public IReadOnlySet<string> CatalogFormatIds { get; } = Fat12FormatCatalog.FileSystemIdByFormat.Keys.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>Indique si le secteur d'amorçage, la disposition et l'en-tête de FAT sont plausibles.</summary>
    public bool CanRead(SectorImage image)
    {
        if (!CatalogFormatIds.Contains(image.FormatId) || image.BlockSize != FatBootSectorLayout.SectorSize || !image.TryGetBlock(0, out var boot) || boot.Data.Count < FatBootSectorLayout.ExtendedBootMinimumLength) return false;
        var mediaDescriptor = boot.Data[FatBootSectorLayout.MediaDescriptorOffset];
        return Fat12LayoutReader.TryRead(boot.Data.ToArray(), image.BlockCount, image.FormatId, out var layout) && Fat12FatReader.HasReadableCopy(image, layout, mediaDescriptor);
    }

    /// <summary>Lit le volume, son espace libre et son arborescence en conservant les secteurs absents.</summary>
    public FileSystemVolume Read(SectorImage image)
    {
        if (!image.TryGetBlock(0, out var boot) || !Fat12LayoutReader.TryRead(boot.Data.ToArray(), image.BlockCount, image.FormatId, out var layout)) throw Fat12FileSystemExceptions.UnsupportedLayout(image.FormatId, image.TryGetBlock(0, out var availableBoot) ? availableBoot.Data : null);
        var warnings = new List<string>();
        var mediaDescriptor = boot.Data[FatBootSectorLayout.MediaDescriptorOffset];
        var fat = Fat12FatReader.ReadBest(image, layout, mediaDescriptor, warnings);
        if (!fat.IsValid || !Fat12FatReader.IsUsable(fat.Bytes, mediaDescriptor, layout.ClusterCount)) throw Fat12FileSystemExceptions.UnsupportedLayout(image.FormatId, boot.Data);
        var root = FatSectorReader.Read(image, layout.RootStart, layout.RootSectors, warnings);
        var entries = Fat12DirectoryReader.Read(image, root, fat, layout, warnings, 0, string.Empty);
        var freeClusters = 0;
        if (fat.IsValid)
        {
            for (var cluster = Fat12Table.FirstDataCluster; cluster < Fat12Table.FirstDataCluster + layout.ClusterCount; cluster++)
            {
                if (Fat12Table.TryRead(fat.Bytes, cluster, out var value) && value == Fat12Table.FreeCluster) freeClusters++;
            }
        }
        var label = FatDirectoryEntryReader.ReadVolumeLabel(root.Bytes) ?? FatDirectoryEntryReader.ReadBootVolumeLabel(boot.Data);
        var freeBytes = fat.IsValid ? (long)freeClusters * layout.SectorsPerCluster * FatBootSectorLayout.SectorSize : 0;
        return new(label, Definitions.FileSystemIds.Fat12, image.Capacity, freeBytes, null, null, entries, warnings);
    }
}
