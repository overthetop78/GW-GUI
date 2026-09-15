using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsScreenDumpPrinterProfile(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".par", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 64 } data
        && data.Skip(2).Take(2).SequenceEqual(new byte[] { 0x1b, (byte)'@' })
        && data.Skip(11).Take(2).SequenceEqual(new byte[] { 0x1b, (byte)'A' })
        && data.Skip(20).Take(2).SequenceEqual(new byte[] { 0x1b, (byte)'K' })
        && data.Skip(29).Take(2).SequenceEqual(new byte[] { 0x1b, (byte)'L' })
        && data.Skip(47).Take(9).All(value => value is >= 32 and < 127);
}
