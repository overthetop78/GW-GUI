using System.Globalization;

namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        var cpuResource = SettingsConstants.CpuResourcePrefix + hardware.DefaultCpu;
        var blocks = new List<EmulationSettingsBlock>
        {
            Block(EmulationMachineTab.Cpu, SettingsConstants.ResourceCpu,
                new EmulationSettingsField(SettingsConstants.Cpu, EmulationMachineTab.Cpu, EmulationMachineTab.Cpu.ToString(), SettingsConstants.ResourceCpu, EmulationSettingsEditor.Information, cpuResource, [new(cpuResource, cpuResource)]),
                Information(SettingsConstants.CpuFrequency, EmulationMachineTab.Cpu, SettingsConstants.ResourceFrequency, $"{hardware.DefaultCpuFrequencyHz / (double)SettingsConstants.Megahertz:0.##} MHz")),
            Block(EmulationMachineTab.Ram, SettingsConstants.ResourceMemory,
                Information(SettingsConstants.MainMemory, EmulationMachineTab.Ram, SettingsConstants.ResourceMemory, $"{hardware.MainMemoryBytes / SettingsConstants.Kibibyte} KiB")),
            Block(EmulationMachineTab.Video, SettingsConstants.ResourceVideo,
                Information(SettingsConstants.Video + "_controller", EmulationMachineTab.Video, SettingsConstants.ResourceVideoController, "Motorola MC6845"),
                Information(SettingsConstants.Video + "_pixels", EmulationMachineTab.Video, SettingsConstants.ResourceVideoPixels, "Acorn Video ULA"),
                Information(SettingsConstants.Video + "_teletext", EmulationMachineTab.Video, SettingsConstants.ResourceTeletext, "Mullard SAA5050")),
            Audio(configuration)
        };
        if (configuration.Core != Emulator.None && new Engine().Adapter(configuration) is IEmulatorSettings settings)
            blocks.InsertRange(BufferConstants.FirstCollectionIndex, settings.DescribeSettings(configuration));
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
            Selection(configuration, SettingsConstants.AudioLatency, SettingsConstants.ResourceAudioLatency, EmulationSettingConstants.DefaultAudioLatencyMilliseconds, EmulationSettingConstants.AudioLatencyMillisecondsChoices, "ms"),
            Selection(configuration, VideoAudioSettingsConstants.AudioVolumeOption, SettingsConstants.ResourceAudioVolume, EmulationSettingConstants.DefaultAudioVolumePercent, EmulationSettingConstants.AudioVolumePercentChoices, "%")) with { IsCompact = true };

    private static EmulationSettingsField Selection(MachineConfiguration configuration, string id, string resource, int defaultValue, IReadOnlyList<int> values, string unit) =>
        new(id, EmulationMachineTab.Audio, EmulationMachineTab.Audio.ToString(), resource,
            EmulationSettingsEditor.Selection, configuration.Options.GetValueOrDefault(id, defaultValue.ToString(CultureInfo.InvariantCulture)),
            values.Select(value => new EmulationSettingsChoice(value.ToString(CultureInfo.InvariantCulture), string.Empty, $"{value} {unit}")).ToArray());


    private static EmulationSettingsField Information(string id, EmulationMachineTab tab, string resource, string value) => new(id, tab, tab.ToString(), resource, EmulationSettingsEditor.Information, value);

    private static EmulationSettingsBlock Block(EmulationMachineTab tab, string resource, params EmulationSettingsField[] fields) => new(resource, tab, resource, fields);
}
