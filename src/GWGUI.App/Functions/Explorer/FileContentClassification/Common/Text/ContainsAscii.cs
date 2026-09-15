using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool ContainsAscii(IReadOnlyList<byte> data, string text, int searchLength)
    {
        var expected = System.Text.Encoding.ASCII.GetBytes(text);
        var limit = Math.Min(data.Count, searchLength) - expected.Length;
        for (var offset = 0; offset <= limit; offset++)
            if (data.Skip(offset).Take(expected.Length).SequenceEqual(expected)) return true;
        return false;
    }
}
