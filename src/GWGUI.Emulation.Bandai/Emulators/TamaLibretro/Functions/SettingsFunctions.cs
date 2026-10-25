namespace GWGUI.Emulation.Bandai.Emulators.TamaLibretro.Functions;
internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration) => [GWGUI.Emulation.Bandai.Common.Machines.Common.Functions.SettingsDescriptionFunctions.Block(
            EmulationMachineTab.Rom,
            SettingsConstants.ResourceFirmware,
            new EmulationSettingsField(
                SettingsConstants.Firmware,
                EmulationMachineTab.Rom,
                SettingsConstants.ResourceFirmware,
                SettingsConstants.ResourceFirmware,
                EmulationSettingsEditor.Path,
                configuration.Firmwares.SingleOrDefault()?.Path,
                RequiresRestart: false
            )
        )];
}