using GWGUI.Emulation.Apple.Emulators.AppleWin.Dictionaries;
using static GWGUI.Emulation.Apple.Common.Machines.Common.Functions.SettingsDescriptionFunctions;
using AppleWinOptions = GWGUI.Emulation.Apple.Emulators.AppleWin.Constants.OptionConstants;
using Cards = GWGUI.Emulation.Apple.Emulators.AppleWin.Constants.ExpansionConstants;

namespace GWGUI.Emulation.Apple.Emulators.AppleWin.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var blocks = new List<EmulationSettingsBlock>();
        CoreOption Option(string key) => AppleWinOptionCatalog.All.Single(option => option.Key == key);
        blocks.Add(Block(EmulationMachineTab.Rom, SettingsConstants.ResourceFirmware,
            new EmulationSettingsField(SettingsConstants.Firmware, EmulationMachineTab.Rom,
                EmulationMachineTab.Rom.ToString(), SettingsConstants.ResourceFirmware, EmulationSettingsEditor.Information,
                SettingsConstants.Firmware, [new(SettingsConstants.Firmware, SettingsConstants.ResourceEmbeddedFirmware)], IsEnabled: false)));
        blocks.Add(Block(EmulationMachineTab.Cpu, SettingsConstants.ResourceExpansion,
            ExpansionFunctions.Fixed(configuration, Cards.Z80Field, Cards.Z80, AppleWinOptions.Slot5, SettingsChoiceConstants.ValueZ80SoftCard, SettingsTextConstants.SettingCardInSlot5, EmulationMachineTab.Cpu)));
        blocks.Add(Block(EmulationMachineTab.Audio, SettingsConstants.ResourceExpansion,
            ExpansionFunctions.AudioCard(configuration, Cards.MockingboardField, Cards.Mockingboard, SettingsChoiceConstants.ValueMockingboardC),
            ExpansionFunctions.AudioCard(configuration, Cards.PhasorField, Cards.Phasor, SettingsChoiceConstants.ValuePhasor),
            ExpansionFunctions.Fixed(configuration, Cards.SamField, Cards.Sam, AppleWinOptions.Slot5, SettingsChoiceConstants.ValueSAM, SettingsTextConstants.SettingCardInSlot5, EmulationMachineTab.Audio)));
        blocks.Add(Block(EmulationMachineTab.Mouse, SettingsConstants.ResourceExpansion,
            ExpansionFunctions.Fixed(configuration, Cards.MouseField, Cards.Mouse, AppleWinOptions.Slot4, SettingsChoiceConstants.ValueMouseCard, SettingsTextConstants.SettingCardInSlot4, EmulationMachineTab.Mouse)));
        blocks.Add(Block(EmulationMachineTab.Video, SettingsConstants.ResourceNativeVideo,
            ExpansionFunctions.Fixed(configuration, Cards.VideoField, Cards.VideoHd, AppleWinOptions.Slot3, SettingsChoiceConstants.ValueVideoHD, SettingsTextConstants.SettingCardInSlot3, EmulationMachineTab.Video),
            Native(configuration, Option(AppleWinOptions.VideoRefreshRate), EmulationMachineTab.Video)));
        blocks.Add(Block(EmulationMachineTab.Storage, StorageSettingsFunctionsConstants.ResourcePlaylist,
            Native(configuration, Option(AppleWinOptions.PlaylistStart), EmulationMachineTab.Storage),
            Native(configuration, Option(AppleWinOptions.FloppyMultidrive), EmulationMachineTab.Storage)));
        return blocks;
    }
}
