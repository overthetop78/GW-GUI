using CommonCoreHostFunctions = GWGUI.Emulation.Bandai.Common.Functions.CoreHostFunctions;
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
        EmulationHardwareSettingsFunctions.AddBlocks(blocks, "bandai", hardware.CpuResourceKey,
            () => EmulationHardwareSettingsFunctions.FormatFrequency(hardware.CpuFrequencyHz, useKilohertz: true),
            () => EmulationHardwareSettingsFunctions.FormatMemory(hardware.MainMemoryBytes),
            () => hardware.DisplayMemoryBytes > 0 ? EmulationHardwareSettingsFunctions.FormatMemory(hardware.DisplayMemoryBytes) : null,
            SettingsConstants.ResourceDisplayMemory, () => $"{hardware.DisplayWidth} × {hardware.DisplayHeight}",
            isCompactCpu: false);
    }
    internal static EmulationSettingsField Information(string id, EmulationMachineTab tab, string block, string label, string value) => new(id, tab, block, label, EmulationSettingsEditor.Information, value);
    internal static EmulationSettingsField Native(MachineConfiguration configuration, CoreOption option, EmulationMachineTab tab, string blockId)
    => CommonCoreHostFunctions.OptionProcessing.CreateSettingsFieldWithToggle(option.Key, tab, blockId,
        () => option.ResourceKey ?? throw new InvalidOperationException($"Missing common resource for {option.Key}."), option.Values,
        choice => new EmulationSettingsChoice(
                choice.Value,
                choice.ResourceKey ?? (choice.InvariantDisplayValue is not null ? string.Empty : throw new InvalidOperationException($"Missing common choice for {option.Key}/{choice.Value}.")),
                choice.InvariantDisplayValue
            ),
        () => false,
        () => configuration.Options.GetValueOrDefault(option.Key, option.DefaultValue), () => option.ExplanationResourceKey,
        toggle => EmulationSettingConstants.Enabled,
        toggle => EmulationSettingConstants.Disabled);

    private static EmulationSettingsBlock Audio(MachineConfiguration configuration) => Block(
        EmulationMachineTab.Audio,
        SettingsConstants.ResourceAudio,
        EmulationHardwareSettingsFunctions.Toggle(SettingsConstants.AudioEnabled, EmulationMachineTab.Audio, SettingsConstants.ResourceAudio, SettingsConstants.ResourceAudioEnabled, configuration.AudioEnabled, null, null),
        EmulationHardwareSettingsFunctions.AudioOutput(SettingsConstants.AudioOutput, SettingsConstants.ResourceAudio, SettingsConstants.ResourceAudioOutput, configuration.Options.GetValueOrDefault(SettingsConstants.AudioOutput, SettingsConstants.DefaultOutput), [new(SettingsConstants.DefaultOutput, SettingsConstants.ResourceDefaultOutput)], null, null),
        AudioSelection(configuration, SettingsConstants.AudioLatency, SettingsConstants.ResourceAudioLatency, EmulationSettingConstants.DefaultAudioLatencyMilliseconds, EmulationSettingConstants.AudioLatencyMillisecondsChoices, SettingsConstants.MillisecondsUnit),
        AudioSelection(configuration, VideoAudioSettingsConstants.AudioVolumeOption, SettingsConstants.ResourceAudioVolume, EmulationSettingConstants.DefaultAudioVolumePercent, EmulationSettingConstants.AudioVolumePercentChoices, SettingsConstants.PercentUnit)
    );
    private static EmulationSettingsField AudioSelection(MachineConfiguration configuration, string id, string resource, int fallback, IReadOnlyList<int> values, string unit) => EmulationHardwareSettingsFunctions.AudioSelection(configuration.Options, id, SettingsConstants.ResourceAudio, resource, fallback, values, unit, CultureInfo.InvariantCulture);
    internal static EmulationSettingsBlock Block(EmulationMachineTab tab, string resource, params EmulationSettingsField[] fields) => new(resource, tab, resource, fields);
}