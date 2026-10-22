using CommonCoreHostFunctions = GWGUI.Emulation.Coleco.Common.Functions.CoreHostFunctions;
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
        CommonCoreHostFunctions.HardwareSettings.AddBlocks(blocks, "coleco", hardware.CpuResourceKey,
            () => CommonCoreHostFunctions.HardwareSettings.FormatFrequency(hardware.CpuFrequencyHz, useKilohertz: true),
            () => CommonCoreHostFunctions.HardwareSettings.FormatMemory(hardware.MainMemoryBytes),
            () => hardware.DisplayMemoryBytes > 0 ? CommonCoreHostFunctions.HardwareSettings.FormatMemory(hardware.DisplayMemoryBytes) : null,
            SettingsConstants.ResourceDisplayMemory, () => $"{hardware.DisplayWidth} × {hardware.DisplayHeight}",
            isCompactCpu: false);
    }

    internal static EmulationSettingsField Native(MachineConfiguration configuration, CoreOption option, EmulationMachineTab tab, string blockId)
    => CommonCoreHostFunctions.OptionProcessing.CreateSettingsFieldWithToggle(option.Key, tab, blockId,
        () => option.ResourceKey ?? throw new InvalidOperationException(option.Key), option.Values,
        choice => Choice(choice, option.Key),
        () => option.Values.Count == 2 && option.Values.All(choice => choice.Value is "Enabled" or "Disabled" or "ON" or "OFF" or "on" or "off" or "enabled" or "disabled"),
        () => configuration.Options.GetValueOrDefault(option.Key, option.DefaultValue), () => option.ExplanationResourceKey,
        toggle => toggle ? option.Values.First(choice => choice.Value is "Enabled" or "ON" or "on" or "enabled").Value : EmulationSettingConstants.Enabled,
        toggle => toggle ? option.Values.First(choice => choice.Value is "Disabled" or "OFF" or "off" or "disabled").Value : EmulationSettingConstants.Disabled);
    private static EmulationSettingsChoice Choice(CoreOptionValue choice, string optionKey)
    {
        if (choice.InvariantDisplayValue is { } invariant) return new(choice.Value, string.Empty, invariant);
        if (choice.VideoStandard is { } standard && choice.FrequencyHz is { } frequencyHz) return new(choice.Value, string.Empty, CommonCoreHostFunctions.VideoProcessing.FormatTiming(standard, frequencyHz));
        if (choice.VideoStandard is not null || choice.FrequencyHz is not null) throw new InvalidOperationException(optionKey + ":" + choice.Value);
        return new(choice.Value, choice.ResourceKey ?? throw new InvalidOperationException(optionKey + ":" + choice.Value), double.TryParse(choice.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out _) ? choice.Value : null);
    }
    private static EmulationSettingsBlock Audio(MachineConfiguration configuration) =>
        Block(EmulationMachineTab.Audio, SettingsConstants.ResourceAudio,
            CommonCoreHostFunctions.HardwareSettings.Toggle(SettingsConstants.AudioEnabled, EmulationMachineTab.Audio, SettingsConstants.ResourceAudio, SettingsConstants.ResourceAudioEnabled, configuration.AudioEnabled, null, null),
            CommonCoreHostFunctions.HardwareSettings.AudioOutput(SettingsConstants.AudioOutput, SettingsConstants.ResourceAudio, SettingsConstants.ResourceAudioOutput, configuration.Options.GetValueOrDefault(SettingsConstants.AudioOutput, SettingsConstants.DefaultOutput), [new(SettingsConstants.DefaultOutput, SettingsConstants.ResourceDefaultOutput)], null, null),
            AudioSelection(configuration, SettingsConstants.AudioLatency, SettingsConstants.ResourceAudioLatency, EmulationSettingConstants.DefaultAudioLatencyMilliseconds, EmulationSettingConstants.AudioLatencyMillisecondsChoices, SettingsConstants.MillisecondsUnit),
            AudioSelection(configuration, VideoAudioSettingsConstants.AudioVolumeOption, SettingsConstants.ResourceAudioVolume, EmulationSettingConstants.DefaultAudioVolumePercent, EmulationSettingConstants.AudioVolumePercentChoices, SettingsConstants.PercentUnit));

    private static EmulationSettingsField AudioSelection(MachineConfiguration configuration, string id, string resource, int fallback, IReadOnlyList<int> values, string unit) =>
        CommonCoreHostFunctions.HardwareSettings.AudioSelection(configuration.Options, id, SettingsConstants.ResourceAudio, resource, fallback, values, unit, CultureInfo.InvariantCulture);

    private static EmulationSettingsBlock Block(EmulationMachineTab tab, string resource, params EmulationSettingsField[] fields) => new(resource, tab, resource, fields);
}

