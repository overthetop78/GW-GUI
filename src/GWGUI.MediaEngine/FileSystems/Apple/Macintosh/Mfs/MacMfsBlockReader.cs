using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Mfs;

/// <summary>Lit des blocs MFS en réservant leur position lorsqu'ils sont absents ou invalides.</summary>
internal static class MacMfsBlockReader
{
    /// <summary>Lit une plage de blocs et retourne leur présence ainsi que la validité globale.</summary>
    public static MacMfsBlockReadResult Read(SectorImage image, int firstBlock, int count, string description, ICollection<string> warnings)
    {
        var bytes = new byte[count * MacMfsFileSystemLayout.SectorSize];
        var present = new bool[count];
        for (var index = 0; index < count; index++)
        {
            var logicalBlock = firstBlock + index;
            if (!image.TryGetBlock(logicalBlock, out var block) || block.Data.Count != MacMfsFileSystemLayout.SectorSize)
            {
                warnings.Add(MacFileSystemExceptions.MissingBlock(description, MacMfsFileSystemLayout.DataForkName, logicalBlock));
                continue;
            }
            block.Data.ToArray().CopyTo(bytes, index * MacMfsFileSystemLayout.SectorSize);
            present[index] = true;
        }
        return new(Array.AsReadOnly(bytes), Array.AsReadOnly(present), present.All(value => value));
    }
}
