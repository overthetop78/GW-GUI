using GWGUI.Emulation.Sharp.Emulators.Px68k.Dictionaries;
using CommonSettings = GWGUI.Emulation.Sharp.Common.Machines.Common.Functions.SettingsDescriptionFunctions;
using Options = GWGUI.Emulation.Sharp.Emulators.Px68k.Dictionaries.OptionCatalog;

namespace GWGUI.Emulation.Sharp.Emulators.Px68k.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<CoreOption> GetOptions(MachineConfiguration? configuration)
    {
        if (configuration is null) return Options.Options;
        var defaults = RuntimeOptionFunctions.Defaults(configuration.Model);
        return Options.Options.Select(option => option with
        {
            DefaultValue = defaults.GetValueOrDefault(option.Key, option.DefaultValue),
            CurrentValue = configuration.Options.GetValueOrDefault(option.Key, defaults.GetValueOrDefault(option.Key, option.DefaultValue))
        }).ToArray();
    }

    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration) =>
        GetOptions(configuration).Where(option => option.IsVisible && option.Category is not null)
            .GroupBy(option => Enum.Parse<EmulationMachineTab>(option.Category!))
            .Select(group =>
            {
                var resource = group.Key switch
                {
                    EmulationMachineTab.Controllers => SettingsConstants.ResourceNativeControls,
                    EmulationMachineTab.Audio => SettingsConstants.ResourceNativeAudio,
                    EmulationMachineTab.Cpu => SettingsConstants.ResourceCpuAcceleration,
                    EmulationMachineTab.Ram => SettingsConstants.ResourceMemoryMain,
                    _ => SettingsConstants.ResourceNativeVideo
                };
                return new EmulationSettingsBlock(resource + ".Options", group.Key, resource,
                    group.Select(option => CommonSettings.Native(configuration, option, group.Key, resource)).ToArray())
                    { IsCompact = group.Key == EmulationMachineTab.Audio };
            }).ToArray();
}
