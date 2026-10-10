using GWGUI.Emulation.Apple.Emulators.MiniVmac.Dictionaries;
using MiniVmacConstants = GWGUI.Emulation.Apple.Emulators.MiniVmac.Constants.EmulatorConstants;
using MiniVmacOptions = GWGUI.Emulation.Apple.Emulators.MiniVmac.Constants.OptionConstants;

namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
    private static void AddMacintosh(List<EmulationSettingsBlock> blocks, MachineConfiguration configuration)
    {
        if (!ConfigurationFunctions.SupportsRuntime(configuration.Model))
        {
            blocks.Add(Block(EmulationMachineTab.General, SettingsConstants.ResourceGeneral,
                Information(SettingsConstants.ResourceUnavailableModel, EmulationMachineTab.General,
                    SettingsConstants.ResourceUnavailableModel, string.Empty)));
            return;
        }
        blocks.Add(Block(EmulationMachineTab.Rom, SettingsConstants.ResourceFirmware,
            new EmulationSettingsField(SettingsConstants.Firmware, EmulationMachineTab.Rom,
                EmulationMachineTab.Rom.ToString(), SettingsConstants.ResourceFirmware, EmulationSettingsEditor.Path,
                configuration.Firmwares.FirstOrDefault(firmware => firmware.Category == FirmwareCategory.SystemRom)?.Path,
                DefaultFolderCategory: EmulationDefaultFolderCategory.Firmware, RequiresRestart: true)));
        blocks.Add(Block(EmulationMachineTab.Ram, SettingsConstants.ResourceRuntimeMemory,
            Information(SettingsConstants.ResourceRuntimeMemory, EmulationMachineTab.Ram,
                SettingsConstants.ResourceRuntimeMemory, $"{MiniVmacConstants.MainMemoryBytes / SettingsConstants.Kibibyte} KiB")));
        blocks.Add(Block(EmulationMachineTab.Video, SettingsConstants.ResourceNativeVideo,
            Native(configuration, MiniVmacOptionCatalog.All.Single(option => option.Key == MiniVmacOptions.Statusbar),
                EmulationMachineTab.Video)));
    }
}
