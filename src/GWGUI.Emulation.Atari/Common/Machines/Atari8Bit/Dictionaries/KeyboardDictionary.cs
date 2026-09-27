using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Machines.Atari8Bit.Dictionaries;

internal static class Atari8BitKeyboardDictionary
{
    internal static readonly IReadOnlyDictionary<EmulationKey, EmulationKey> DefaultHostKeys =
        new Dictionary<EmulationKey, EmulationKey>
        {
            [EmulationKey.AtariOption] = EmulationKey.F2,
            [EmulationKey.AtariSelect] = EmulationKey.F3,
            [EmulationKey.AtariStart] = EmulationKey.F1,
            [EmulationKey.Help] = EmulationKey.Insert,
            [EmulationKey.Break] = EmulationKey.End
        };
}
