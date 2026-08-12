using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Fat12;

/// <summary>Parcourt une chaîne FAT12 en distinguant chaque cause d'invalidité.</summary>
internal static class Fat12ClusterChainReader
{
    /// <summary>Lit une chaîne avec un ensemble de clusters propre à cet appel.</summary>
    public static Fat12ClusterChain Read(SectorImage image, FatSectorRange fat, Fat12Layout layout, int firstCluster, List<string> warnings, string name)
    {
        if (firstCluster < Fat12Table.FirstDataCluster) return new([], true, []);
        using var stream = new MemoryStream();
        var visited = new HashSet<int>();
        var cluster = firstCluster;
        var valid = fat.IsValid;
        while (cluster < Fat12Table.FirstEndOfChain)
        {
            if (cluster < Fat12Table.FirstDataCluster || cluster >= layout.ClusterCount + Fat12Table.FirstDataCluster)
            {
                warnings.Add(Fat12FileSystemExceptions.ClusterOutsideRange(name, cluster));
                valid = false;
                break;
            }
            if (!visited.Add(cluster))
            {
                warnings.Add(Fat12FileSystemExceptions.CyclicChain(name, cluster));
                valid = false;
                break;
            }
            var firstSector = layout.DataStart + (cluster - Fat12Table.FirstDataCluster) * layout.SectorsPerCluster;
            var sectors = FatSectorReader.Read(image, firstSector, layout.SectorsPerCluster, warnings);
            stream.Write(sectors.Bytes);
            valid &= sectors.IsValid;
            if (!Fat12Table.TryRead(fat.Bytes, cluster, out cluster))
            {
                warnings.Add(Fat12FileSystemExceptions.TruncatedTable(cluster).Message);
                valid = false;
                break;
            }
        }
        return new(stream.ToArray(), valid, visited);
    }
}
