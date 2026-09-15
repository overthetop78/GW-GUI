using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsCompleteAtariCassetteRecordStream(FileSystemEntry entry) =>
        entry.Content is { Count: >= 256 } data
        && entry.Metadata.TryGetValue("sourceKind", out var sourceKind)
        && sourceKind.Equals("atari-cas-records", StringComparison.Ordinal)
        && entry.Metadata.TryGetValue("fullRecordCount", out var fullCountText)
        && int.TryParse(fullCountText, out var fullCount)
        && fullCount >= 2 && data.Count == fullCount * 128
        && entry.Metadata.TryGetValue("partialRecordCount", out var partialCount) && partialCount == "0"
        && entry.Metadata.TryGetValue("endRecordPresent", out var endPresent)
        && bool.TryParse(endPresent, out var hasEnd) && hasEnd;
}
