using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariPhantomGraphics(FileSystemEntry entry)
    {
        if (!IsCompleteAtariCassetteRecordStream(entry) || entry.Content is not { } data)
            return false;

        return data is { Count: 6912 }
                && data.Take(16).SequenceEqual(new byte[]
                {
                    0xf3, 0xa8, 0x79, 0xa8, 0x00, 0xa9, 0xf3, 0xa6,
                    0x79, 0xa6, 0x00, 0xa3, 0xf3, 0xa4, 0x79, 0xa4
                })
            || data is { Count: 2304 }
                && data.Take(24).All(value => value == 0)
                && data.Count(value => value == 0) == 772
            || data is { Count: 5376 }
                && data.Take(16).SequenceEqual(new byte[]
                {
                    0x66, 0x9a, 0xa9, 0x66, 0x9a, 0x9a, 0x66, 0xa9,
                    0x66, 0x9a, 0xa9, 0x66, 0x9a, 0x9a, 0x66, 0xa9
                })
            || data is { Count: 5376 }
                && ContainsAscii(data, "LOADING PHANTOM", data.Count)
                && ContainsAscii(data, "SET TAPE COUNTER TO ZERO", data.Count);
    }
}
