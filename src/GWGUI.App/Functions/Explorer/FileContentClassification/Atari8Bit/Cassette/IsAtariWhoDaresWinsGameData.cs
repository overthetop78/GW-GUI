using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariWhoDaresWinsGameData(FileSystemEntry entry) =>
        IsCompleteAtariCassetteRecordStream(entry)
        && entry.Content is { Count: 13824 } data
        && data.Take(16).SequenceEqual(new byte[]
        {
            0x00, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04, 0x00,
            0x00, 0x00, 0x00, 0x05, 0x06, 0x07, 0x08, 0x09
        })
        && ContainsAscii(data, "SCORE", data.Count)
        && ContainsAscii(data, "LIVES", data.Count)
        && ContainsAscii(data, "OUTPOST", data.Count)
        && ContainsAscii(data, "CAPTURED", data.Count);
}
