using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsTrickMachineRoutine(FileSystemEntry entry)
    {
        if (!string.Equals(System.IO.Path.GetExtension(entry.Name), ".dan", StringComparison.OrdinalIgnoreCase)
            || entry.Content is not { } data)
            return false;
        return data.Count == 20
                && data.Take(3).SequenceEqual(new byte[] { 0x48, 0x8a, 0x48 })
                && data.Skip(16).SequenceEqual(new byte[] { 0x68, 0xaa, 0x68, 0x40 })
            || data.Count == 88
                && data.Take(5).SequenceEqual(new byte[] { 0x68, 0xa0, 0x0b, 0xa2, 0x86 })
                && data.Skip(85).SequenceEqual(new byte[] { 0x4c, 0x62, 0xe4 });
    }
}
