using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtari8BitCassetteBootProgram(FileSystemEntry entry)
    {
        if (!entry.Metadata.TryGetValue("sourceKind", out var sourceKind)
            || !sourceKind.Equals("atari-cas-records", StringComparison.Ordinal)
            || entry.Content is not { Count: >= 128 } data
            || data[1] == 0)
            return false;

        var recordCount = data[1];
        var loadAddress = ReadUInt16(data, 2);
        var initAddress = ReadUInt16(data, 4);
        var loadedEnd = loadAddress + recordCount * 128;
        return loadAddress > 0
            && loadedEnd <= 0x10000
            && (initAddress == 0 || initAddress >= loadAddress && initAddress < loadedEnd)
            && data.Count > (recordCount - 1) * 128;
    }
}
