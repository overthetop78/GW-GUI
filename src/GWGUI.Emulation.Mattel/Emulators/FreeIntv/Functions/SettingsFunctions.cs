using CommonSettings = GWGUI.Emulation.Mattel.Common.Machines.Common.Functions.SettingsDescriptionFunctions;
using GWGUI.Emulation.Mattel.Emulators.FreeIntv.Dictionaries;

namespace GWGUI.Emulation.Mattel.Emulators.FreeIntv.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<CoreOption> GetOptions() => FreeIntvOptionCatalog.All;

    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration) =>
        [new(SettingsConstants.ResourceNativeControls, EmulationMachineTab.Controllers, SettingsConstants.ResourceNativeControls,
            GetOptions().Where(option => option.IsVisible).Select(option => CommonSettings.Native(configuration, option, EmulationMachineTab.Controllers, SettingsConstants.ResourceNativeControls)).ToArray())];
}
