using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsSoundTrackerMusic(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".muz", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: >= 128 } data
        && data.Take(6).SequenceEqual(new byte[] { (byte)'M', (byte)'u', (byte)'s', (byte)'i', (byte)'c', (byte)' ' });
}
