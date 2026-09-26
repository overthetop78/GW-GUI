using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Common.Machines.AmigaCDTV.Constants;

internal static class AmigaCdtvKeyboardConstants
{
    internal static readonly IReadOnlyList<EmulationKey> SpecialKeys =
        [EmulationKey.Help, EmulationKey.LeftAmiga, EmulationKey.RightAmiga];
}
