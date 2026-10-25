using GWGUI.Emulation.Bandai.Emulators.PlaydiaEmu.Dictionaries;
using static GWGUI.Emulation.Bandai.Common.Machines.Common.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Bandai.Emulators.PlaydiaEmu.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration) =>
    [
        Block(EmulationMachineTab.General, SettingsConstants.ResourceEmulator, Information(SettingsConstants.PlaydiaHle, EmulationMachineTab.General, SettingsConstants.ResourceEmulator, SettingsConstants.ResourcePlaydiaHle, string.Empty)),
        Block(EmulationMachineTab.Controllers, SettingsConstants.ResourceNativeControls, PlaydiaEmuOptionCatalog.All.Where(option => option.IsVisible).Select(option => Native(configuration, option, EmulationMachineTab.Controllers, SettingsConstants.ResourceNativeControls)).ToArray())
    ];
}