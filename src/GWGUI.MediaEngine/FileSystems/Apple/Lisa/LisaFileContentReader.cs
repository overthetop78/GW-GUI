using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Apple.Lisa;

/// <summary>Reconstruit les fichiers Lisa depuis leurs pages taguées.</summary>
internal static class LisaFileContentReader
{
    /// <summary>Regroupe les pages utilisateur par identifiant.</summary>
    public static IEnumerable<(ushort FileId, LisaFileContent File)> ReadAll(SectorImage image, List<string> warnings)
    {
        var pages = image.AvailableBlocks.Select(block => (Block: block, HasTag: LisaPageTagReader.TryRead(block, out var tag), Tag: tag)).Where(item => item.HasTag && LisaPageTagReader.IsUserFile(item.Tag.FileId));
        foreach (var group in pages.GroupBy(item => item.Tag.FileId).OrderBy(group => group.Key)) yield return (group.Key, Read(group.Key, group.Select(item => (item.Block, item.Tag.PageNumber)), warnings));
    }

    /// <summary>Ordonne les pages, conserve les lacunes et signale les doublons.</summary>
    private static LisaFileContent Read(ushort fileId, IEnumerable<(SectorBlock Block, int PageNumber)> source, List<string> warnings)
    {
        var ordered = source.OrderBy(item => item.PageNumber).ThenBy(item => item.Block.LogicalBlock).ToArray();
        using var content = new MemoryStream();
        var valid = true;
        var expectedPage = ordered[0].PageNumber;
        var pageSize = ordered[0].Block.Data.Count;
        var seen = new HashSet<int>();
        foreach (var item in ordered)
        {
            if (!seen.Add(item.PageNumber))
            {
                warnings.Add(LisaFileSystemExceptions.DuplicatePage(fileId, item.PageNumber));
                valid = false;
                continue;
            }
            while (expectedPage < item.PageNumber)
            {
                warnings.Add(LisaFileSystemExceptions.MissingPage(fileId, expectedPage));
                content.Write(new byte[pageSize]);
                expectedPage++;
                valid = false;
            }
            content.Write(item.Block.Data.ToArray());
            expectedPage = item.PageNumber + 1;
        }
        return new(Array.AsReadOnly(content.ToArray()), valid, ordered[0].Block.LogicalBlock);
    }
}
