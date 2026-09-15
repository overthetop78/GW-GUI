using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariCassetteGraphicRecordStream(FileSystemEntry entry)
    {
        if (!IsCompleteAtariCassetteRecordStream(entry) || entry.Content is not { } data) return false;
        return data is { Count: 8448 }
                && data.Take(16).SequenceEqual(new byte[]
                {
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x04, 0x04, 0x04, 0xa6, 0xa6, 0x04, 0x04, 0x04
                })
                && data.Distinct().Take(128).Count() == 128
            || data is { Count: 4096 }
                && data.Take(16).SequenceEqual(new byte[]
                {
                    0x02, 0x00, 0x4e, 0x40, 0x70, 0x56, 0x56, 0x80,
                    0x0c, 0x00, 0x0c, 0x01, 0x06, 0x00, 0xce, 0x8c
                })
                && data.Count(value => value == 0) >= 2048;
    }
}
