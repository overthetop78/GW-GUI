using Options = GWGUI.Emulation.Sinclair.Emulators.EightyOne.Dictionaries.OptionCatalog;
using CommonSettings = GWGUI.Emulation.Sinclair.Common.Machines.Common.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Sinclair.Emulators.EightyOne.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<CoreOption> GetOptions(MachineConfiguration? configuration) =>
        CommonSettings.NativeOptions(configuration, Options.Options, RuntimeOptionFunctions.Defaults, RuntimeOptionFunctions.IsAvailable);

    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration) =>
        CommonSettings.NativeBlocks(configuration, GetOptions(configuration));
}
