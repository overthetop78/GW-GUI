using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariCassetteCompilationDataStream(FileSystemEntry entry) =>
        entry.Content is { Count: 10362 } data
        && entry.Metadata.TryGetValue("sourceKind", out var sourceKind)
        && sourceKind.Equals("atari-cas-records", StringComparison.Ordinal)
        && entry.Metadata.TryGetValue("fullRecordCount", out var fullCount) && fullCount == "80"
        && entry.Metadata.TryGetValue("partialRecordCount", out var partialCount) && partialCount == "1"
        && entry.Metadata.TryGetValue("endRecordPresent", out var endPresent)
        && bool.TryParse(endPresent, out var hasEnd) && hasEnd
        && data.Take(16).SequenceEqual(new byte[]
        {
            0x70, 0xf0, 0x70, 0x4e, 0x50, 0x11, 0x0e, 0x0e,
            0x0e, 0x0e, 0x0e, 0x0e, 0x0e, 0x0e, 0x0e, 0x0e
        })
        && data.Distinct().Take(128).Count() == 128;
}
