using System.Globalization;

namespace GWGUI.Emulation.Thomson.Common.Machines.Common.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        var blocks = new List<EmulationSettingsBlock>();
        if (configuration.Core != Emulator.None && new Engine().Adapter(configuration)is IEmulatorSettings settings)
            blocks.AddRange(settings.DescribeSettings(configuration));
        blocks.Add(new(
            SettingsConstants.ResourceCpuProcessor,
            EmulationMachineTab.Cpu,
            SettingsConstants.ResourceCpuProcessor,
            [
            Information(SettingsConstants.CpuModel, EmulationMachineTab.Cpu, SettingsConstants.ResourceCpuProcessor, SettingsConstants.ResourceCpuModel, hardware.CpuName),
            Information(
            SettingsConstants.CpuFrequency,
            EmulationMachineTab.Cpu,
            SettingsConstants.ResourceCpuProcessor,
            SettingsConstants.ResourceCpuSpeedOriginal,
            $"{(hardware.CpuFrequencyHz / SettingsConstants.HertzPerMegahertz).ToString("0.######", CultureInfo.CurrentCulture)} {SettingsConstants.MegahertzUnit}"
        )
        ],
            IsCompact: true
        ));
        blocks.Add(new(
            SettingsConstants.ResourceMemoryMain,
            EmulationMachineTab.Ram,
            SettingsConstants.ResourceMemoryMain,
            [Information(
            SettingsConstants.MainMemory,
            EmulationMachineTab.Ram,
            SettingsConstants.ResourceMemoryMain,
            SettingsConstants.ResourceMemoryMain,
            $"{hardware.MainMemoryBytes / SettingsConstants.BytesPerKibibyte} {SettingsConstants.KibibyteUnit}"
        )],
            IsCompact: true
        ));
        blocks.Add(Audio(configuration));
        var speeds = Enumerable.Range(
            InputMappingConstants.MinimumMouseSpeedPercent / SettingsConstants.MouseSpeedStepPercent,
            (InputMappingConstants.MaximumMouseSpeedPercent - InputMappingConstants.MinimumMouseSpeedPercent) / SettingsConstants.MouseSpeedStepPercent + 1
        ).Select(step => step * SettingsConstants.MouseSpeedStepPercent).Select(value => new EmulationSettingsChoice(value.ToString(CultureInfo.InvariantCulture), string.Empty, $"{value} {SettingsConstants.PercentUnit}")).ToArray();
        blocks.Add(new(
            SettingsConstants.ResourceMouse,
            EmulationMachineTab.Mouse,
            SettingsConstants.ResourceMouse,
            [new(
            SettingsConstants.MouseSpeed,
            EmulationMachineTab.Mouse,
            SettingsConstants.ResourceMouse,
            SettingsConstants.ResourceMouseSpeed,
            EmulationSettingsEditor.Selection,
            configuration.Input.MouseSpeedPercent.ToString(CultureInfo.InvariantCulture),
            speeds
        )],
            IsCompact: true
        ));
        return blocks;
    }
    private static EmulationSettingsField Information(string id, EmulationMachineTab tab, string block, string label, string value) => new(id, tab, block, label, EmulationSettingsEditor.Information, value);
    private static EmulationSettingsBlock Audio(MachineConfiguration configuration) => new(
        SettingsConstants.ResourceAudio,
        EmulationMachineTab.Audio,
        SettingsConstants.ResourceAudio,
        [
        new(
        SettingsConstants.AudioEnabled,
        EmulationMachineTab.Audio,
        SettingsConstants.ResourceAudio,
        SettingsConstants.ResourceAudioEnabled,
        EmulationSettingsEditor.Toggle,
        configuration.AudioEnabled ? EmulationSettingConstants.Enabled : EmulationSettingConstants.Disabled
    ),
        new(
        SettingsConstants.AudioOutput,
        EmulationMachineTab.Audio,
        SettingsConstants.ResourceAudio,
        SettingsConstants.ResourceAudioOutput,
        EmulationSettingsEditor.Selection,
        configuration.Options.GetValueOrDefault(SettingsConstants.AudioOutput, SettingsConstants.DefaultOutput),
        [new(SettingsConstants.DefaultOutput, SettingsConstants.ResourceDefaultOutput)],
        ChoiceSource: EmulationSettingsChoiceSource.AudioOutputDevices
    ),
        AudioSelection(configuration, SettingsConstants.AudioLatency, SettingsConstants.ResourceAudioLatency, SettingsConstants.DefaultLatency, SettingsConstants.Latencies, SettingsConstants.MillisecondsUnit),
        AudioSelection(configuration, VideoAudioSettingsConstants.AudioVolumeOption, SettingsConstants.ResourceAudioVolume, SettingsConstants.DefaultVolume, SettingsConstants.Volumes, SettingsConstants.PercentUnit)
    ],
        IsCompact: true
    );
    private static EmulationSettingsField AudioSelection(MachineConfiguration configuration, string id, string resource, int fallback, IReadOnlyList<int> values, string unit) => new(
        id,
        EmulationMachineTab.Audio,
        SettingsConstants.ResourceAudio,
        resource,
        EmulationSettingsEditor.Selection,
        configuration.Options.GetValueOrDefault(id, fallback.ToString(CultureInfo.InvariantCulture)),
        values.Select(value => new EmulationSettingsChoice(value.ToString(CultureInfo.InvariantCulture), string.Empty, $"{value.ToString(CultureInfo.InvariantCulture)} {unit}")).ToArray()
    );
}
