using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Mfs;

/// <summary>Reconstruit un fork MFS depuis sa chaîne d'allocation.</summary>
internal static class MacMfsForkReader
{
    /// <summary>Lit un fork et conserve la position de chaque secteur absent ou de mauvaise taille.</summary>
    public static MacMfsForkResult Read(SectorImage image, MacMfsAllocationMap map, int allocationStart, uint allocationSize, int firstCluster, uint logicalLength, string file, string fork, ICollection<string> warnings)
    {
        if (logicalLength == 0) return new([], firstCluster == 0, [], []);
        if (firstCluster == 0)
        {
            warnings.Add(MacFileSystemExceptions.InconsistentForkMetadata(file, fork, logicalLength, firstCluster));
            return new([], false, [], []);
        }
        if (allocationSize == 0 || allocationSize % MacMfsFileSystemLayout.SectorSize != 0) throw MacFileSystemExceptions.InvalidAllocationSize(MacMfsFileSystemLayout.SystemName, allocationSize, MacMfsFileSystemLayout.SectorSize);
        var blocksPerAllocation = checked((int)(allocationSize / MacMfsFileSystemLayout.SectorSize));
        var requiredClusters = checked((int)((logicalLength + allocationSize - 1) / allocationSize));
        var chain = map.Traverse(firstCluster, requiredClusters);
        if (!chain.IsValid) warnings.Add(MacFileSystemExceptions.InvalidAllocationChain(file, fork, chain.HasCycle, chain.IsOutOfRange, chain.IsPrematureEnd));
        using var output = new MemoryStream();
        var missingBlocks = new List<int>();
        foreach (var cluster in chain.Clusters)
        {
            var firstBlock = allocationStart + (cluster - MacMfsFileSystemLayout.FirstUsableCluster) * blocksPerAllocation;
            for (var index = 0; index < blocksPerAllocation && output.Length < logicalLength; index++)
            {
                var logicalBlock = firstBlock + index;
                if (!image.TryGetBlock(logicalBlock, out var block) || block.Data.Count != MacMfsFileSystemLayout.SectorSize)
                {
                    missingBlocks.Add(logicalBlock);
                    warnings.Add(MacFileSystemExceptions.MissingBlock(file, fork, logicalBlock));
                    output.Write(new byte[MacMfsFileSystemLayout.SectorSize]);
                }
                else output.Write(block.Data.ToArray());
            }
        }
        if (output.Length < logicalLength) warnings.Add(MacFileSystemExceptions.IncompleteData(file, fork, output.Length, logicalLength));
        var content = output.ToArray().Take(checked((int)Math.Min(logicalLength, int.MaxValue))).ToArray();
        return new(Array.AsReadOnly(content), chain.IsValid && missingBlocks.Count == 0 && content.Length == logicalLength, missingBlocks.AsReadOnly(), chain.Clusters);
    }
}
