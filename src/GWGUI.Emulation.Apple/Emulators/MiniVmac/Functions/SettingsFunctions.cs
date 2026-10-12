using static GWGUI.Emulation.Apple.Common.Machines.Common.Functions.SettingsDescriptionFunctions;
using GWGUI.Emulation.Apple.Emulators.MiniVmac.Dictionaries;
using MiniVmacConstants = GWGUI.Emulation.Apple.Emulators.MiniVmac.Constants.EmulatorConstants;
using MiniVmacOptions = GWGUI.Emulation.Apple.Emulators.MiniVmac.Constants.OptionConstants;

namespace GWGUI.Emulation.Apple.Emulators.MiniVmac.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var blocks = new List<EmulationSettingsBlock>();
        if (!ConfigurationFunctions.SupportsRuntime(configuration.Model))
            return [];
        blocks.Add(Block(EmulationMachineTab.Rom, SettingsConstants.ResourceFirmware,
            new EmulationSettingsField(SettingsConstants.Firmware, EmulationMachineTab.Rom,
                EmulationMachineTab.Rom.ToString(), SettingsConstants.ResourceFirmware, EmulationSettingsEditor.Path,
                configuration.Firmwares.FirstOrDefault(firmware => firmware.Category == FirmwareCategory.SystemRom)?.Path,
                DefaultFolderCategory: EmulationDefaultFolderCategory.Firmware, RequiresRestart: false)));
        blocks.Add(Block(EmulationMachineTab.Ram, SettingsConstants.ResourceRuntimeMemory,
            Information(SettingsConstants.ResourceRuntimeMemory, EmulationMachineTab.Ram,
                SettingsConstants.ResourceRuntimeMemory, $"{MiniVmacConstants.MainMemoryBytes / SettingsConstants.Kibibyte} KiB")));
        blocks.Add(Block(EmulationMachineTab.Video, SettingsConstants.ResourceNativeVideo,
            Native(configuration, MiniVmacOptionCatalog.All.Single(option => option.Key == MiniVmacOptions.Statusbar),
                EmulationMachineTab.Video)));
        return blocks;
    }
}
