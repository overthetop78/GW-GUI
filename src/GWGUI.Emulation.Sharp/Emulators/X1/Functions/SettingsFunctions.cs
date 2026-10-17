using GWGUI.Emulation.Sharp.Emulators.X1.Dictionaries;
using CommonSettings = GWGUI.Emulation.Sharp.Common.Machines.Common.Functions.SettingsDescriptionFunctions;
using Options = GWGUI.Emulation.Sharp.Emulators.X1.Dictionaries.OptionCatalog;

namespace GWGUI.Emulation.Sharp.Emulators.X1.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<CoreOption> GetOptions(MachineConfiguration? configuration) =>
        CommonSettings.NativeOptions(configuration, Options.Options, RuntimeOptionFunctions.Defaults);

    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration) =>
        CommonSettings.NativeBlocks(configuration, GetOptions(configuration));
}
