using System.Globalization;

namespace GWGUI.Emulation.Bandai.Common.Machines.Common.Functions;
internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var blocks = new List<EmulationSettingsBlock>
        {
            Audio(configuration)
        };
        AddHardware(blocks, configuration);
        if (new Engine().Adapter(configuration)is IEmulatorSettings settings)
            blocks.AddRange(settings.DescribeSettings(configuration));
        return blocks;
    }

    private static void AddHardware(List<EmulationSettingsBlock> blocks, MachineConfiguration configuration)
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        if (hardware is null)
            return;
        blocks.Add(Block(
                EmulationMachineTab.Cpu,
                SettingsConstants.ResourceCpuProcessor,
                new(
                    SettingsConstants.CpuModel,
                    EmulationMachineTab.Cpu,
                    SettingsConstants.ResourceCpuProcessor,
                    SettingsConstants.ResourceCpuModel,
                    EmulationSettingsEditor.Information,
                    hardware.CpuResourceKey,
                    [new(hardware.CpuResourceKey, hardware.CpuResourceKey)]
                ),
                Information(
                    SettingsConstants.CpuFrequency,
                    EmulationMachineTab.Cpu,
                    SettingsConstants.ResourceCpuProcessor,
                    SettingsConstants.ResourceCpuSpeedOriginal,
                    hardware.CpuFrequencyHz >= SettingsConstants.HertzPerMegahertz
                        ? $"{(hardware.CpuFrequencyHz / SettingsConstants.HertzPerMegahertz).ToString("0.######", CultureInfo.CurrentCulture)} {SettingsConstants.MegahertzUnit}"
                        : $"{(hardware.CpuFrequencyHz / SettingsConstants.HertzPerKilohertz).ToString("0.###", CultureInfo.CurrentCulture)} {SettingsConstants.KilohertzUnit}"
                )
            ));
        var memoryFields = new List<EmulationSettingsField>
        {
            Information(SettingsConstants.MainMemory, EmulationMachineTab.Ram, SettingsConstants.ResourceMemoryMain, SettingsConstants.ResourceMemoryMain, Memory(hardware.MainMemoryBytes))
        };
        if (hardware.DisplayMemoryBytes > 0)
            memoryFields.Add(Information(SettingsConstants.DisplayMemory, EmulationMachineTab.Ram, SettingsConstants.ResourceMemoryMain, SettingsConstants.ResourceDisplayMemory, Memory(hardware.DisplayMemoryBytes)));
        blocks.Add(Block(EmulationMachineTab.Ram, SettingsConstants.ResourceMemoryMain, memoryFields.ToArray()));
        blocks.Add(Block(
                EmulationMachineTab.Video,
                SettingsConstants.ResourceVideoResolution,
                Information(SettingsConstants.DisplayResolution, EmulationMachineTab.Video, SettingsConstants.ResourceVideoResolution, SettingsConstants.ResourceVideoResolution, $"{hardware.DisplayWidth} × {hardware.DisplayHeight}")
            ));
    }

    private static string Memory(long bytes) => bytes >= SettingsConstants.BytesPerKibibyte ? $"{(bytes / SettingsConstants.BytesPerKibibyte).ToString(CultureInfo.InvariantCulture)} {SettingsConstants.KibibyteUnit}" : $"{bytes.ToString(CultureInfo.InvariantCulture)} {SettingsConstants.ByteUnit}";
    internal static EmulationSettingsField Information(string id, EmulationMachineTab tab, string block, string label, string value) => new(id, tab, block, label, EmulationSettingsEditor.Information, value);
    internal static EmulationSettingsField Native(MachineConfiguration configuration, CoreOption option, EmulationMachineTab tab, string blockId)
    {
        var resource = option.ResourceKey ?? throw new InvalidOperationException($"Missing common resource for {option.Key}.");
        var choices = option.Values.Select(choice => new EmulationSettingsChoice(
                choice.Value,
                choice.ResourceKey ?? (choice.InvariantDisplayValue is not null ? string.Empty : throw new InvalidOperationException($"Missing common choice for {option.Key}/{choice.Value}.")),
                choice.InvariantDisplayValue
            )).ToArray();
        return new( option.Key, tab, blockId, resource, EmulationSettingsEditor.Selection, configuration.Options.GetValueOrDefault(option.Key, option.DefaultValue), choices, ExplanationResourceKey: option.ExplanationResourceKey, RequiresRestart: false );
    }

    private static EmulationSettingsBlock Audio(MachineConfiguration configuration) => Block(
        EmulationMachineTab.Audio,
        SettingsConstants.ResourceAudio,
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
        AudioSelection(configuration, SettingsConstants.AudioLatency, SettingsConstants.ResourceAudioLatency, EmulationSettingConstants.DefaultAudioLatencyMilliseconds, EmulationSettingConstants.AudioLatencyMillisecondsChoices, SettingsConstants.MillisecondsUnit),
        AudioSelection(configuration, VideoAudioSettingsConstants.AudioVolumeOption, SettingsConstants.ResourceAudioVolume, EmulationSettingConstants.DefaultAudioVolumePercent, EmulationSettingConstants.AudioVolumePercentChoices, SettingsConstants.PercentUnit)
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
    internal static EmulationSettingsBlock Block(EmulationMachineTab tab, string resource, params EmulationSettingsField[] fields) => new(resource, tab, resource, fields);
}