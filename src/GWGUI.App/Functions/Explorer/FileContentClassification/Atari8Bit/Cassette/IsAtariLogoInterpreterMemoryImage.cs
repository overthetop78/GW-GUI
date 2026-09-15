using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariLogoInterpreterMemoryImage(FileSystemEntry entry)
    {
        if (entry.Content is not { Count: 16384 } data
            || !entry.Metadata.TryGetValue("sourceKind", out var sourceKind)
            || !sourceKind.Equals("atari-cas-records", StringComparison.Ordinal)
            || !entry.Metadata.TryGetValue("recordCount", out var recordCount) || recordCount != "128"
            || !entry.Metadata.TryGetValue("fullRecordCount", out var fullRecordCount) || fullRecordCount != "128"
            || !entry.Metadata.TryGetValue("partialRecordCount", out var partialRecordCount) || partialRecordCount != "0"
            || !entry.Metadata.TryGetValue("endRecordPresent", out var endRecordPresent)
            || !bool.TryParse(endRecordPresent, out var hasEndRecord) || hasEndRecord)
            return false;
        return ContainsAtasciiText(data, "BRAK MIEJSCA")
            && ContainsAtasciiText(data, "NIE MOGE OTWORZYC")
            && ContainsAtasciiText(data, "PRZERWANE!");
    }
}
