using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariCassetteAssemblerSourceStream(FileSystemEntry entry) =>
        IsCompleteAtariCassetteRecordStream(entry)
        && entry.Content is { Count: 16384 } data
        && data.Take(16).SequenceEqual(new byte[]
        {
            0x75, 0x00, 0x00, 0xd3, 0x5f, 0x01, 0x11, 0x07,
            0x08, 0x01, 0x00, 0x01, 0x80, 0x01, 0x00, 0x13
        })
        && ContainsAscii(data, "LDA", data.Count)
        && ContainsAscii(data, "STA", data.Count)
        && ContainsAscii(data, "WSYNC", data.Count)
        && ContainsAscii(data, "CHBASE", data.Count);
}
