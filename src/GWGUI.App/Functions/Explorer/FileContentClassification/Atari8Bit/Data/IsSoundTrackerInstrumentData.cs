using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsSoundTrackerInstrumentData(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".dta", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 4646 } data
        && data.Take(6).SequenceEqual(new byte[] { (byte)'R', (byte)'A', (byte)'W', (byte)'D', (byte)'T', (byte)'A' });
}
