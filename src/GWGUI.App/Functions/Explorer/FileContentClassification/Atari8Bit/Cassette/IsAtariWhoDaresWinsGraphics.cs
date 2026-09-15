using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariWhoDaresWinsGraphics(FileSystemEntry entry)
    {
        if (!IsCompleteAtariCassetteRecordStream(entry) || entry.Content is not { } data)
            return false;

        return data is { Count: 24576 }
                && data.Take(16).SequenceEqual(new byte[]
                {
                    0x00, 0x05, 0xe6, 0xa2, 0x2a, 0xa0, 0x00, 0x05,
                    0xad, 0xa8, 0x2a, 0xa0, 0x01, 0x05, 0xad, 0xa6
                })
            || data is { Count: 8192 }
                && data.Take(16).SequenceEqual(new byte[]
                {
                    0x88, 0x85, 0x84, 0x89, 0x80, 0x83, 0x89, 0x87,
                    0x86, 0x81, 0x85, 0x86, 0x81, 0x84, 0x80, 0x84
                });
    }
}
