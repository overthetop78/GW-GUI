using System.Globalization;

namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        var blocks = new List<EmulationSettingsBlock>
        {
            Block(EmulationMachineTab.Cpu, SettingsConstants.ResourceCpu,
                Information(SettingsConstants.Cpu, EmulationMachineTab.Cpu, SettingsConstants.ResourceCpu, hardware.DefaultCpu.ToString()),
                Information(SettingsConstants.CpuFrequency, EmulationMachineTab.Cpu, SettingsConstants.ResourceFrequency, EmulationHardwareSettingsFunctions.FormatMegahertz(hardware.DefaultCpuFrequencyHz, "0.##"))),
            Block(EmulationMachineTab.Ram, SettingsConstants.ResourceMemory,
                Information(SettingsConstants.MainMemory, EmulationMachineTab.Ram, SettingsConstants.ResourceMemory, $"{hardware.MainMemoryBytes / SettingsConstants.Kibibyte} KiB")),
            Audio(configuration)
        };
        if (new Engine().SelectedAdapter(configuration) is IEmulatorSettings settings) blocks.AddRange(settings.DescribeSettings(configuration));
        return blocks;
    }

    private static EmulationSettingsBlock Audio(MachineConfiguration configuration) =>
        Block(EmulationMachineTab.Audio, SettingsConstants.ResourceAudio,
            new EmulationSettingsField(SettingsConstants.AudioEnabled, EmulationMachineTab.Audio,
                EmulationMachineTab.Audio.ToString(), SettingsConstants.ResourceAudioEnabled,
                EmulationSettingsEditor.Toggle, configuration.AudioEnabled ? EmulationSettingConstants.Enabled : EmulationSettingConstants.Disabled),
            new EmulationSettingsField(SettingsConstants.AudioOutput, EmulationMachineTab.Audio,
                EmulationMachineTab.Audio.ToString(), SettingsConstants.ResourceAudioOutput,
                EmulationSettingsEditor.Selection, configuration.Options.GetValueOrDefault(SettingsConstants.AudioOutput, SettingsConstants.DefaultOutput),
                [new(SettingsConstants.DefaultOutput, SettingsConstants.ResourceDefaultOutput)],
                ChoiceSource: EmulationSettingsChoiceSource.AudioOutputDevices),
            AudioSelection(configuration, SettingsConstants.AudioLatency, SettingsConstants.ResourceAudioLatency, EmulationSettingConstants.DefaultAudioLatencyMilliseconds, EmulationSettingConstants.AudioLatencyMillisecondsChoices, "ms"),
            AudioSelection(configuration, VideoAudioSettingsConstants.AudioVolumeOption, SettingsConstants.ResourceAudioVolume, EmulationSettingConstants.DefaultAudioVolumePercent, EmulationSettingConstants.AudioVolumePercentChoices, "%"));

    private static EmulationSettingsField AudioSelection(MachineConfiguration configuration, string id, string resource, int fallback, IReadOnlyList<int> values, string unit) =>
        new(id, EmulationMachineTab.Audio, EmulationMachineTab.Audio.ToString(), resource,
            EmulationSettingsEditor.Selection, configuration.Options.GetValueOrDefault(id, fallback.ToString(CultureInfo.InvariantCulture)),
            values.Select(value => new EmulationSettingsChoice(value.ToString(CultureInfo.InvariantCulture), string.Empty, $"{value} {unit}")).ToArray());

    internal static EmulationSettingsField Native(MachineConfiguration configuration, CoreOption option, EmulationMachineTab tab)
    {
        var resource = option.ResourceKey ?? throw new InvalidOperationException($"Missing common resource for {option.Key}.");
        var value = configuration.Options.GetValueOrDefault(option.Key, option.DefaultValue);
        var choices = option.Values.Select((choice, index) => new EmulationSettingsChoice(choice.Value,
            choice.ResourceKey ?? (choice.InvariantDisplayValue is not null ? string.Empty : throw new InvalidOperationException($"Missing common choice resource for {option.Key}: {choice.Value}.")), choice.InvariantDisplayValue))
            .ToArray();
        return new(option.Key, tab, tab.ToString(), resource, EmulationSettingsEditor.Selection, value, choices, RequiresRestart: false);
    }

    internal static string Resolution(int width, int height) => $"{width} × {height}";

    internal static EmulationSettingsField Information(string id, EmulationMachineTab tab, string resource, string value) => new(id, tab, tab.ToString(), resource, EmulationSettingsEditor.Information, value, IsEnabled: false);

    internal static EmulationSettingsBlock Block(EmulationMachineTab tab, string resource, params EmulationSettingsField[] fields) => new(resource, tab, resource, fields);
}
