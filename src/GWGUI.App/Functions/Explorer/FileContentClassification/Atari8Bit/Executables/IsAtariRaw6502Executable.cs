using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariRaw6502Executable(FileSystemEntry entry)
    {
        if (!string.Equals(System.IO.Path.GetExtension(entry.Name), ".bit", StringComparison.OrdinalIgnoreCase)
            || entry.Content is not { Count: >= 256 } data
            || data[0] == 0xff && data[1] == 0xff)
        {
            return false;
        }

        var controlFlowInstructions = 0;
        var hardwareRegisterReferences = 0;
        var limit = Math.Min(data.Count, 256);
        for (var offset = 0; offset < limit; offset++)
        {
            if (data[offset] is 0x20 or 0x4c or 0x60) controlFlowInstructions++;
            if (offset + 2 < limit
                && IsAbsolute6502Opcode(data[offset])
                && data[offset + 2] is >= 0xd0 and <= 0xd7)
            {
                hardwareRegisterReferences++;
            }
        }

        return controlFlowInstructions >= 8 && hardwareRegisterReferences >= 4;
    }

    private static bool IsAbsolute6502Opcode(byte opcode) => opcode is
        0x0d or 0x1d or 0x2c or 0x2d or 0x3d or 0x4d or 0x5d or 0x6d or 0x7d
        or 0x8c or 0x8d or 0x8e or 0x99 or 0x9d
        or 0xac or 0xad or 0xae or 0xb9 or 0xbd or 0xbe
        or 0xcc or 0xcd or 0xce or 0xec or 0xed or 0xee;
}
