using GWGUI.Emulation.Philips.Emulators.O2em.Constants;
using GWGUI.Emulation.Philips.Emulators.O2em.Dictionaries;
using CommonSettings = GWGUI.Emulation.Philips.Common.Machines.Common.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Philips.Emulators.O2em.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<CoreOption> GetOptions() => O2emOptionCatalog.All;

    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var blocks = new List<EmulationSettingsBlock>();
        foreach (var group in GetOptions().Where(option => option.IsVisible).GroupBy(option => option.Key switch
        {
            OptionConstants.SwapGamepads => EmulationMachineTab.Controllers,
            OptionConstants.AudioVolume or OptionConstants.VoiceVolume or OptionConstants.LowPassFilter or OptionConstants.LowPassRange => EmulationMachineTab.Audio,
            _ => EmulationMachineTab.Video
        }))
        {
            var resource = group.Key switch
            {
                EmulationMachineTab.Audio => SettingsConstants.ResourceNativeAudio,
                EmulationMachineTab.Controllers => SettingsConstants.ResourceControllerMappings,
                EmulationMachineTab.Cpu => SettingsConstants.ResourceCpuAcceleration,
                EmulationMachineTab.Keyboard => SettingsConstants.ResourceNativeKeyboard,
                EmulationMachineTab.General => SettingsConstants.ResourceNativeSystem,
                _ => SettingsConstants.ResourceNativeVideo
            };
            blocks.Add(new(resource, group.Key, resource, group.Select(option => CommonSettings.Native(configuration, option, group.Key, resource)
                with { IsGlobalControllerOption = group.Key == EmulationMachineTab.Controllers }).ToArray())
                { IsCompact = group.Key == EmulationMachineTab.Audio });
        }
        return blocks;
    }
}
