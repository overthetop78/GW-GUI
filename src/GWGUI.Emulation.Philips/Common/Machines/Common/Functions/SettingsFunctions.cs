using System.Globalization;

namespace GWGUI.Emulation.Philips.Common.Machines.Common.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var blocks = new List<EmulationSettingsBlock> { Audio(configuration) };
        AddHardware(blocks, configuration);
        if (new Engine().Adapter(configuration) is IEmulatorSettings settings)
            blocks.AddRange(settings.DescribeSettings(configuration));
        var firmwareFields = FirmwareFunctions.Roles(configuration.Model).Select(role => new EmulationSettingsField(
            FirmwareFunctions.Field(role), EmulationMachineTab.Rom, SettingsConstants.ResourceFirmware,
            "Emulation.Philips.Firmware." + role, EmulationSettingsEditor.Path,
            configuration.Firmwares.SingleOrDefault(item => item.Category == role)?.Path)).ToArray();
        if (firmwareFields.Length > 0)
            blocks.Add(new(SettingsConstants.ResourceFirmware + ".File", EmulationMachineTab.Rom, SettingsConstants.ResourceFirmware, firmwareFields));
        return blocks;
    }

    private static void AddHardware(List<EmulationSettingsBlock> blocks, MachineConfiguration configuration)
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        EmulationHardwareSettingsFunctions.AddBlocks(blocks, "philips", hardware.CpuResourceKey,
            () => hardware.CpuFrequencyHz is long frequency ? EmulationHardwareSettingsFunctions.FormatFrequency(frequency) : null,
            () => hardware.MainMemoryBytes is long main ? EmulationHardwareSettingsFunctions.FormatMemory(main) : null,
            () => hardware.DisplayMemoryBytes is long display ? EmulationHardwareSettingsFunctions.FormatMemory(display) : null,
            SettingsConstants.ResourceDisplayMemory,
            () =>
            {
                if (hardware.DisplayWidth is int width && hardware.DisplayHeight is int height)
                {
                    if (new Engine().Adapter(configuration) is IEmulatorVideoSettings video && video.GetOutputResolution(configuration) is { } resolution)
                    {
                        width = resolution.Width;
                        height = resolution.Height;
                    }
                    return $"{width} × {height}";
                }
                return null;
            },
            isCompactCpu: hardware.CpuFrequencyHz is null);
    }

    internal static EmulationSettingsField Native(MachineConfiguration configuration, CoreOption option, EmulationMachineTab tab, string block)
    => EmulationCoreOptionFunctions.CreateSettingsFieldWithToggle(option.Key, tab, block,
        () => option.ResourceKey ?? throw new InvalidOperationException(option.Key), option.Values,
        choice => new EmulationSettingsChoice(choice.Value, choice.ResourceKey ?? string.Empty, InvariantChoice(choice)),
        () => option.Values.Count == 2 && option.Values.All(choice => choice.Value is "enabled" or "disabled"),
        () => configuration.Options.GetValueOrDefault(option.Key, option.DefaultValue), () => option.ExplanationResourceKey,
        toggle => toggle ? "enabled" : EmulationSettingConstants.Enabled,
        toggle => toggle ? "disabled" : EmulationSettingConstants.Disabled);

    internal static string ChoiceLabel(CoreOptionValue choice) => choice.ResourceKey ?? InvariantChoice(choice) ?? choice.Label;

    private static string? InvariantChoice(CoreOptionValue choice) => choice.VideoStandard is { } standard && choice.FrequencyHz is { } frequency ? EmulationVideoFunctions.FormatTiming(standard, frequency) : choice.InvariantDisplayValue;

    private static EmulationSettingsBlock Audio(MachineConfiguration configuration) => Block(EmulationMachineTab.Audio,
        SettingsConstants.ResourceAudio,
        new(SettingsConstants.AudioEnabled, EmulationMachineTab.Audio, SettingsConstants.ResourceAudio,
            SettingsConstants.ResourceAudioEnabled, EmulationSettingsEditor.Toggle,
            configuration.AudioEnabled ? EmulationSettingConstants.Enabled : EmulationSettingConstants.Disabled),
        new(SettingsConstants.AudioOutput, EmulationMachineTab.Audio, SettingsConstants.ResourceAudio,
            SettingsConstants.ResourceAudioOutput, EmulationSettingsEditor.Selection,
            configuration.Options.GetValueOrDefault(SettingsConstants.AudioOutput, SettingsConstants.DefaultOutput),
            [new(SettingsConstants.DefaultOutput, SettingsConstants.ResourceDefaultOutput)], ChoiceSource: EmulationSettingsChoiceSource.AudioOutputDevices),
        AudioSelection(configuration, SettingsConstants.AudioLatency, SettingsConstants.ResourceAudioLatency, EmulationSettingConstants.DefaultAudioLatencyMilliseconds, EmulationSettingConstants.AudioLatencyMillisecondsChoices, SettingsConstants.MillisecondsUnit),
        AudioSelection(configuration, VideoAudioSettingsConstants.AudioVolumeOption, SettingsConstants.ResourceAudioVolume, EmulationSettingConstants.DefaultAudioVolumePercent, EmulationSettingConstants.AudioVolumePercentChoices, SettingsConstants.PercentUnit));

    private static EmulationSettingsField AudioSelection(MachineConfiguration configuration, string id, string resource, int fallback, IReadOnlyList<int> values, string unit) => new(id, EmulationMachineTab.Audio, SettingsConstants.ResourceAudio,
            resource, EmulationSettingsEditor.Selection, configuration.Options.GetValueOrDefault(id, fallback.ToString(CultureInfo.InvariantCulture)),
            values.Select(value => new EmulationSettingsChoice(value.ToString(CultureInfo.InvariantCulture), string.Empty, $"{value.ToString(CultureInfo.InvariantCulture)} {unit}")).ToArray());

    private static EmulationSettingsBlock Block(EmulationMachineTab tab, string resource, params EmulationSettingsField[] fields) => new(resource, tab, resource, fields);
}
