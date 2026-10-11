using System.Globalization;

namespace GWGUI.Emulation.Sharp.Common.Machines.Common.Functions;

internal static class CoreOptionValidationFunctions
{
    internal static IReadOnlyList<CoreOption> ForCore(Emulator core) => core switch { Emulator.None => [], Emulator.Px68k => GWGUI.Emulation.Sharp.Emulators.Px68k.Dictionaries.OptionCatalog.Options, Emulator.X1 => GWGUI.Emulation.Sharp.Emulators.X1.Dictionaries.OptionCatalog.Options, _ => throw new ArgumentOutOfRangeException(nameof(core)) };

    internal static IReadOnlyList<CoreOption> ForConfiguration(MachineConfiguration configuration)
    {
        var defaults = configuration.Core switch
        { Emulator.None => new Dictionary<string, string>(), Emulator.Px68k => GWGUI.Emulation.Sharp.Emulators.Px68k.Functions.RuntimeOptionFunctions.Defaults(configuration.Model), Emulator.X1 => GWGUI.Emulation.Sharp.Emulators.X1.Functions.RuntimeOptionFunctions.Defaults(configuration.Model), _ => throw new ArgumentOutOfRangeException(nameof(configuration))
        };
        return ForCore(configuration.Core).Select(option => option with { DefaultValue = defaults.GetValueOrDefault(option.Key, option.DefaultValue), CurrentValue = configuration.Options.GetValueOrDefault(option.Key, defaults.GetValueOrDefault(option.Key, option.DefaultValue)) }).ToArray();
    }
    internal static void Validate(MachineConfiguration configuration)
    {
        var catalog = ForCore(configuration.Core);
        foreach (var option in configuration.Options)
        {
            var valid = option.Key switch
            { SettingsConstants.AudioOutput => !string.IsNullOrWhiteSpace(option.Value), SettingsConstants.AudioLatency => int.TryParse(option.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var latency) && SettingsConstants.Latencies.Contains(latency), VideoAudioSettingsConstants.AudioVolumeOption => int.TryParse(option.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var volume) && volume >= VideoAudioSettingsConstants.MinimumVolumePercent && volume <= VideoAudioSettingsConstants.MaximumVolumePercent, _ => catalog.Any(definition => definition.Key == option.Key && definition.Values.Any(choice => choice.Value == option.Value))
            };
            if (!valid) throw new ArgumentException(nameof(configuration.Options));
        }
    }
}
