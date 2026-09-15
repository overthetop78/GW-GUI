using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariA000SelfExtractingExecutable(IReadOnlyList<byte>? data) =>
        data is { Count: 1190 }
        && ReadUInt16(data, 0) == 0xffff
        && ReadUInt16(data, 2) == 0xa000
        && ReadUInt16(data, 4) == 0xbfff
        && data.Skip(6).Take(24).SequenceEqual(new byte[]
        {
            0xd8, 0xa9, 0xaa, 0x85, 0xc1, 0xa9, 0x00, 0x85,
            0xc0, 0xa9, 0x2a, 0x85, 0xc3, 0xa9, 0x00, 0x85,
            0xc2, 0xa0, 0x00, 0xb1, 0xc0, 0x91, 0xc2, 0xc8
        });
}
