using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Common.Machines.AmigaComputers.Dictionaries;

internal static class AmigaComputerKeyboardDictionary
{
    internal static readonly IReadOnlyDictionary<EmulationKey, EmulationKey> DefaultHostKeys =
        new Dictionary<EmulationKey, EmulationKey>
        {
            [EmulationKey.Help] = EmulationKey.Insert,
            [EmulationKey.LeftAmiga] = EmulationKey.PageUp,
            [EmulationKey.RightAmiga] = EmulationKey.PageDown
        };
}
