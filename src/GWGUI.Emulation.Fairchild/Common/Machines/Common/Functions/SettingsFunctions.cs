using CommonCoreHostFunctions = GWGUI.Emulation.Fairchild.Common.Functions.CoreHostFunctions;
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
    => CommonCoreHostFunctions.OptionProcessing.CreateSettingsFieldWithToggle(option.Key, tab, block,
        () => option.ResourceKey ?? throw new InvalidOperationException($"Missing common resource for {option.Key}."), option.Values,
        choice => new EmulationSettingsChoice(choice.Value, choice.ResourceKey ?? throw new InvalidOperationException($"Missing common choice resource for {option.Key}: {choice.Value}.")),
        () => option.Values.Count == 2 && option.Values.All(choice => choice.Value is "enabled" or "disabled"),
        () => configuration.Options.GetValueOrDefault(option.Key, option.DefaultValue), () => option.ExplanationResourceKey,
        toggle => toggle ? "enabled" : EmulationSettingConstants.Enabled,
        toggle => toggle ? "disabled" : EmulationSettingConstants.Disabled);

    private static EmulationSettingsBlock Audio(MachineConfiguration configuration) => Block(EmulationMachineTab.Audio,
        SettingsConstants.ResourceAudio,
        EmulationHardwareSettingsFunctions.Toggle(SettingsConstants.AudioEnabled, EmulationMachineTab.Audio, SettingsConstants.ResourceAudio, SettingsConstants.ResourceAudioEnabled, configuration.AudioEnabled, null, null),
        EmulationHardwareSettingsFunctions.AudioOutput(SettingsConstants.AudioOutput, SettingsConstants.ResourceAudio, SettingsConstants.ResourceAudioOutput, configuration.Options.GetValueOrDefault(SettingsConstants.AudioOutput, SettingsConstants.DefaultOutput), [new(SettingsConstants.DefaultOutput, SettingsConstants.ResourceDefaultOutput)], null, null),
        AudioSelection(configuration, SettingsConstants.AudioLatency, SettingsConstants.ResourceAudioLatency, EmulationSettingConstants.DefaultAudioLatencyMilliseconds, EmulationSettingConstants.AudioLatencyMillisecondsChoices, SettingsConstants.MillisecondsUnit),
        AudioSelection(configuration, VideoAudioSettingsConstants.AudioVolumeOption, SettingsConstants.ResourceAudioVolume, EmulationSettingConstants.DefaultAudioVolumePercent, EmulationSettingConstants.AudioVolumePercentChoices, SettingsConstants.PercentUnit));

    private static EmulationSettingsField AudioSelection(MachineConfiguration configuration, string id, string resource, int fallback, IReadOnlyList<int> values, string unit) => EmulationHardwareSettingsFunctions.AudioSelection(configuration.Options, id, SettingsConstants.ResourceAudio, resource, fallback, values, unit, CultureInfo.InvariantCulture);

    private static EmulationSettingsBlock Block(EmulationMachineTab tab, string resource, params EmulationSettingsField[] fields) => new(resource, tab, resource, fields);
}
