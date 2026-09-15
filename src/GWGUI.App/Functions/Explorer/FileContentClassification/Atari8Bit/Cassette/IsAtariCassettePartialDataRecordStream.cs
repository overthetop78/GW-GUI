using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariCassettePartialDataRecordStream(FileSystemEntry entry) =>
        entry.Content is { Count: 6460 } data
        && entry.Metadata.TryGetValue("sourceKind", out var sourceKind)
        && sourceKind.Equals("atari-cas-records", StringComparison.Ordinal)
        && entry.Metadata.TryGetValue("fullRecordCount", out var fullCount) && fullCount == "50"
        && entry.Metadata.TryGetValue("partialRecordCount", out var partialCount) && partialCount == "1"
        && entry.Metadata.TryGetValue("endRecordPresent", out var endPresent)
        && bool.TryParse(endPresent, out var hasEnd) && hasEnd
        && data.Take(16).SequenceEqual(new byte[]
        {
            0x28, 0x03, 0x01, 0x02, 0x20, 0xb2, 0x00, 0x0c,
            0x10, 0x07, 0x50, 0x03, 0x00, 0x02, 0x20, 0xba
        })
        && data.Distinct().Take(128).Count() == 128;
}
