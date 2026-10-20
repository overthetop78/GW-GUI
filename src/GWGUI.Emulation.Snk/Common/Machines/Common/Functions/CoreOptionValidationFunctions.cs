using System.Globalization;

namespace GWGUI.Emulation.Snk.Common.Machines.Common.Functions;
internal static class CoreOptionValidationFunctions
{
    internal static IReadOnlyList<CoreOption> ForCore(Emulator core) => core != Emulator.None && EmulatorCatalog.CreateAdapter(core)is IEmulatorSettingsAdapter adapter ? adapter.CoreOptions : [];
    internal static IReadOnlyList<CoreOption> ForConfiguration(MachineConfiguration configuration) => configuration.Core != Emulator.None && EmulatorCatalog.CreateAdapter(configuration.Core)is IEmulatorSettingsAdapter adapter ? adapter.DescribeOptions(configuration) : [];
    internal static void Validate(MachineConfiguration configuration)
    {
        var adapter = configuration.Core == Emulator.None ? null : EmulatorCatalog.CreateAdapter(configuration.Core) as IEmulatorSettingsAdapter;
        var catalog = adapter?.DescribeOptions(configuration) ?? [];
        foreach (var option in configuration.Options)
        {
            var valid = option.Key switch
            {
                SettingsConstants.AudioOutput => !string.IsNullOrWhiteSpace(option.Value),
                SettingsConstants.AudioLatency => !EmulationAudioFunctions.HasInvalidConfiguredLatency(option.Value),
                VideoAudioSettingsConstants.AudioVolumeOption => !EmulationAudioFunctions.HasInvalidConfiguredVolume(option.Value, VideoAudioSettingsConstants.MinimumVolumePercent, VideoAudioSettingsConstants.MaximumVolumePercent),
                _ => adapter?.IsDeferredOption(configuration,
                    option.Key) 
                    == true 
                    && !string.IsNullOrEmpty(option.Value) 
                    || catalog.Any(definition => definition.Key 
                    == option.Key 
                    && definition.Values.Any(choice => choice.Value 
                    == option.Value) 
                    && (definition.IsEnabled 
                    || option.Value 
                    == definition.DefaultValue))
            };
            if (!valid)
                throw new ArgumentException(nameof(configuration.Options));
        }
    }
}