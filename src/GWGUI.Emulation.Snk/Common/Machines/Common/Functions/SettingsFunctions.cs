using CommonCoreHostFunctions = GWGUI.Emulation.Snk.Common.Functions.CoreHostFunctions;
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
        CommonCoreHostFunctions.HardwareSettings.AddBlocks(blocks, "snk", hardware.CpuResourceKey,
            () => hardware.CpuFrequencyHz is long frequency ? CommonCoreHostFunctions.HardwareSettings.FormatFrequency(frequency) : null,
            () => hardware.MainMemoryBytes is long main ? CommonCoreHostFunctions.HardwareSettings.FormatMemory(main) : null,
            () => hardware.DisplayMemoryBytes is long display ? CommonCoreHostFunctions.HardwareSettings.FormatMemory(display) : null,
            SettingsConstants.ResourceDisplayMemory,
            () =>
            {
                if (hardware.DisplayWidth is int width && hardware.DisplayHeight is int height)
                {
                    return $"{width} × {height}";
                }
                return null;
            },
            isCompactCpu: true);
    }
    private static EmulationSettingsField Native(MachineConfiguration configuration, CoreOption option, EmulationMachineTab tab, string block)
    => CommonCoreHostFunctions.OptionProcessing.CreateSettingsField(option.Key, tab, block,
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
        CommonCoreHostFunctions.HardwareSettings.Toggle(SettingsConstants.AudioEnabled, EmulationMachineTab.Audio, SettingsConstants.ResourceAudio, SettingsConstants.ResourceAudioEnabled, configuration.AudioEnabled, null, null),
        CommonCoreHostFunctions.HardwareSettings.AudioOutput(SettingsConstants.AudioOutput, SettingsConstants.ResourceAudio, SettingsConstants.ResourceAudioOutput, configuration.Options.GetValueOrDefault(SettingsConstants.AudioOutput, SettingsConstants.DefaultOutput), [new(SettingsConstants.DefaultOutput, SettingsConstants.ResourceDefaultOutput)], null, null),
        AudioSelection(configuration, SettingsConstants.AudioLatency, SettingsConstants.ResourceAudioLatency, EmulationSettingConstants.DefaultAudioLatencyMilliseconds, EmulationSettingConstants.AudioLatencyMillisecondsChoices, SettingsConstants.MillisecondsUnit),
        AudioSelection(configuration, VideoAudioSettingsConstants.AudioVolumeOption, SettingsConstants.ResourceAudioVolume, EmulationSettingConstants.DefaultAudioVolumePercent, EmulationSettingConstants.AudioVolumePercentChoices, SettingsConstants.PercentUnit)
    );
    private static EmulationSettingsField AudioSelection(MachineConfiguration configuration, string id, string resource, int fallback, IReadOnlyList<int> values, string unit) => CommonCoreHostFunctions.HardwareSettings.AudioSelection(configuration.Options, id, SettingsConstants.ResourceAudio, resource, fallback, values, unit, CultureInfo.InvariantCulture);
    private static EmulationSettingsBlock Block(EmulationMachineTab tab, string resource, params EmulationSettingsField[] fields) => new(resource, tab, resource, fields);
}