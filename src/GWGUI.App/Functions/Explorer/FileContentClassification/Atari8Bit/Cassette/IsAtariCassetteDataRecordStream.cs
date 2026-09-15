using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariCassetteDataRecordStream(FileSystemEntry entry)
    {
        if (!IsCompleteAtariCassetteRecordStream(entry) || entry.Content is not { } data) return false;
        return data is { Count: 6528 }
                && data.Take(16).SequenceEqual(new byte[]
                {
                    0x70, 0x70, 0x70, 0xc2, 0x60, 0x43, 0x0d, 0x0d,
                    0x0d, 0x0d, 0x0d, 0x0d, 0x0d, 0x0d, 0x0d, 0x0d
                })
                && data.Distinct().Take(128).Count() == 128
            || data is { Count: 21760 }
                && data.Take(34).All(value => value == 0)
                && data.Skip(34).Take(2).SequenceEqual(new byte[] { 0x0f, 0xc0 })
                && data.Distinct().Take(128).Count() == 128;
    }
}
