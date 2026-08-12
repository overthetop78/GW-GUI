using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Commodore.Dos;

/// <summary>Lit les deux secteurs BAM d'un volume 1581.</summary>
internal static class Commodore1581BamReader
{
    /// <summary>Compte les blocs libres et distingue un BAM illisible d'un volume plein.</summary>
    public static CommodoreDosFreeSpace Read(SectorImage image, List<string> warnings)
    {
        var free = 0;
        foreach (var sector in new[] { Commodore1581DosLayout.FirstBamSector, Commodore1581DosLayout.SecondBamSector })
        {
            var status = CommodoreDosSectorReader.Read(image, Commodore1581DosLayout.HeaderTrack, sector, out var bam);
            if (status != CommodoreDosSectorReadStatus.Success)
            {
                warnings.Add(status == CommodoreDosSectorReadStatus.Truncated ? CommodoreDosWarnings.BamTruncated(Commodore1581DosLayout.HeaderTrack, sector, bam.Count) : CommodoreDosWarnings.BamMissing(Commodore1581DosLayout.HeaderTrack, sector));
                return new(null);
            }
            var requiredLength = Commodore1581DosLayout.BamEntriesOffset + Commodore1581DosLayout.BamEntryCount * Commodore1581DosLayout.BamEntrySize;
            if (bam.Count < requiredLength)
            {
                warnings.Add(CommodoreDosWarnings.BamTruncated(Commodore1581DosLayout.HeaderTrack, sector, bam.Count));
                return new(null);
            }
            for (var index = 0; index < Commodore1581DosLayout.BamEntryCount; index++) free += bam[Commodore1581DosLayout.BamEntriesOffset + index * Commodore1581DosLayout.BamEntrySize];
        }
        return new(free);
    }
}
