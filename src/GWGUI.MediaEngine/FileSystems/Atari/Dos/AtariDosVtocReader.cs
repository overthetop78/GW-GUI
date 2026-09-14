using System.Buffers.Binary;

using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.FileSystems.Atari.Dos;

/// <summary>Valide le VTOC Atari DOS et lit son espace libre optionnel.</summary>
public static class AtariDosVtocReader
{
    /// <summary>Indique si le secteur possède le marqueur et la longueur minimale attendus.</summary>
    public static bool LooksValid(IReadOnlyList<byte> data, int maximumSectorCount)
    {
        if (data.Count < AtariDosFileSystemLayout.MinimumSectorSize ||
            data[0] is not (AtariDosFileSystemLayout.LegacyVtocMarker
                or AtariDosFileSystemLayout.VtocMarker
                or AtariDosFileSystemLayout.DoubleDensityVtocMarker
                or AtariDosFileSystemLayout.ExtendedVtocMarker)) return false;
        var usable = data[AtariDosFileSystemLayout.UsableSectorCountOffset] |
            data[AtariDosFileSystemLayout.UsableSectorCountOffset + 1] << 8;
        var free = data[AtariDosFileSystemLayout.FreeSectorCountOffset] |
            data[AtariDosFileSystemLayout.FreeSectorCountOffset + 1] << 8;
        return usable > 0 && usable <= maximumSectorCount && free <= usable;
    }
    /// <summary>Lit le compteur libre lorsqu'il est disponible.</summary>
    public static int? ReadFreeSectors(SectorImage image)
    {
        if (!TrySector(image, AtariDosFileSystemLayout.VtocSector, out var data) || data.Length < AtariDosFileSystemLayout.FreeSectorCountOffset + AtariDosFileSystemLayout.FreeSectorCountLength) return null;
        return BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(AtariDosFileSystemLayout.FreeSectorCountOffset, AtariDosFileSystemLayout.FreeSectorCountLength));
    }
    /// <summary>Lit une copie du secteur Atari numéroté depuis un.</summary>
    public static bool TrySector(SectorImage image, int sectorNumber, out byte[] data)
    {
        data = [];
        if (sectorNumber <= 0 || !image.TryGetBlock(sectorNumber - 1, out var block)) return false;
        data = block.Data.ToArray();
        return true;
    }
}
