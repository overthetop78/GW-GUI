using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Ucsd;

/// <summary>Reconstruit les fichiers UCSD dont le dernier bloc est une borne exclusive.</summary>
internal static class UcsdFileContentReader
{
    /// <summary>Lit la plage [premier bloc, dernier bloc) et applique la longueur utile du dernier bloc.</summary>
    public static UcsdFileContent Read(SectorImage image, int firstBlock, int lastBlockExclusive, int lastBytes, string name, ICollection<string> warnings)
    {
        var blockCount = lastBlockExclusive - firstBlock;
        if (blockCount <= 0) return new([], blockCount == 0 && lastBytes == 0, [], 0);
        if (lastBytes < 0 || lastBytes > UcsdFileSystemLayout.BlockSize)
        {
            warnings.Add(UcsdFileSystemExceptions.InvalidLastBlockByteCount(name, lastBytes));
            return new([], false, [], 0);
        }
        var effectiveLastBytes = lastBytes == 0 ? UcsdFileSystemLayout.BlockSize : lastBytes;
        var size = checked((long)(blockCount - 1) * UcsdFileSystemLayout.BlockSize + effectiveLastBytes);
        var read = UcsdBlockReader.Read(image, firstBlock, blockCount);
        var content = read.Bytes.Take(checked((int)Math.Min(size, read.Bytes.Count))).ToArray();
        if (!read.IsValid) warnings.Add(UcsdFileSystemExceptions.MissingBlocks(name, read.MissingBlocks));
        return new(Array.AsReadOnly(content), read.IsValid, read.MissingBlocks, size);
    }
}
