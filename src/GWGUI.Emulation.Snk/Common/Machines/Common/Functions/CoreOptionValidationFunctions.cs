using AudioFunctions = GWGUI.Emulation.Snk.Common.Functions.AudioFunctions;
using GWGUI.Emulation;
using System.Globalization;

namespace GWGUI.Emulation.Snk.Common.Machines.Common.Functions;
internal static class CoreOptionValidationFunctions
{
    internal static IReadOnlyList<CoreOption> ForCore(Emulator core) => core != Emulator.None && EmulatorCatalog.CreateAdapter(core)is IEmulatorSettingsAdapter adapter ? adapter.GetSettingsOptions() : [];
    internal static IReadOnlyList<CoreOption> ForConfiguration(MachineConfiguration configuration) => configuration.Core != Emulator.None && EmulatorCatalog.CreateAdapter(configuration.Core)is IEmulatorSettingsAdapter adapter ? adapter.GetSettingsOptions(configuration) : [];
    internal static void Validate(MachineConfiguration configuration)
    {
        var adapter = configuration.Core == Emulator.None ? null : EmulatorCatalog.CreateAdapter(configuration.Core);
        var catalog = (adapter as IEmulatorSettingsAdapter)?.GetSettingsOptions(configuration) ?? [];
        foreach (var option in configuration.Options)
        {
            var valid = option.Key switch
            {
                SettingsConstants.AudioOutput => !string.IsNullOrWhiteSpace(option.Value),
                SettingsConstants.AudioLatency => !AudioFunctions.Processing.HasInvalidConfiguredLatency(option.Value),
                VideoAudioSettingsConstants.AudioVolumeOption => !AudioFunctions.Processing.HasInvalidConfiguredVolume(option.Value, VideoAudioSettingsConstants.MinimumVolumePercent, VideoAudioSettingsConstants.MaximumVolumePercent),
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