using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariWriterDocument(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 64 }
            || ReadUInt16(data, 0) != 0x0017
            || data[25] != 0x84)
            return false;
        var body = data.Skip(32).ToArray();
        return body.Contains((byte)0x9b)
            && body.Count(value => value == 0x9b || (value & 0x7f) is >= 32 and < 127) >= body.Length * 0.9;
    }
}
