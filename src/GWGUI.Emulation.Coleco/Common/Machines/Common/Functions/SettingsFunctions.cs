using System.Globalization;
namespace GWGUI.Emulation.Coleco.Common.Machines.Common.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var blocks = new List<EmulationSettingsBlock> { Audio(configuration) };
        AddHardware(blocks, configuration);
        if (new Engine().Adapter(configuration) is IEmulatorSettings settings)
            blocks.AddRange(settings.DescribeSettings(configuration).Where(block => block.Tab != EmulationMachineTab.Storage));
        blocks.Add(Block(EmulationMachineTab.Rom, SettingsConstants.ResourceFirmware,
            FirmwareFunctions.Roles(configuration.Model).Select(role => new EmulationSettingsField(
                FirmwareFunctions.Field(role), EmulationMachineTab.Rom, SettingsConstants.ResourceFirmware,
                "Emulation.Coleco.Firmware." + role, EmulationSettingsEditor.Path,
                configuration.Firmwares.SingleOrDefault(item => item.Category == role)?.Path)).ToArray()));
        return blocks;
    }

    private static void AddHardware(List<EmulationSettingsBlock> blocks, MachineConfiguration configuration)
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        blocks.Add(Block(EmulationMachineTab.Cpu, SettingsConstants.ResourceCpuProcessor,
            new(SettingsConstants.CpuModel, EmulationMachineTab.Cpu, SettingsConstants.ResourceCpuProcessor,
                SettingsConstants.ResourceCpuModel, EmulationSettingsEditor.Information, hardware.CpuResourceKey,
                [new(hardware.CpuResourceKey, hardware.CpuResourceKey)]),
            Information(SettingsConstants.CpuFrequency, EmulationMachineTab.Cpu,
                SettingsConstants.ResourceCpuProcessor, SettingsConstants.ResourceCpuSpeedOriginal,
                hardware.CpuFrequencyHz >= SettingsConstants.HertzPerMegahertz
                    ? $"{(hardware.CpuFrequencyHz / SettingsConstants.HertzPerMegahertz).ToString("0.######", CultureInfo.CurrentCulture)} {SettingsConstants.MegahertzUnit}"
                    : $"{(hardware.CpuFrequencyHz / SettingsConstants.HertzPerKilohertz).ToString("0.###", CultureInfo.CurrentCulture)} {SettingsConstants.KilohertzUnit}")));
        var memoryFields = new List<EmulationSettingsField>
        {
            Information(SettingsConstants.MainMemory, EmulationMachineTab.Ram, SettingsConstants.ResourceMemoryMain, SettingsConstants.ResourceMemoryMain, Memory(hardware.MainMemoryBytes))
        };
        if (hardware.DisplayMemoryBytes > 0)
            memoryFields.Add(Information(SettingsConstants.DisplayMemory, EmulationMachineTab.Ram, SettingsConstants.ResourceMemoryMain, SettingsConstants.ResourceDisplayMemory, Memory(hardware.DisplayMemoryBytes)));
        blocks.Add(Block(EmulationMachineTab.Ram, SettingsConstants.ResourceMemoryMain, memoryFields.ToArray()));
        blocks.Add(Block(EmulationMachineTab.Video, SettingsConstants.ResourceVideoResolution,
            Information(SettingsConstants.DisplayResolution, EmulationMachineTab.Video, SettingsConstants.ResourceVideoResolution, SettingsConstants.ResourceVideoResolution, $"{hardware.DisplayWidth} × {hardware.DisplayHeight}")));
    }

    private static string Memory(long bytes) => bytes >= SettingsConstants.BytesPerKibibyte ? $"{(bytes / SettingsConstants.BytesPerKibibyte).ToString(CultureInfo.InvariantCulture)} {SettingsConstants.KibibyteUnit}" : $"{bytes.ToString(CultureInfo.InvariantCulture)} {SettingsConstants.ByteUnit}";

    private static EmulationSettingsField Information(string id, EmulationMachineTab tab, string block, string label, string value) => new(id, tab, block, label, EmulationSettingsEditor.Information, value);

    internal static EmulationSettingsField Native(MachineConfiguration configuration, CoreOption option, EmulationMachineTab tab, string blockId)
    {
        var resource = option.ResourceKey ?? throw new InvalidOperationException(option.Key);
        var choices = option.Values.Select(choice => Choice(choice, option.Key)).ToArray();
        var toggle = option.Values.Count == 2 && option.Values.All(choice => choice.Value is "Enabled" or "Disabled" or "ON" or "OFF" or "on" or "off" or "enabled" or "disabled");
        return new(option.Key, tab, blockId, resource,
            toggle ? EmulationSettingsEditor.Toggle : EmulationSettingsEditor.Selection,
            configuration.Options.GetValueOrDefault(option.Key, option.DefaultValue), choices,
            ExplanationResourceKey: option.ExplanationResourceKey,
            EnabledValue: toggle ? option.Values.First(choice => choice.Value is "Enabled" or "ON" or "on" or "enabled").Value : EmulationSettingConstants.Enabled,
            DisabledValue: toggle ? option.Values.First(choice => choice.Value is "Disabled" or "OFF" or "off" or "disabled").Value : EmulationSettingConstants.Disabled,
            RequiresRestart: false);
    }
    private static EmulationSettingsChoice Choice(CoreOptionValue choice, string optionKey)
    {
        if (choice.InvariantDisplayValue is { } invariant) return new(choice.Value, string.Empty, invariant);
        if (choice.VideoStandard is { } standard && choice.FrequencyHz is { } frequencyHz) return new(choice.Value, string.Empty, EmulationVideoFunctions.FormatTiming(standard, frequencyHz));
        if (choice.VideoStandard is not null || choice.FrequencyHz is not null) throw new InvalidOperationException(optionKey + ":" + choice.Value);
        return new(choice.Value, choice.ResourceKey ?? throw new InvalidOperationException(optionKey + ":" + choice.Value), double.TryParse(choice.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out _) ? choice.Value : null);
    }
    private static EmulationSettingsBlock Audio(MachineConfiguration configuration) =>
        Block(EmulationMachineTab.Audio, SettingsConstants.ResourceAudio,
            new(SettingsConstants.AudioEnabled, EmulationMachineTab.Audio, SettingsConstants.ResourceAudio,
                SettingsConstants.ResourceAudioEnabled, EmulationSettingsEditor.Toggle,
                configuration.AudioEnabled ? EmulationSettingConstants.Enabled : EmulationSettingConstants.Disabled),
            new(SettingsConstants.AudioOutput, EmulationMachineTab.Audio, SettingsConstants.ResourceAudio,
                SettingsConstants.ResourceAudioOutput, EmulationSettingsEditor.Selection,
                configuration.Options.GetValueOrDefault(SettingsConstants.AudioOutput, SettingsConstants.DefaultOutput),
                [new(SettingsConstants.DefaultOutput, SettingsConstants.ResourceDefaultOutput)],
                ChoiceSource: EmulationSettingsChoiceSource.AudioOutputDevices),
            AudioSelection(configuration, SettingsConstants.AudioLatency, SettingsConstants.ResourceAudioLatency, SettingsConstants.DefaultLatency, SettingsConstants.Latencies, SettingsConstants.MillisecondsUnit),
            AudioSelection(configuration, VideoAudioSettingsConstants.AudioVolumeOption, SettingsConstants.ResourceAudioVolume, SettingsConstants.DefaultVolume, SettingsConstants.Volumes, SettingsConstants.PercentUnit));

    private static EmulationSettingsField AudioSelection(MachineConfiguration configuration, string id, string resource, int fallback, IReadOnlyList<int> values, string unit) =>
        new(id, EmulationMachineTab.Audio, SettingsConstants.ResourceAudio, resource,
            EmulationSettingsEditor.Selection, configuration.Options.GetValueOrDefault(id, fallback.ToString(CultureInfo.InvariantCulture)),
            values.Select(value => new EmulationSettingsChoice(value.ToString(CultureInfo.InvariantCulture), string.Empty, $"{value.ToString(CultureInfo.InvariantCulture)} {unit}")).ToArray());

    private static EmulationSettingsBlock Block(EmulationMachineTab tab, string resource, params EmulationSettingsField[] fields) => new(resource, tab, resource, fields);
}

