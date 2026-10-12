using GWGUI.Emulation.Coleco.Emulators.GearColeco.Dictionaries;
using GWGUI.Emulation.Coleco.Emulators.JollyCV.Dictionaries;
using GWGUI.Emulation.Coleco.Emulators.BlueMSX.Dictionaries;

namespace GWGUI.Emulation.Coleco.Common.Machines.Common.Functions;

internal static class CoreOptionValidationFunctions
{
    internal static IReadOnlyList<CoreOption> ForCore(Emulator core) => core switch
    {
        Emulator.GearColeco => GearColecoOptionCatalog.All,
        Emulator.JollyCV => JollyCVOptionCatalog.All,
        Emulator.BlueMSX => BlueMSXOptionCatalog.All,
        _ => throw new ArgumentOutOfRangeException(nameof(core))
    };

    internal static void Validate(MachineConfiguration configuration)
    {
        var catalog = ForCore(configuration.Core);
        foreach (var option in configuration.Options)
        {
            if (option.Key == SettingsConstants.AudioOutput)
            {
                if (string.IsNullOrWhiteSpace(option.Value)) throw new ArgumentException(nameof(configuration.Options));
                continue;
            }
            if (option.Key == SettingsConstants.AudioLatency)
            {
                if (!int.TryParse(option.Value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var latency)
                    || !SettingsConstants.Latencies.Contains(latency))
                    throw new ArgumentException(nameof(configuration.Options));
                continue;
            }
            if (option.Key == VideoAudioSettingsConstants.AudioVolumeOption)
            {
                if (!int.TryParse(option.Value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var volume)
                    || volume is < VideoAudioSettingsConstants.MinimumVolumePercent
                        or > VideoAudioSettingsConstants.MaximumVolumePercent)
                    throw new ArgumentException(nameof(configuration.Options));
                continue;
            }
            if (!catalog.Any(definition => definition.Key == option.Key && definition.Values.Any(choice => choice.Value == option.Value)))
                throw new ArgumentException(nameof(configuration.Options));
        }
        if (configuration.Core == Emulator.GearColeco
            && configuration.Options.TryGetValue(Emulators.GearColeco.Constants.OptionConstants.OptionCartridgeHardware, out var hardware)
            && hardware != (configuration.Model == MachineModel.Adam ? CoreModelConstants.Adam : CoreModelConstants.ColecoVision))
            throw new ArgumentException(nameof(configuration.Options));
        if (configuration.Core == Emulator.BlueMSX
            && configuration.Options.TryGetValue(Emulators.BlueMSX.Constants.OptionConstants.OptionMsxtype, out var machine)
            && machine is not CoreModelConstants.Automatic and not CoreModelConstants.ColecoVision)
            throw new ArgumentException(nameof(configuration.Options));
    }
}
