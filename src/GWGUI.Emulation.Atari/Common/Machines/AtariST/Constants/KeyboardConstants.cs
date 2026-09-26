using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Machines.AtariST.Constants;

internal static class AtariStKeyboardConstants
{
    internal static readonly IReadOnlyList<EmulationKey> SpecialKeys =
        [EmulationKey.Help, EmulationKey.Undo, EmulationKey.Break];
}
