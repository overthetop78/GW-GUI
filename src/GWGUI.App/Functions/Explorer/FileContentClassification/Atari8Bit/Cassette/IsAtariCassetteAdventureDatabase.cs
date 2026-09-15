using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariCassetteAdventureDatabase(FileSystemEntry entry) =>
        entry.Content is { Count: 10031 } data
        && entry.Metadata.TryGetValue("sourceKind", out var sourceKind)
        && sourceKind.Equals("atari-cas-records", StringComparison.Ordinal)
        && entry.Metadata.TryGetValue("fullRecordCount", out var fullCount) && fullCount == "78"
        && entry.Metadata.TryGetValue("partialRecordCount", out var partialCount) && partialCount == "1"
        && entry.Metadata.TryGetValue("endRecordPresent", out var endPresent)
        && bool.TryParse(endPresent, out var hasEnd) && hasEnd
        && ContainsAscii(data, "in a dark hall", data.Count)
        && ContainsAscii(data, "SILVER CRUCIFIX", data.Count)
        && ContainsAscii(data, "North,South,East", data.Count);
}
