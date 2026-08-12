using GWGUI.MediaEngine.Geometries.Commodore;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Commodore.Dos;

/// <summary>Lit les BAM des volumes 1541 et des deux faces 1571.</summary>
internal static class Commodore1541BamReader
{
    /// <summary>Compte les blocs libres et signale tout BAM requis absent ou tronqué.</summary>
    public static CommodoreDosFreeSpace Read(SectorImage image, List<string> warnings)
    {
        var free = 0;
        if (!ReadSide(image, Commodore1541DosLayout.HeaderTrack, image.Cylinders, ref free, warnings)) return new(null);
        if (image.Heads == Commodore1571Geometry.SideCount && !ReadSide(image, Commodore1541DosLayout.HeaderTrack + image.Cylinders, image.Cylinders, ref free, warnings)) return new(null);
        return new(free);
    }

    /// <summary>Lit le BAM d'une face avec le nombre réel de pistes de l'image.</summary>
    private static bool ReadSide(SectorImage image, int track, int trackCount, ref int free, List<string> warnings)
    {
        var status = CommodoreDosSectorReader.Read(image, track, Commodore1541DosLayout.HeaderSector, out var bam);
        if (status != CommodoreDosSectorReadStatus.Success)
        {
            warnings.Add(status == CommodoreDosSectorReadStatus.Truncated ? CommodoreDosWarnings.BamTruncated(track, Commodore1541DosLayout.HeaderSector, bam.Count) : CommodoreDosWarnings.BamMissing(track, Commodore1541DosLayout.HeaderSector));
            return false;
        }
        var requiredLength = Commodore1541DosLayout.BamEntriesOffset + trackCount * Commodore1541DosLayout.BamEntrySize;
        if (bam.Count < requiredLength)
        {
            warnings.Add(CommodoreDosWarnings.BamTruncated(track, Commodore1541DosLayout.HeaderSector, bam.Count));
            return false;
        }
        for (var index = 0; index < trackCount; index++) free += bam[Commodore1541DosLayout.BamEntriesOffset + index * Commodore1541DosLayout.BamEntrySize];
        return true;
    }
}
