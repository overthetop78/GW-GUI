using System.Globalization;

namespace GWGUI.Emulation.Snk.Common.Machines.Common.Functions;
internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var blocks = new List<EmulationSettingsBlock>
        {
            Audio(configuration)
        };
        AddHardware(blocks, configuration);
        foreach (var group in RelevantOptions(configuration).GroupBy(option => Enum.Parse<EmulationMachineTab>(option.Category!)))
        {
            var resource = group.Key switch
            {
                EmulationMachineTab.Controllers => SettingsConstants.ResourceNativeControls,
                EmulationMachineTab.Audio => SettingsConstants.ResourceNativeAudio,
                EmulationMachineTab.Cpu => SettingsConstants.ResourceCpuAcceleration,
                EmulationMachineTab.Ram => SettingsConstants.ResourceMemoryMain,
                EmulationMachineTab.General => SettingsConstants.ResourceNativeSystem,
                EmulationMachineTab.Storage => SettingsConstants.ResourceNativeStorage,
                EmulationMachineTab.Rom => SettingsConstants.ResourceFirmware,
                _ => SettingsConstants.ResourceNativeVideo
            };
            var blockId = resource + ".Options." + group.Key;
            blocks.Add(new(blockId, group.Key, resource, group.Select(option => Native(configuration, option, group.Key, blockId)).ToArray(), IsCompact: group.Key == EmulationMachineTab.Audio));
        }

        var firmwareFields = FirmwareFunctions.Roles(configuration.Model, configuration.Core).Select(role => new EmulationSettingsField(
                FirmwareFunctions.Field(role),
                EmulationMachineTab.Rom,
                SettingsConstants.ResourceFirmware,
                "Emulation.Snk.Firmware." + role,
                EmulationSettingsEditor.Path,
                configuration.Firmwares.SingleOrDefault(item => item.Category == role)?.Path
            )).ToArray();
        if (firmwareFields.Length > 0)
            blocks.Add(new(SettingsConstants.ResourceFirmware + ".File", EmulationMachineTab.Rom, SettingsConstants.ResourceFirmware, firmwareFields));
        return blocks;
    }

    internal static IReadOnlyList<CoreOption> RelevantOptions(MachineConfiguration configuration) => CoreOptionValidationFunctions.ForConfiguration(configuration).Where(option => option.IsVisible && option.Category is not null).ToArray();
    private static void AddHardware(List<EmulationSettingsBlock> blocks, MachineConfiguration configuration)
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        var cpu = new List<EmulationSettingsField>
        {
            new(
                SettingsConstants.CpuModel,
                EmulationMachineTab.Cpu,
                SettingsConstants.ResourceCpuProcessor,
                SettingsConstants.ResourceCpuModel,
                EmulationSettingsEditor.Information,
                hardware.CpuResourceKey,
                [new(hardware.CpuResourceKey, hardware.CpuResourceKey)]
            )
        };
        if (hardware.CpuFrequencyHz is long frequency)
            cpu.Add(Information(
                    SettingsConstants.CpuFrequency,
                    EmulationMachineTab.Cpu,
                    SettingsConstants.ResourceCpuProcessor,
                    SettingsConstants.ResourceCpuSpeedOriginal,
                    $"{(frequency / SettingsConstants.HertzPerMegahertz).ToString("0.######", CultureInfo.CurrentCulture)} {SettingsConstants.MegahertzUnit}"
                ));
        blocks.Add(Block(EmulationMachineTab.Cpu, SettingsConstants.ResourceCpuProcessor, cpu.ToArray())with { IsCompact = true });
        var memory = new List<EmulationSettingsField>();
        if (hardware.MainMemoryBytes is long main)
            memory.Add(Information(SettingsConstants.MainMemory, EmulationMachineTab.Ram, SettingsConstants.ResourceMemoryMain, SettingsConstants.ResourceMemoryMain, Memory(main)));
        if (hardware.DisplayMemoryBytes is long display)
            memory.Add(Information(SettingsConstants.DisplayMemory, EmulationMachineTab.Ram, SettingsConstants.ResourceMemoryMain, SettingsConstants.ResourceDisplayMemory, Memory(display)));
        if (memory.Count > 0)
            blocks.Add(Block(EmulationMachineTab.Ram, SettingsConstants.ResourceMemoryMain, memory.ToArray()));
        if (hardware.DisplayWidth is int width && hardware.DisplayHeight is int height)
            blocks.Add(Block(
                    EmulationMachineTab.Video,
                    SettingsConstants.ResourceVideoResolution,
                    Information(SettingsConstants.DisplayResolution, EmulationMachineTab.Video, SettingsConstants.ResourceVideoResolution, SettingsConstants.ResourceVideoResolution, $"{width} × {height}")
                ));
    }

    private static string Memory(long bytes) => bytes >= SettingsConstants.BytesPerKibibyte ? $"{(bytes / SettingsConstants.BytesPerKibibyte).ToString(CultureInfo.InvariantCulture)} {SettingsConstants.KibibyteUnit}" : $"{bytes.ToString(CultureInfo.InvariantCulture)} {SettingsConstants.ByteUnit}";
    private static EmulationSettingsField Information(string id, EmulationMachineTab tab, string block, string label, string value) => new(id, tab, block, label, EmulationSettingsEditor.Information, value);
    private static EmulationSettingsField Native(MachineConfiguration configuration, CoreOption option, EmulationMachineTab tab, string block)
    => EmulationCoreOptionFunctions.CreateSettingsField(option.Key, tab, block,
        () => option.ResourceKey ?? throw new InvalidOperationException($"Missing common resource for {option.Key}."), option.Values,
        choice => new EmulationSettingsChoice(
                choice.Value,
                choice.FrameSkipRatio is null ? CommonChoiceResource(choice.Value) ?? choice.ResourceKey ?? throw new InvalidOperationException($"Missing common choice resource for {option.Key}: {choice.Value}.") : string.Empty,
                choice.FrameSkipRatio is FrameSkipRatio ratio ? SettingsChoiceFunctions.Format(ratio) : null
            ),
        choice => CommonChoiceResource(choice.Value) == SettingsConstants.ResourceEnabled,
        choice => CommonChoiceResource(choice.Value) == SettingsConstants.ResourceDisabled,
        static choice => choice.Value,
        () => configuration.Options.GetValueOrDefault(option.Key, option.DefaultValue),
        () => option.ExplanationResourceKey, () => option.IsEnabled);

    private static string? CommonChoiceResource(string value) => value.ToLowerInvariant() switch
    {
        "enabled" or "on" => SettingsConstants.ResourceEnabled,
        "disabled" or "off" => SettingsConstants.ResourceDisabled,
        "default" => SettingsConstants.ResourceDefault,
        "auto" => SettingsConstants.ResourceAutomatic,
        "none" => SettingsConstants.ResourceNone,
        _ => null
    };
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
    private static EmulationSettingsBlock Block(EmulationMachineTab tab, string resource, params EmulationSettingsField[] fields) => new(resource, tab, resource, fields);
}