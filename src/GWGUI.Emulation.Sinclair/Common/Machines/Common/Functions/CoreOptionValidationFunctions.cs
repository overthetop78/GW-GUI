using System.Globalization;

namespace GWGUI.Emulation.Sinclair.Common.Machines.Common.Functions;
internal static class CoreOptionValidationFunctions
{
    internal static IReadOnlyList<CoreOption> ForCore(Emulator core) => core == Emulator.None ? [] : new Engine().Adapter(core) is IEmulatorSettings settings ? settings.GetSettingsOptions() : [];
    internal static IReadOnlyList<CoreOption> ForConfiguration(MachineConfiguration configuration) => configuration.Core == Emulator.None ? [] : new Engine().Adapter(configuration) is IEmulatorSettings settings ? settings.GetSettingsOptions(configuration) : [];

    internal static void Validate(MachineConfiguration configuration)
    {
        var catalog = ForConfiguration(configuration);
        foreach (var option in configuration.Options)
        {
            var valid = option.Key switch
            {
                SettingsConstants.AudioOutput => !string.IsNullOrWhiteSpace(option.Value),
                SettingsConstants.AudioLatency => !EmulationAudioFunctions.HasInvalidConfiguredLatency(option.Value),
                VideoAudioSettingsConstants.AudioVolumeOption => !EmulationAudioFunctions.HasInvalidConfiguredVolume(option.Value, VideoAudioSettingsConstants.MinimumVolumePercent, VideoAudioSettingsConstants.MaximumVolumePercent),
                _ => catalog.Any(definition => definition.Key == option.Key && definition.Values.Any(choice => choice.Value == option.Value) && (definition.IsEnabled || option.Value == definition.DefaultValue))
            };
            if (!valid)
                throw new ArgumentException(nameof(configuration.Options));
        }
    }
}