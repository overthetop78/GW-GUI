using GWGUI.Emulation;

namespace GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants;

internal static class AmigaComputerKeyboardConstants
{
    internal static readonly IReadOnlyList<EmulationKey> SpecialKeys =
        [EmulationKey.Help, EmulationKey.LeftAmiga, EmulationKey.RightAmiga];
}
