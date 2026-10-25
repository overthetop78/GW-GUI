using GWGUI.Emulation;

namespace GWGUI.Emulation.Commodore.Common.Machines.CommodoreCDTV.Dictionaries;

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
