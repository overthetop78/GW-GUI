using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariAstroChaseCassetteProgram(FileSystemEntry entry) =>
        IsCompleteAtariCassetteRecordStream(entry)
        && entry.Content is { Count: 16640 } data
        && data.Take(16).SequenceEqual(new byte[]
        {
            0x10, 0x01, 0x00, 0x10, 0x01, 0x01, 0x00, 0x11,
            0x01, 0x10, 0x00, 0x00, 0x00, 0x01, 0x10, 0x00
        })
        && ContainsAscii(data, "ASTRO CHASE", data.Count)
        && ContainsAscii(data, "FERNANDO HERRERA", data.Count)
        && ContainsAscii(data, "FIRST STAR SOFTWARE", data.Count);
}
