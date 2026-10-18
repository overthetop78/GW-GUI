using System.Globalization;

namespace GWGUI.Emulation.Fairchild.Common.Machines.Common.Functions;

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
            "Emulation.Fairchild.Firmware." + role, EmulationSettingsEditor.Path,
            configuration.Firmwares.SingleOrDefault(item => item.Category == role)?.Path)).ToArray();
        if (firmwareFields.Length > 0)
            blocks.Add(new(SettingsConstants.ResourceFirmware + ".File", EmulationMachineTab.Rom, SettingsConstants.ResourceFirmware, firmwareFields));
        return blocks;
    }

    private static void AddHardware(List<EmulationSettingsBlock> blocks, MachineConfiguration configuration)
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        EmulationHardwareSettingsFunctions.AddBlocks(blocks, "fairchild", hardware.CpuResourceKey,
            () => hardware.CpuFrequencyHz is long frequency ? EmulationHardwareSettingsFunctions.FormatFrequency(frequency) : null,
            () => hardware.MainMemoryBytes is long main ? EmulationHardwareSettingsFunctions.FormatMemory(main) : null,
            () => hardware.DisplayMemoryBytes is long display ? EmulationHardwareSettingsFunctions.FormatMemory(display) : null,
            SettingsConstants.ResourceDisplayMemory, () => $"{hardware.DisplayWidth} × {hardware.DisplayHeight}",
            isCompactCpu: hardware.CpuFrequencyHz is null);
    }

    internal static EmulationSettingsField Native(MachineConfiguration configuration, CoreOption option, EmulationMachineTab tab, string block)
    {
        var resource = option.ResourceKey ?? throw new InvalidOperationException($"Missing common resource for {option.Key}.");
        var choices = option.Values.Select(choice => new EmulationSettingsChoice(choice.Value, choice.ResourceKey ?? throw new InvalidOperationException($"Missing common choice resource for {option.Key}: {choice.Value}."))).ToArray();
        var toggle = option.Values.Count == 2 && option.Values.All(choice => choice.Value is "enabled" or "disabled");
        return new(option.Key, tab, block, resource, toggle ? EmulationSettingsEditor.Toggle : EmulationSettingsEditor.Selection,
            configuration.Options.GetValueOrDefault(option.Key, option.DefaultValue), choices,
            ExplanationResourceKey: option.ExplanationResourceKey,
            EnabledValue: toggle ? "enabled" : EmulationSettingConstants.Enabled,
            DisabledValue: toggle ? "disabled" : EmulationSettingConstants.Disabled,
            RequiresRestart: false);
    }

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
