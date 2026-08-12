using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;

/// <summary>Lit les trois descripteurs d'extents intégrés d'un fork HFS.</summary>
internal static class MacHfsExtentReader
{
    /// <summary>Reconstruit les secteurs décrits par les extents en conservant la position de chaque secteur absent.</summary>
    public static MacHfsExtentResult Read(SectorImage image, ReadOnlySpan<byte> extents, int allocationStart, uint allocationSize, uint logicalLength)
    {
        if (allocationSize == 0 || allocationSize % MacHfsFileSystemLayout.SectorSize != 0) throw MacFileSystemExceptions.InvalidAllocationSize(MacHfsFileSystemLayout.SystemName, allocationSize, MacHfsFileSystemLayout.SectorSize);
        using var output = new MemoryStream();
        var missingBlocks = new List<int>();
        var blocksPerAllocation = checked((int)(allocationSize / MacHfsFileSystemLayout.SectorSize));
        for (var extent = 0; extent < MacHfsFileSystemLayout.EmbeddedExtentCount && output.Length < logicalLength; extent++)
        {
            var descriptorOffset = extent * MacHfsFileSystemLayout.ExtentDescriptorLength;
            var start = MacFileSystemPrimitives.ReadUInt16(extents, descriptorOffset + MacHfsFileSystemLayout.ExtentStartOffset);
            var count = MacFileSystemPrimitives.ReadUInt16(extents, descriptorOffset + MacHfsFileSystemLayout.ExtentCountOffset);
            for (var allocation = 0; allocation < count && output.Length < logicalLength; allocation++)
            {
                for (var blockIndex = 0; blockIndex < blocksPerAllocation && output.Length < logicalLength; blockIndex++)
                {
                    var logicalBlock = allocationStart + (start + allocation) * blocksPerAllocation + blockIndex;
                    if (!image.TryGetBlock(logicalBlock, out var block) || block.Data.Count != MacHfsFileSystemLayout.SectorSize)
                    {
                        missingBlocks.Add(logicalBlock);
                        output.Write(new byte[MacHfsFileSystemLayout.SectorSize]);
                    }
                    else output.Write(block.Data.ToArray());
                }
            }
        }
        var remainingLength = Math.Max(0, (long)logicalLength - output.Length);
        var content = output.ToArray().Take(checked((int)Math.Min(logicalLength, int.MaxValue))).ToArray();
        return new(content, missingBlocks.Count == 0 && remainingLength == 0, missingBlocks, remainingLength);
    }
}
