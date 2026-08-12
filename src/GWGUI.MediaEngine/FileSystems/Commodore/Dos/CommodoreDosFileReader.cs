using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Geometries.Commodore;

namespace GWGUI.MediaEngine.FileSystems.Commodore.Dos;

/// <summary>Reconstruit une chaîne de secteurs de fichier Commodore DOS.</summary>
internal static class CommodoreDosFileReader
{
    /// <summary>Lit une chaîne et conserve un état invalide lorsque sa lecture reste partielle.</summary>
    public static CommodoreDosFileReadResult Read(SectorImage image, int firstTrack, int firstSector, List<string> warnings, string name)
    {
        if (firstTrack == 0) return new([], true, [], [], null);
        int? firstLogicalBlock = CommodoreDosGeometry.TryToLogicalBlock(image, firstTrack, firstSector, out var firstBlock) ? firstBlock : null;
        var firstWarningIndex = warnings.Count;
        using var stream = new MemoryStream();
        var visited = new HashSet<(int Track, int Sector)>();
        var track = firstTrack;
        var sector = firstSector;
        var valid = true;
        while (track != 0)
        {
            if (!visited.Add((track, sector)))
            {
                warnings.Add(CommodoreDosWarnings.FileReadFailure(name, $"la chaîne est cyclique en {track}/{sector}."));
                valid = false;
                break;
            }
            var sectorStatus = CommodoreDosSectorReader.Read(image, track, sector, out var data);
            if (sectorStatus != CommodoreDosSectorReadStatus.Success)
            {
                warnings.Add(CommodoreDosWarnings.FileReadFailure(name, sectorStatus switch { CommodoreDosSectorReadStatus.InvalidCoordinate => $"la coordonnée {track}/{sector} est invalide.", CommodoreDosSectorReadStatus.Truncated => $"le secteur {track}/{sector} est tronqué.", _ => $"le secteur {track}/{sector} est absent." }));
                valid = false;
                break;
            }
            var nextTrack = data[CommodoreDosLayout.NextTrackOffset];
            var nextSector = data[CommodoreDosLayout.NextSectorOffset];
            var used = CommodoreDosLayout.DataBytesPerSector;
            if (nextTrack == 0)
            {
                if (nextSector is < 1 or > CommodoreDosLayout.DataBytesPerSector + 1)
                {
                    warnings.Add(CommodoreDosWarnings.FileReadFailure(name, $"le compteur final {nextSector} est invalide."));
                    valid = false;
                    break;
                }
                used = nextSector - 1;
            }
            for (var index = 0; index < used; index++) stream.WriteByte(data[CommodoreDosLayout.LinkLength + index]);
            track = nextTrack;
            sector = nextSector;
            if (stream.Length > image.Capacity)
            {
                warnings.Add(CommodoreDosWarnings.CapacityExceeded(name));
                valid = false;
                break;
            }
        }
        return new(stream.ToArray(), valid, visited, warnings.Skip(firstWarningIndex), firstLogicalBlock);
    }
}
