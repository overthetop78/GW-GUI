using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Geometries.Msx;
using GWGUI.MediaEngine.Recognition.Atari;
using GWGUI.MediaEngine.Recognition.Msx;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Interpretations;

/// <summary>Fournit les transformations techniques partagées entre les interprétations d'images sectorielles.</summary>
internal static class SectorImageInterpretation
{
    /// <summary>Crée une vue de l'image portant un nouvel identifiant sans modifier ses blocs.</summary>
    public static SectorImage Retag(SectorImage image, string formatId) => new(formatId, image.BlockSize, image.Cylinders, image.Heads, image.SectorsPerTrack, image.AvailableBlocks, image.AvailableBlocks.Any(block => block.Data.Count != image.BlockSize), image.Capacity, image.BlockCount);

    /// <summary>Indique si une arborescence contient un programme Atari ST.</summary>
    public static bool ContainsAtariStProgram(IEnumerable<FileSystemEntry> entries)
    {
        foreach (var entry in entries)
        {
            if (entry.Kind == FileSystemEntryKind.File)
            {
                var extension = Path.GetExtension(entry.Name);
                if (AtariProgramDefinitions.Extensions.Contains(extension) || entry.Content is not null && entry.Content.Count >= AtariProgramDefinitions.Signature.Length && entry.Content.Take(AtariProgramDefinitions.Signature.Length).SequenceEqual(AtariProgramDefinitions.Signature.ToArray())) return true;
            }
            if (ContainsAtariStProgram(entry.Children)) return true;
        }
        return false;
    }

    /// <summary>Lit et valide la géométrie FAT annoncée par le secteur d'amorçage.</summary>
    public static bool TryReadFatGeometry(SectorImage image, out int cylinders, out int heads, out int sectorsPerTrack, out int totalSectors)
    {
        cylinders = heads = sectorsPerTrack = totalSectors = 0;
        if (image.BlockSize != FatBootSectorLayout.SectorSize || !image.TryGetBlock(0, out var boot) || boot.Data.Count < FatBootSectorLayout.MinimumLength) return false;
        var bytes = boot.Data.ToArray().AsSpan();
        var bytesPerSector = BinaryPrimitives.ReadUInt16LittleEndian(bytes[FatBootSectorLayout.BytesPerSectorOffset..]);
        totalSectors = BinaryPrimitives.ReadUInt16LittleEndian(bytes[FatBootSectorLayout.TotalSectors16Offset..]);
        if (totalSectors == 0) totalSectors = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(bytes[FatBootSectorLayout.TotalSectors32Offset..]));
        sectorsPerTrack = BinaryPrimitives.ReadUInt16LittleEndian(bytes[FatBootSectorLayout.SectorsPerTrackOffset..]);
        heads = BinaryPrimitives.ReadUInt16LittleEndian(bytes[FatBootSectorLayout.HeadCountOffset..]);
        if (bytesPerSector != FatBootSectorLayout.SectorSize || totalSectors <= 0 || sectorsPerTrack <= 0 || heads <= 0 || totalSectors > image.BlockCount || totalSectors % (sectorsPerTrack * heads) != 0) return false;
        cylinders = totalSectors / (sectorsPerTrack * heads);
        return cylinders > 0;
    }

    /// <summary>Tente de réidentifier une image comme une géométrie MSX connue.</summary>
    public static bool TryCreateMsx(SectorImage image, out SectorImage interpretation)
    {
        interpretation = null!;
        if (image.FormatId.StartsWith(DiskImageFormatIds.MsxPrefix, StringComparison.OrdinalIgnoreCase) || !image.TryGetBlock(0, out var boot) || boot.Data.Count != FatBootSectorLayout.SectorSize || !MsxBootSectorProbe.LooksLikeMsx(boot.Data.ToArray())) return false;
        var geometry = MsxDiskGeometryCatalog.Find(checked(image.BlockCount * image.BlockSize), boot.Data[FatBootSectorLayout.MediaDescriptorOffset]);
        if (geometry is null) return false;
        interpretation = Retag(image, geometry.FormatId);
        return true;
    }
}
