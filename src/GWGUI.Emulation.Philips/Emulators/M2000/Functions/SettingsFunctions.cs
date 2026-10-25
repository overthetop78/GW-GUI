using GWGUI.Emulation.Philips.Emulators.M2000.Constants;
using GWGUI.Emulation.Philips.Emulators.M2000.Dictionaries;
using CommonSettings = GWGUI.Emulation.Philips.Common.Machines.Common.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Philips.Emulators.M2000.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<CoreOption> GetOptions() => M2000OptionCatalog.All;

    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var blocks = new List<EmulationSettingsBlock>();
        foreach (var group in GetOptions().Where(option => option.IsVisible).GroupBy(option => EmulationMachineTab.Keyboard))
        {
            var resource = group.Key switch
            {
                EmulationMachineTab.Audio => SettingsConstants.ResourceNativeAudio,
                EmulationMachineTab.Cpu => SettingsConstants.ResourceCpuAcceleration,
                EmulationMachineTab.Keyboard => SettingsConstants.ResourceNativeKeyboard,
                EmulationMachineTab.General => SettingsConstants.ResourceNativeSystem,
                _ => SettingsConstants.ResourceNativeVideo
            };
            blocks.Add(new(resource, group.Key, resource, group.Select(option => CommonSettings.Native(configuration, option, group.Key, resource)).ToArray())
                { IsCompact = group.Key == EmulationMachineTab.Audio });
        }
        return blocks;
    }
}
