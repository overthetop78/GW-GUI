using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsVisualiserHelpDocument(FileSystemEntry entry)
    {
        var extension = System.IO.Path.GetExtension(entry.Name);
        if (!entry.Name.StartsWith("HELP.", StringComparison.OrdinalIgnoreCase)
            || extension.Length != 4
            || char.ToUpperInvariant(extension[1]) is not ('C' or 'S')
            || !char.IsDigit(extension[2]) || !char.IsDigit(extension[3])
            || entry.Content is not { Count: 961 } data)
            return false;

        var hasMixedPrompt = Enumerable.Range(0, data.Count - 12)
            .Any(offset => data.Skip(offset).Take(13).SequenceEqual(new byte[]
            {
                0x30, (byte)'r', (byte)'e', (byte)'s', (byte)'s', 0x00,
                (byte)'a', (byte)'n', (byte)'y', 0x00, (byte)'k', (byte)'e', (byte)'y'
            }));
        var hasMenu = ContainsAscii(data, "exit", data.Count)
            && ContainsAscii(data, "view", data.Count)
            && ContainsAscii(data, "next", data.Count)
            && ContainsAscii(data, "page", data.Count);
        var invertedPrompt = new byte[]
        {
            0xb0, 0xf2, 0xe5, 0xf3, 0xf3, 0x80,
            0xe1, 0xee, 0xf9, 0x80, 0xeb, 0xe5, 0xf9
        };
        var hasInvertedPrompt = Enumerable.Range(0, data.Count - invertedPrompt.Length + 1)
            .Any(offset => data.Skip(offset).Take(invertedPrompt.Length).SequenceEqual(invertedPrompt));
        return hasMixedPrompt || hasMenu || hasInvertedPrompt;
    }
}
