using GWGUI.Emulation.Philips.Emulators.M2000.Constants;

namespace GWGUI.Emulation.Philips.Emulators.M2000.Dictionaries;

internal static class M2000OptionCatalog
{
    internal static IReadOnlyList<CoreOption> All { get; } =
    [
        new(OptionConstants.KeyboardMapping, "Keyboard mapping", null, null,
            OptionConstants.Symbolic, OptionConstants.Symbolic,
            [new(OptionConstants.Symbolic, "symbolic"), new(OptionConstants.Positional, "positional")])
    ];
}
