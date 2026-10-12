using System.Globalization;

namespace GWGUI.Emulation.Sinclair.Common.Machines.Common.Functions;
internal static class CoreOptionValidationFunctions
{
    internal static IReadOnlyList<CoreOption> ForCore(Emulator core) => core switch
    {
        Emulator.None => [],
        Emulator.EightyOne => GWGUI.Emulation.Sinclair.Emulators.EightyOne.Dictionaries.OptionCatalog.Options,
        Emulator.Fuse => GWGUI.Emulation.Sinclair.Emulators.Fuse.Dictionaries.OptionCatalog.Options,
        _ => throw new ArgumentOutOfRangeException(nameof(core))
    };
    internal static IReadOnlyList<CoreOption> ForConfiguration(MachineConfiguration configuration)
    {
        var defaults = configuration.Core switch
        {
            Emulator.None => new Dictionary<string, string>(),
            Emulator.EightyOne => GWGUI.Emulation.Sinclair.Emulators.EightyOne.Functions.RuntimeOptionFunctions.Defaults(configuration.Model),
            Emulator.Fuse => GWGUI.Emulation.Sinclair.Emulators.Fuse.Functions.RuntimeOptionFunctions.Defaults(configuration.Model),
            _ => throw new ArgumentOutOfRangeException(nameof(configuration))
        };
        return ForCore(configuration.Core).Select(option => option with { DefaultValue = defaults.GetValueOrDefault(option.Key, option.DefaultValue),
            CurrentValue = configuration.Options.GetValueOrDefault(option.Key, defaults.GetValueOrDefault(option.Key, option.DefaultValue)),
            IsEnabled = IsAvailable(configuration, option.Key) }).ToArray();
    }

    private static bool IsAvailable(MachineConfiguration configuration, string key) => configuration.Core switch
    {
        Emulator.None => false,
        Emulator.EightyOne => GWGUI.Emulation.Sinclair.Emulators.EightyOne.Functions.RuntimeOptionFunctions.IsAvailable(configuration, key),
        Emulator.Fuse => GWGUI.Emulation.Sinclair.Emulators.Fuse.Functions.RuntimeOptionFunctions.IsAvailable(configuration, key),
        _ => throw new ArgumentOutOfRangeException(nameof(configuration))
    };
    internal static void Validate(MachineConfiguration configuration)
    {
        var catalog = ForConfiguration(configuration);
        foreach (var option in configuration.Options)
        {
            var valid = option.Key switch
            {
                SettingsConstants.AudioOutput => !string.IsNullOrWhiteSpace(option.Value),
                SettingsConstants.AudioLatency => int.TryParse(option.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var latency) && SettingsConstants.Latencies.Contains(latency),
                VideoAudioSettingsConstants.AudioVolumeOption => int.TryParse(option.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var volume) && volume >= VideoAudioSettingsConstants.MinimumVolumePercent && volume <= VideoAudioSettingsConstants.MaximumVolumePercent,
                _ => catalog.Any(definition => definition.Key == option.Key && definition.Values.Any(choice => choice.Value == option.Value) && (definition.IsEnabled || option.Value == definition.DefaultValue))
            };
            if (!valid)
                throw new ArgumentException(nameof(configuration.Options));
        }
    }
}