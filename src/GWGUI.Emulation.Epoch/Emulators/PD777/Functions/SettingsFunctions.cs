using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Epoch.Emulators.PD777.Dictionaries;
using CommonSettings = GWGUI.Emulation.Epoch.Common.Machines.Common.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Epoch.Emulators.PD777.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationMediaDevice> Storage(MachineConfiguration configuration) =>
        [new(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, MediaRuntimeConstants.PdCartridgeExtensions,
            RequiresMachineRecreation: true, ImageDirectory: configuration.Folders.Cartridges, IsPermanent: true)];

    internal static IReadOnlyList<CoreOption> GetOptions() => PD777OptionCatalog.All;

    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration) =>
        [new(SettingsConstants.ResourceNativeControls, EmulationMachineTab.Controllers, SettingsConstants.ResourceNativeControls,
            GetOptions().Select(option => CommonSettings.Native(configuration, option, EmulationMachineTab.Controllers, SettingsConstants.ResourceNativeControls)).ToArray())];
}
