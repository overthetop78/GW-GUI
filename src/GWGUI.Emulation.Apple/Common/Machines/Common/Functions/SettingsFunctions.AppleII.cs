using GWGUI.Emulation.Apple.Emulators.AppleWin.Dictionaries;
using AppleWinOptions = GWGUI.Emulation.Apple.Emulators.AppleWin.Constants.OptionConstants;

namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
    private static void AddAppleII(List<EmulationSettingsBlock> blocks, MachineConfiguration configuration)
    {
        CoreOption Option(string key) => AppleWinOptionCatalog.All.Single(option => option.Key == key);
        blocks.Add(Block(EmulationMachineTab.Rom, SettingsConstants.ResourceFirmware,
            new EmulationSettingsField(SettingsConstants.Firmware, EmulationMachineTab.Rom,
                EmulationMachineTab.Rom.ToString(), SettingsConstants.ResourceFirmware, EmulationSettingsEditor.Information,
                null, ExplanationResourceKey: SettingsConstants.ResourceEmbeddedFirmware)));
        blocks.Add(Block(EmulationMachineTab.General, SettingsConstants.ResourceExpansion,
            Native(configuration, Option(AppleWinOptions.Slot3), EmulationMachineTab.General),
            Native(configuration, Option(AppleWinOptions.Slot4), EmulationMachineTab.General),
            Native(configuration, Option(AppleWinOptions.Slot5), EmulationMachineTab.General),
            Native(configuration, Option(AppleWinOptions.Slot7), EmulationMachineTab.General)));
        blocks.Add(Block(EmulationMachineTab.Video, SettingsConstants.ResourceNativeVideo,
            Native(configuration, Option(AppleWinOptions.VideoMode), EmulationMachineTab.Video),
            Native(configuration, Option(AppleWinOptions.VideoStyle), EmulationMachineTab.Video),
            Native(configuration, Option(AppleWinOptions.VideoRefreshRate), EmulationMachineTab.Video)));
        blocks.Add(Block(EmulationMachineTab.Storage, StorageSettingsFunctionsConstants.ResourcePlaylist,
            Native(configuration, Option(AppleWinOptions.PlaylistStart), EmulationMachineTab.Storage),
            Native(configuration, Option(AppleWinOptions.FloppyMultidrive), EmulationMachineTab.Storage)));
    }
}
