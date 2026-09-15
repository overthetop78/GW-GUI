using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool ContainsAtasciiText(IReadOnlyList<byte> data, string text)
    {
        var expected = System.Text.Encoding.ASCII.GetBytes(text);
        for (var offset = 0; offset <= data.Count - expected.Length; offset++)
        {
            var matches = true;
            for (var index = 0; index < expected.Length; index++)
            {
                if ((data[offset + index] & 0x7f) == expected[index]) continue;
                matches = false;
                break;
            }
            if (matches) return true;
        }
        return false;
    }
}
