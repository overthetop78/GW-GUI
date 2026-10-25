using AudioFunctions = GWGUI.Emulation.Epoch.Common.Functions.AudioFunctions;
using GWGUI.Emulation;
using System.Globalization;

namespace GWGUI.Emulation.Epoch.Common.Machines.Common.Functions;

internal static class CoreOptionValidationFunctions
{
    internal static IReadOnlyList<CoreOption> ForCore(Emulator core) => new Engine().Adapter(core).GetSettingsOptions();

    internal static void Validate(MachineConfiguration configuration)
    {
        var catalog = ForCore(configuration.Core);
        foreach (var option in configuration.Options)
        {
            var valid = option.Key switch
            {
                SettingsConstants.AudioOutput => !string.IsNullOrWhiteSpace(option.Value),
                SettingsConstants.AudioLatency => !AudioFunctions.Processing.HasInvalidConfiguredLatency(option.Value),
                VideoAudioSettingsConstants.AudioVolumeOption => !AudioFunctions.Processing.HasInvalidConfiguredVolume(option.Value, VideoAudioSettingsConstants.MinimumVolumePercent, VideoAudioSettingsConstants.MaximumVolumePercent),
                _ => catalog.Any(definition => definition.Key == option.Key && definition.Values.Any(choice => choice.Value == option.Value))
            };
            if (!valid) throw new ArgumentException(nameof(configuration.Options));
        }
        if (new Engine().Adapter(configuration) is { } validator)
            validator.ValidateConfiguration(configuration);
    }
}
