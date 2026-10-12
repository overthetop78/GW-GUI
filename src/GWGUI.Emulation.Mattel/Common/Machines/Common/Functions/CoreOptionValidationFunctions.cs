using System.Globalization;
namespace GWGUI.Emulation.Mattel.Common.Machines.Common.Functions;

internal static class CoreOptionValidationFunctions
{
    internal static IReadOnlyList<CoreOption> ForCore(Emulator core) => core switch
    {
        Emulator.FreeIntv => Emulators.FreeIntv.Dictionaries.FreeIntvOptionCatalog.All,
        _ => throw new ArgumentOutOfRangeException(nameof(core))
    };

    internal static void Validate(MachineConfiguration configuration)
    {
        var catalog = ForCore(configuration.Core);
        foreach (var option in configuration.Options)
        {
            var valid = option.Key switch
            {
                SettingsConstants.AudioOutput => !string.IsNullOrWhiteSpace(option.Value),
                SettingsConstants.AudioLatency => int.TryParse(option.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var latency) && SettingsConstants.Latencies.Contains(latency),
                VideoAudioSettingsConstants.AudioVolumeOption => int.TryParse(option.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var volume) && volume >= VideoAudioSettingsConstants.MinimumVolumePercent
                    && volume <= VideoAudioSettingsConstants.MaximumVolumePercent,
                _ => catalog.Any(definition => definition.Key == option.Key && definition.Values.Any(choice => choice.Value == option.Value))
            };
            if (!valid) throw new ArgumentException(nameof(configuration.Options));
        }
    }
}
