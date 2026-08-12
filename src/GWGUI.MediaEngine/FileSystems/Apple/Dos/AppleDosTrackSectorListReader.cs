using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Apple.Dos;

/// <summary>Reconstruit un fichier depuis sa chaîne de listes piste/secteur.</summary>
public static class AppleDosTrackSectorListReader
{
    /// <summary>Lit les listes et données accessibles tout en conservant leur validité.</summary>
    public static AppleDosFileData Read(SectorImage image, int track, int sector, ICollection<string> warnings, string name)
    {
        using var output = new MemoryStream();
        var visited = new HashSet<int>();
        var firstReference = AppleDosFileSystemLayout.IsValidAddress(track, sector, image.Cylinders, image.SectorsPerTrack) ? track * image.SectorsPerTrack + sector : -1;
        var traversed = 0;
        var valid = true;
        while (track != 0)
        {
            if (!AppleDosFileSystemLayout.IsValidAddress(track, sector, image.Cylinders, image.SectorsPerTrack))
            {
                warnings.Add(AppleDosFileSystemWarnings.InvalidAddress(name, track, sector));
                valid = false;
                break;
            }
            var logical = track * image.SectorsPerTrack + sector;
            if (!visited.Add(logical))
            {
                warnings.Add(AppleDosFileSystemWarnings.CyclicList(name, track, sector));
                valid = false;
                break;
            }
            if (!image.TryGetBlock(logical, out var list) || list.Data.Count != AppleDosFileSystemLayout.SectorSize)
            {
                warnings.Add(AppleDosFileSystemWarnings.MissingList(name, track, sector));
                valid = false;
                break;
            }
            traversed++;
            var data = list.Data.ToArray();
            for (var pairIndex = 0; pairIndex < AppleDosFileSystemLayout.TrackSectorPairCount; pairIndex++)
            {
                var offset = AppleDosFileSystemLayout.TrackSectorPairsOffset + pairIndex * AppleDosFileSystemLayout.TrackSectorPairSize;
                var dataTrack = data[offset];
                var dataSector = data[offset + AppleDosFileSystemLayout.EntrySectorOffset];
                if (dataTrack == 0) continue;
                traversed++;
                if (!AppleDosFileSystemLayout.IsValidAddress(dataTrack, dataSector, image.Cylinders, image.SectorsPerTrack))
                {
                    warnings.Add(AppleDosFileSystemWarnings.InvalidAddress(name, dataTrack, dataSector));
                    valid = false;
                    continue;
                }
                if (!image.TryGetBlock(dataTrack * image.SectorsPerTrack + dataSector, out var block))
                {
                    warnings.Add(AppleDosFileSystemWarnings.MissingData(name, dataTrack, dataSector));
                    valid = false;
                    continue;
                }
                foreach (var value in block.Data) output.WriteByte(value);
            }
            track = data[AppleDosFileSystemLayout.NextTrackOffset];
            sector = data[AppleDosFileSystemLayout.NextSectorOffset];
        }
        return new(output.ToArray(), valid, firstReference, traversed);
    }
}
