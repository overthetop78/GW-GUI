using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Dec.Rt11;

/// <summary>Lit positionnellement les blocs d'un fichier RT-11.</summary>
public static class Rt11FileContentReader
{
    /// <summary>Lit le nombre de blocs demandé et réserve les secteurs absents.</summary>
    public static Rt11FileContent Read(SectorImage image, int startBlock, int blockCount)
    {
        if (blockCount < 0 || blockCount > image.BlockCount) return new([], false, []);
        var content = new byte[checked(blockCount * Rt11FileSystemLayout.BlockSize)];
        var missing = new List<int>();
        for (var index = 0; index < blockCount; index++)
        {
            var logicalBlock = startBlock + index;
            if (!image.TryGetBlock(logicalBlock, out var block) || block.Data.Count != Rt11FileSystemLayout.BlockSize) missing.Add(logicalBlock);
            else block.Data.ToArray().AsSpan().CopyTo(content.AsSpan(index * Rt11FileSystemLayout.BlockSize));
        }
        return new(Array.AsReadOnly(content), missing.Count == 0, missing.AsReadOnly());
    }
}
