using AudioFunctions = GWGUI.Emulation.Sinclair.Common.Functions.AudioFunctions;
using GWGUI.Emulation;
using System.Globalization;

namespace GWGUI.Emulation.Sinclair.Common.Machines.Common.Functions;
internal static class CoreOptionValidationFunctions
{
    internal static IReadOnlyList<CoreOption> ForCore(Emulator core) => core == Emulator.None ? [] : new Engine().Adapter(core).GetSettingsOptions();
    internal static IReadOnlyList<CoreOption> ForConfiguration(MachineConfiguration configuration) => configuration.Core == Emulator.None ? [] : new Engine().Adapter(configuration).GetSettingsOptions(configuration);

    internal static void Validate(MachineConfiguration configuration)
    {
        var catalog = ForConfiguration(configuration);
        foreach (var option in configuration.Options)
        {
            var valid = option.Key switch
            {
                SettingsConstants.AudioOutput => !string.IsNullOrWhiteSpace(option.Value),
                SettingsConstants.AudioLatency => !AudioFunctions.Processing.HasInvalidConfiguredLatency(option.Value),
                VideoAudioSettingsConstants.AudioVolumeOption => !AudioFunctions.Processing.HasInvalidConfiguredVolume(option.Value, VideoAudioSettingsConstants.MinimumVolumePercent, VideoAudioSettingsConstants.MaximumVolumePercent),
                _ => catalog.Any(definition => definition.Key == option.Key && definition.Values.Any(choice => choice.Value == option.Value) && (definition.IsEnabled || option.Value == definition.DefaultValue))
            };
            if (!valid)
                throw new ArgumentException(nameof(configuration.Options));
        }
    }
}