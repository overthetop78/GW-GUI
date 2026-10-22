using CommonCoreHostFunctions = GWGUI.Emulation.Sharp.Common.Functions.CoreHostFunctions;
using System.Globalization;

namespace GWGUI.Emulation.Sharp.Common.Machines.Common.Functions;
internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<CoreOption> NativeOptions(MachineConfiguration? configuration, IReadOnlyList<CoreOption> options,
        Func<MachineModel, IReadOnlyDictionary<string, string>> defaults)
    {
        if (configuration is null) return options;
        return CommonCoreHostFunctions.OptionProcessing.WithDefaults(options, defaults(configuration.Model), configuration.Options,
            static option => option.Key, static option => option.DefaultValue,
            (option, defaultValue, currentValue) => option with
            {
                DefaultValue = defaultValue, CurrentValue = currentValue
            });
    }

    internal static IReadOnlyList<EmulationSettingsBlock> NativeBlocks(MachineConfiguration configuration, IReadOnlyList<CoreOption> options) =>
        CommonCoreHostFunctions.OptionProcessing.CreateSettingsBlocks(options, static option => option.IsVisible,
            static option => option.Category, NativeResource,
            (option, tab, resource) => Native(configuration, option, tab, resource));

    private static string NativeResource(EmulationMachineTab tab) => tab switch
                {
                    EmulationMachineTab.Controllers => SettingsConstants.ResourceNativeControls,
                    EmulationMachineTab.Audio => SettingsConstants.ResourceNativeAudio,
                    EmulationMachineTab.Cpu => SettingsConstants.ResourceCpuAcceleration,
                    EmulationMachineTab.Ram => SettingsConstants.ResourceMemoryMain,
                    _ => SettingsConstants.ResourceNativeVideo
                };

    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var blocks = new List<EmulationSettingsBlock>
        {
            Audio(configuration)
        };
        AddHardware(blocks, configuration);
        if (InputSettingsFunctions.SupportsMouse(configuration))
            blocks.Add(Block(
                    EmulationMachineTab.Mouse,
                    SettingsConstants.ResourceMouse,
                    new EmulationSettingsField(
                        SettingsConstants.MouseSpeed,
                        EmulationMachineTab.Mouse,
                        SettingsConstants.ResourceMouse,
                        SettingsConstants.ResourceMouseSpeed,
                        EmulationSettingsEditor.Selection,
                        configuration.Input.MouseSpeedPercent.ToString(CultureInfo.InvariantCulture),
                        Enumerable.Range(
                            InputMappingConstants.MinimumMouseSpeedPercent / InputMappingConstants.MouseSpeedStepPercent,
                            (InputMappingConstants.MaximumMouseSpeedPercent - InputMappingConstants.MinimumMouseSpeedPercent) / InputMappingConstants.MouseSpeedStepPercent + 1
                        ).Select(step => step * InputMappingConstants.MouseSpeedStepPercent).Select(value => new EmulationSettingsChoice(value.ToString(CultureInfo.InvariantCulture), string.Empty, $"{value} {SettingsConstants.PercentUnit}")).ToArray()
                    )
                ));
        if (configuration.Core != Emulator.None && new Engine().Adapter(configuration) is IEmulatorSettings settings)
            blocks.AddRange(settings.DescribeSettings(configuration));

        var firmwareFields = FirmwareFunctions.Roles(configuration.Model).Select(role => new EmulationSettingsField(
                FirmwareFunctions.Field(role),
                EmulationMachineTab.Rom,
                SettingsConstants.ResourceFirmware,
                "Emulation.Sharp.Firmware." + role,
                EmulationSettingsEditor.Path,
                configuration.Firmwares.SingleOrDefault(item => item.Category == role)?.Path
            )).ToArray();
        if (firmwareFields.Length > 0)
            blocks.Add(new(SettingsConstants.ResourceFirmware + ".File", EmulationMachineTab.Rom, SettingsConstants.ResourceFirmware, firmwareFields));
        return blocks;
    }

    private static void AddHardware(List<EmulationSettingsBlock> blocks, MachineConfiguration configuration)
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        EmulationHardwareSettingsFunctions.AddBlocks(blocks, "sharp", hardware.CpuResourceKey,
            () => hardware.CpuFrequencyHz is long frequency ? EmulationHardwareSettingsFunctions.FormatFrequency(frequency) : null,
            () => hardware.MainMemoryBytes is long main ? EmulationHardwareSettingsFunctions.FormatMemory(main) : null,
            () => hardware.DisplayMemoryBytes is long display ? EmulationHardwareSettingsFunctions.FormatMemory(display) : null,
            SettingsConstants.ResourceDisplayMemory,
            () =>
            {
                if (hardware.DisplayWidth is int width && hardware.DisplayHeight is int height)
                {
                    return $"{width} × {height}";
                }
                return null;
            },
            isCompactCpu: hardware.CpuFrequencyHz is null);
    }
    internal static EmulationSettingsField Native(MachineConfiguration configuration, CoreOption option, EmulationMachineTab tab, string block)
    => CommonCoreHostFunctions.OptionProcessing.CreateSettingsField(option.Key, tab, block,
        () => option.ResourceKey ?? throw new InvalidOperationException(option.Key), option.Values,
        choice => new EmulationSettingsChoice(choice.Value, choice.ResourceKey ?? string.Empty, choice.InvariantDisplayValue),
        choice => choice.ResourceKey == SettingsChoiceConstants.Enabled,
        choice => choice.ResourceKey == SettingsChoiceConstants.Disabled,
        static choice => choice.Value,
        () => configuration.Options.GetValueOrDefault(option.Key, option.DefaultValue),
        () => option.ExplanationResourceKey);

    internal static string ChoiceLabel(CoreOptionValue choice) => choice.ResourceKey ?? choice.InvariantDisplayValue ?? choice.Label;

    private static EmulationSettingsBlock Audio(MachineConfiguration configuration) => Block(
        EmulationMachineTab.Audio,
        SettingsConstants.ResourceAudio,
        EmulationHardwareSettingsFunctions.Toggle(SettingsConstants.AudioEnabled, EmulationMachineTab.Audio, SettingsConstants.ResourceAudio, SettingsConstants.ResourceAudioEnabled, configuration.AudioEnabled, null, null),
        EmulationHardwareSettingsFunctions.AudioOutput(SettingsConstants.AudioOutput, SettingsConstants.ResourceAudio, SettingsConstants.ResourceAudioOutput, configuration.Options.GetValueOrDefault(SettingsConstants.AudioOutput, SettingsConstants.DefaultOutput), [new(SettingsConstants.DefaultOutput, SettingsConstants.ResourceDefaultOutput)], null, null),
        AudioSelection(configuration, SettingsConstants.AudioLatency, SettingsConstants.ResourceAudioLatency, EmulationSettingConstants.DefaultAudioLatencyMilliseconds, EmulationSettingConstants.AudioLatencyMillisecondsChoices, SettingsConstants.MillisecondsUnit),
        AudioSelection(configuration, VideoAudioSettingsConstants.AudioVolumeOption, SettingsConstants.ResourceAudioVolume, EmulationSettingConstants.DefaultAudioVolumePercent, EmulationSettingConstants.AudioVolumePercentChoices, SettingsConstants.PercentUnit)
    );
    private static EmulationSettingsField AudioSelection(MachineConfiguration configuration, string id, string resource, int fallback, IReadOnlyList<int> values, string unit) => EmulationHardwareSettingsFunctions.AudioSelection(configuration.Options, id, SettingsConstants.ResourceAudio, resource, fallback, values, unit, CultureInfo.InvariantCulture);
    private static EmulationSettingsBlock Block(EmulationMachineTab tab, string resource, params EmulationSettingsField[] fields) => new(resource, tab, resource, fields);
}
