using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Ucsd;

/// <summary>Lit positionnellement les blocs UCSD sans décaler les blocs suivants.</summary>
internal static class UcsdBlockReader
{
    /// <summary>Lit une plage et réserve exactement un secteur par bloc invalide.</summary>
    public static UcsdBlockReadResult Read(SectorImage image, int firstBlock, int blockCount)
    {
        var bytes = new byte[checked(blockCount * UcsdFileSystemLayout.BlockSize)];
        var present = new bool[blockCount];
        var missing = new List<int>();
        for (var index = 0; index < blockCount; index++)
        {
            var logicalBlock = firstBlock + index;
            if (!image.TryGetBlock(logicalBlock, out var block) || block.Data.Count != UcsdFileSystemLayout.BlockSize)
            {
                missing.Add(logicalBlock);
                continue;
            }
            var destinationOffset = index * UcsdFileSystemLayout.BlockSize;
            for (var byteIndex = 0; byteIndex < UcsdFileSystemLayout.BlockSize; byteIndex++) bytes[destinationOffset + byteIndex] = block.Data[byteIndex];
            present[index] = true;
        }
        return new(Array.AsReadOnly(bytes), Array.AsReadOnly(present), missing.AsReadOnly());
    }
}
