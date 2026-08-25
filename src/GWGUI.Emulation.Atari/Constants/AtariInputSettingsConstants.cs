using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Constants;

internal static class AtariInputSettingsConstants
{
    internal static readonly IReadOnlyList<EmulationKey> FunctionKeys =
        [EmulationKey.F1, EmulationKey.F2, EmulationKey.F3, EmulationKey.F4, EmulationKey.F5,
         EmulationKey.F6, EmulationKey.F7, EmulationKey.F8, EmulationKey.F9, EmulationKey.F10];

    internal static readonly IReadOnlyList<EmulationKey> ComputerSpecialKeys =
        [EmulationKey.Help, EmulationKey.AtariUndo, EmulationKey.AtariBreak];

    internal static readonly IReadOnlyList<EmulationKey> Atari800SpecialKeys =
        [EmulationKey.AtariOption, EmulationKey.AtariSelect, EmulationKey.AtariStart,
         EmulationKey.Help, EmulationKey.AtariBreak];

    internal static readonly IReadOnlyDictionary<EmulationKey, EmulationKey> DefaultKeys =
        new Dictionary<EmulationKey, EmulationKey>
        {
            [EmulationKey.Help] = EmulationKey.Insert,
            [EmulationKey.AtariUndo] = EmulationKey.Home,
            [EmulationKey.AtariBreak] = EmulationKey.End,
            [EmulationKey.AtariOption] = EmulationKey.F2,
            [EmulationKey.AtariSelect] = EmulationKey.F3,
            [EmulationKey.AtariStart] = EmulationKey.F1
        };
}
