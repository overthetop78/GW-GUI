using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariCharacterGeneratorCassetteProgram(FileSystemEntry entry) =>
        entry.Content is { Count: 2176 } data
        && entry.Metadata.TryGetValue("sourceKind", out var sourceKind)
        && sourceKind.Equals("atari-cas-records", StringComparison.Ordinal)
        && entry.Metadata.TryGetValue("fullRecordCount", out var fullCount) && fullCount == "17"
        && entry.Metadata.TryGetValue("partialRecordCount", out var partialCount) && partialCount == "0"
        && entry.Metadata.TryGetValue("endRecordPresent", out var endPresent)
        && bool.TryParse(endPresent, out var hasEnd) && !hasEnd
        && ContainsAscii(data, "CHARACTER generator", data.Count)
        && ContainsAscii(data, "P.B. SOFTWARE", data.Count)
        && ContainsAscii(data, "EDIT CHARACTER", data.Count);
}
