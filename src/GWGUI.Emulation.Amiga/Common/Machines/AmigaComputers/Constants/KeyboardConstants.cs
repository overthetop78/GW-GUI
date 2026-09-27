using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Common.Machines.AmigaComputers.Constants;

internal static class AmigaComputerKeyboardConstants
{
    internal static readonly IReadOnlyList<EmulationKey> SpecialKeys =
        [EmulationKey.Help, EmulationKey.LeftAmiga, EmulationKey.RightAmiga];
}
