using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtari8BitArtistUnleashedImage(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 32 }) return false;
        var offset = 0;
        while (offset < 16 && data[offset] == (byte)' ') offset++;
        var address = System.Text.Encoding.ASCII.GetBytes("41296");
        if (offset == 0 || offset + address.Length + 3 >= data.Count) return false;
        for (var index = 0; index < address.Length; index++)
            if (data[offset + index] != address[index]) return false;
        offset += address.Length;
        return data[offset] == 0x9b && data[offset + 1] == 0 && data[offset + 2] == 0;
    }
}
