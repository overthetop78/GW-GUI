using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariPhantomGameData(FileSystemEntry entry)
    {
        if (!IsCompleteAtariCassetteRecordStream(entry) || entry.Content is not { } data)
            return false;

        return data is { Count: 512 }
                && data.Take(16).SequenceEqual(new byte[]
                {
                    0x04, 0x09, 0x0a, 0x0b, 0x0c, 0x0e, 0x11, 0x13,
                    0x14, 0x15, 0x16, 0x18, 0x00, 0x04, 0x09, 0x0e
                })
            || data is { Count: 1536 }
                && (data.Take(16).SequenceEqual(new byte[]
                    {
                        0x40, 0xcf, 0x05, 0x51, 0x05, 0x4f, 0x05, 0x4d,
                        0x05, 0x4b, 0x45, 0x69, 0x49, 0x29, 0x8d, 0x69
                    })
                    || data.Take(16).SequenceEqual(new byte[]
                    {
                        0x40, 0xcf, 0x82, 0x2e, 0xc6, 0x2e, 0x8a, 0x2e,
                        0xce, 0x2e, 0x95, 0x2e, 0xd9, 0x2e, 0x5b, 0x4c
                    })
                    || data.Take(16).SequenceEqual(new byte[]
                    {
                        0x66, 0xd0, 0x62, 0x2f, 0x5e, 0x2f, 0x9a, 0x2f,
                        0xd3, 0x2f, 0x4f, 0x2f, 0x1a, 0x4d, 0x1a, 0x4b
                    }));
    }
}
