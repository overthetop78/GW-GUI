using Options = GWGUI.Emulation.Sinclair.Emulators.Fuse.Dictionaries.OptionCatalog;
using CommonSettings = GWGUI.Emulation.Sinclair.Common.Machines.Common.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Sinclair.Emulators.Fuse.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<CoreOption> GetOptions(MachineConfiguration? configuration)
    {
        if (configuration is null) return Options.Options;
        var defaults = RuntimeOptionFunctions.Defaults(configuration.Model);
        return Options.Options.Select(option => option with
        {
            DefaultValue = defaults.GetValueOrDefault(option.Key, option.DefaultValue),
            CurrentValue = configuration.Options.GetValueOrDefault(option.Key, defaults.GetValueOrDefault(option.Key, option.DefaultValue)),
            IsEnabled = RuntimeOptionFunctions.IsAvailable(configuration, option.Key)
        }).ToArray();
    }

    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration) =>
        GetOptions(configuration).Where(option => option.IsVisible && option.Category is not null)
            .GroupBy(option => Enum.Parse<EmulationMachineTab>(option.Category!))
            .Select(group =>
            {
                var resource = group.Key switch
                {
                    EmulationMachineTab.Audio => SettingsConstants.ResourceNativeAudio,
                    EmulationMachineTab.Keyboard => SettingsConstants.ResourceNativeKeyboard,
                    EmulationMachineTab.Storage => SettingsConstants.ResourceNativeStorage,
                    EmulationMachineTab.Cpu => SettingsConstants.ResourceCpuAcceleration,
                    EmulationMachineTab.Ram => SettingsConstants.ResourceMemoryMain,
                    _ => SettingsConstants.ResourceNativeVideo
                };
                return new EmulationSettingsBlock(resource + ".Options", group.Key, resource,
                    group.Select(option => CommonSettings.Native(configuration, option, group.Key, resource)).ToArray())
                    { IsCompact = group.Key == EmulationMachineTab.Audio };
            }).ToArray();
}
