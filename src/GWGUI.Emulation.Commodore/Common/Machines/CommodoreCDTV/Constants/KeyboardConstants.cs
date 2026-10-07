using GWGUI.Emulation;

namespace GWGUI.Emulation.Commodore.Common.Machines.CommodoreCDTV.Constants;

internal static class AmigaCdtvKeyboardConstants
{
    internal static readonly IReadOnlyList<EmulationKey> SpecialKeys =
        [EmulationKey.Help, EmulationKey.LeftAmiga, EmulationKey.RightAmiga];
}
