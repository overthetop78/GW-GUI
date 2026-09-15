using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariCassettePartialProgramStream(FileSystemEntry entry) =>
        entry.Content is { Count: 10667 } data
        && entry.Metadata.TryGetValue("sourceKind", out var sourceKind)
        && sourceKind.Equals("atari-cas-records", StringComparison.Ordinal)
        && entry.Metadata.TryGetValue("fullRecordCount", out var fullCount) && fullCount == "83"
        && entry.Metadata.TryGetValue("partialRecordCount", out var partialCount) && partialCount == "1"
        && entry.Metadata.TryGetValue("endRecordPresent", out var endPresent)
        && bool.TryParse(endPresent, out var hasEnd) && hasEnd
        && data.Take(16).SequenceEqual(new byte[]
        {
            0x01, 0x00, 0x02, 0x41, 0x02, 0xc6, 0x01, 0xc5,
            0x02, 0xc4, 0x01, 0xc5, 0x01, 0xc6, 0x0a, 0x00
        })
        && ContainsSequence(data, new byte[] { 0xad, 0x58, 0x1e, 0xd0, 0x14, 0xa0, 0x00, 0xae, 0xef, 0x1e });
}
