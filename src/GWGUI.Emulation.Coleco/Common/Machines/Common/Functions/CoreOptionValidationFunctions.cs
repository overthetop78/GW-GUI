using AudioFunctions = GWGUI.Emulation.Coleco.Common.Functions.AudioFunctions;
using GWGUI.Emulation;
namespace GWGUI.Emulation.Coleco.Common.Machines.Common.Functions;

internal static class CoreOptionValidationFunctions
{
    internal static IReadOnlyList<CoreOption> ForCore(Emulator core) => (new Engine().Adapter(core) as IEmulatorSettings)?.GetSettingsOptions() ?? [];

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
                if (AudioFunctions.Processing.HasInvalidConfiguredLatency(option.Value))
                    throw new ArgumentException(nameof(configuration.Options));
                continue;
            }
            if (option.Key == VideoAudioSettingsConstants.AudioVolumeOption)
            {
                if (AudioFunctions.Processing.HasInvalidConfiguredVolume(option.Value, VideoAudioSettingsConstants.MinimumVolumePercent, VideoAudioSettingsConstants.MaximumVolumePercent))
                    throw new ArgumentException(nameof(configuration.Options));
                continue;
            }
            if (!catalog.Any(definition => definition.Key == option.Key && definition.Values.Any(choice => choice.Value == option.Value)))
                throw new ArgumentException(nameof(configuration.Options));
        }
        if (new Engine().Adapter(configuration) is IEmulatorConfigurationValidation validator)
            validator.ValidateConfiguration(configuration);
    }
}
