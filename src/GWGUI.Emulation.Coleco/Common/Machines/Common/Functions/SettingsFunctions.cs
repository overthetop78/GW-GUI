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
        EmulationHardwareSettingsFunctions.AddBlocks(blocks, "coleco", hardware.CpuResourceKey,
            () => EmulationHardwareSettingsFunctions.FormatFrequency(hardware.CpuFrequencyHz, useKilohertz: true),
            () => EmulationHardwareSettingsFunctions.FormatMemory(hardware.MainMemoryBytes),
            () => hardware.DisplayMemoryBytes > 0 ? EmulationHardwareSettingsFunctions.FormatMemory(hardware.DisplayMemoryBytes) : null,
            SettingsConstants.ResourceDisplayMemory, () => $"{hardware.DisplayWidth} × {hardware.DisplayHeight}",
            isCompactCpu: false);
    }

    internal static EmulationSettingsField Native(MachineConfiguration configuration, CoreOption option, EmulationMachineTab tab, string blockId)
    => EmulationCoreOptionFunctions.CreateSettingsFieldWithToggle(option.Key, tab, blockId,
        () => option.ResourceKey ?? throw new InvalidOperationException(option.Key), option.Values,
        choice => Choice(choice, option.Key),
        () => option.Values.Count == 2 && option.Values.All(choice => choice.Value is "Enabled" or "Disabled" or "ON" or "OFF" or "on" or "off" or "enabled" or "disabled"),
        () => configuration.Options.GetValueOrDefault(option.Key, option.DefaultValue), () => option.ExplanationResourceKey,
        toggle => toggle ? option.Values.First(choice => choice.Value is "Enabled" or "ON" or "on" or "enabled").Value : EmulationSettingConstants.Enabled,
        toggle => toggle ? option.Values.First(choice => choice.Value is "Disabled" or "OFF" or "off" or "disabled").Value : EmulationSettingConstants.Disabled);
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
            AudioSelection(configuration, SettingsConstants.AudioLatency, SettingsConstants.ResourceAudioLatency, EmulationSettingConstants.DefaultAudioLatencyMilliseconds, EmulationSettingConstants.AudioLatencyMillisecondsChoices, SettingsConstants.MillisecondsUnit),
            AudioSelection(configuration, VideoAudioSettingsConstants.AudioVolumeOption, SettingsConstants.ResourceAudioVolume, EmulationSettingConstants.DefaultAudioVolumePercent, EmulationSettingConstants.AudioVolumePercentChoices, SettingsConstants.PercentUnit));

    private static EmulationSettingsField AudioSelection(MachineConfiguration configuration, string id, string resource, int fallback, IReadOnlyList<int> values, string unit) =>
        new(id, EmulationMachineTab.Audio, SettingsConstants.ResourceAudio, resource,
            EmulationSettingsEditor.Selection, configuration.Options.GetValueOrDefault(id, fallback.ToString(CultureInfo.InvariantCulture)),
            values.Select(value => new EmulationSettingsChoice(value.ToString(CultureInfo.InvariantCulture), string.Empty, $"{value.ToString(CultureInfo.InvariantCulture)} {unit}")).ToArray());

    private static EmulationSettingsBlock Block(EmulationMachineTab tab, string resource, params EmulationSettingsField[] fields) => new(resource, tab, resource, fields);
}

