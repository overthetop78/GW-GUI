using System.Globalization;
namespace GWGUI.Emulation.Philips.Common.Machines.Common.Functions;

internal static class CoreOptionValidationFunctions
{
    internal static IReadOnlyList<CoreOption> ForCore(Emulator core) => (new Engine().Adapter(core) as IEmulatorSettings)?.GetSettingsOptions() ?? [];

    internal static void Validate(MachineConfiguration configuration)
    {
        var catalog = ForCore(configuration.Core);
        foreach (var option in configuration.Options)
        {
            var valid = option.Key switch
            {
                SettingsConstants.AudioOutput => !string.IsNullOrWhiteSpace(option.Value),
                SettingsConstants.AudioLatency => int.TryParse(option.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var latency) && EmulationSettingConstants.AudioLatencyMillisecondsChoices.Contains(latency),
                VideoAudioSettingsConstants.AudioVolumeOption => int.TryParse(option.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var volume) && volume >= VideoAudioSettingsConstants.MinimumVolumePercent
                    && volume <= VideoAudioSettingsConstants.MaximumVolumePercent,
                _ => catalog.Any(definition => definition.Key == option.Key && definition.Values.Any(choice => choice.Value == option.Value))
            };
            if (!valid) throw new ArgumentException(nameof(configuration.Options));
        }
    }
}
