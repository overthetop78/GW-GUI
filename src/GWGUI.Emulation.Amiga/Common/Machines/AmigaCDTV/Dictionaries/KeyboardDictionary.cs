using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Common.Machines.AmigaCDTV.Dictionaries;

internal static class AmigaCdtvKeyboardDictionary
{
    internal static readonly IReadOnlyDictionary<EmulationKey, EmulationKey> DefaultHostKeys =
        new Dictionary<EmulationKey, EmulationKey>
        {
            [EmulationKey.Help] = EmulationKey.Insert,
            [EmulationKey.LeftAmiga] = EmulationKey.PageUp,
            [EmulationKey.RightAmiga] = EmulationKey.PageDown
        };
}
