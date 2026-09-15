using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariMiner2049PackedExecutable(IReadOnlyList<byte>? data) =>
        data is { Count: 17164 }
        && data.Take(16).SequenceEqual(new byte[]
        {
            0xff, 0xff, 0xfe, 0x60, 0x5d, 0x60, 0xa9, 0x23,
            0x8d, 0x30, 0x02, 0x8d, 0x02, 0xd4, 0xa9, 0x60
        })
        && data.Skip(data.Count - 6).SequenceEqual(new byte[] { 0xe2, 0x02, 0xe3, 0x02, 0x80, 0x7b });
}
