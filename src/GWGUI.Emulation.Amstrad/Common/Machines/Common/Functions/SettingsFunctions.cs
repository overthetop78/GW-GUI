using CrocoDSProfile = GWGUI.Emulation.Amstrad.Emulators.CrocoDS.Constants.EmulatorConstants;
namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration,
        IReadOnlyList<CoreOption>? coreOptions = null)
    {
        if (configuration.EmulatorId == CrocoDSProfile.Id) return CreateCrocoDS(configuration, coreOptions ?? []);
        var model = ModelCatalog.Get(configuration.Model);
        var options = configuration.Options ?? new Dictionary<string, string>();
        var ram = options.GetValueOrDefault(SettingsConstants.Ram,
            model.RamKib.ToString(System.Globalization.CultureInfo.InvariantCulture));
        return
        [
            Block(SettingsDescriptionFunctionsConstants.General, EmulationMachineTab.General,
                SettingsDescriptionFunctionsConstants.ResourceGeneral,
                SettingsDescriptionFunctionsConstants.IconGeneral, SettingsDescriptionFunctionsConstants.TwoColumnLayout,
                Information(SettingsConstants.Model, EmulationMachineTab.General,
                    SettingsDescriptionFunctionsConstants.General,
                    SettingsDescriptionFunctionsConstants.ResourceMachineModel, model.DisplayName),
                Information(SettingsConstants.Emulator, EmulationMachineTab.General,
                    SettingsDescriptionFunctionsConstants.General,
                    SettingsDescriptionFunctionsConstants.ResourceEmulator,
                    SettingsDescriptionFunctionsConstants.Caprice32)),
            Block(SettingsDescriptionFunctionsConstants.Cpu, EmulationMachineTab.Cpu,
                SettingsDescriptionFunctionsConstants.ResourceCpuProcessor,
                SettingsDescriptionFunctionsConstants.IconCpu, SettingsDescriptionFunctionsConstants.TwoColumnLayout,
                Information(SettingsConstants.Model + SettingsHelpDictionaryConstants.CpuSettingSuffix, EmulationMachineTab.Cpu,
                    SettingsDescriptionFunctionsConstants.Cpu,
                    SettingsDescriptionFunctionsConstants.ResourceCpuModel,
                    SettingsDescriptionFunctionsConstants.Z80A),
                Information(SettingsConstants.Model + SettingsHelpDictionaryConstants.CpuFrequencySettingSuffix, EmulationMachineTab.Cpu,
                    SettingsDescriptionFunctionsConstants.Cpu,
                    SettingsDescriptionFunctionsConstants.ResourceCpuSpeed,
                    SettingsDescriptionFunctionsConstants.CpuFrequency)),
            Block(SettingsDescriptionFunctionsConstants.Memory, EmulationMachineTab.Ram,
                SettingsDescriptionFunctionsConstants.ResourceMemoryMain,
                SettingsDescriptionFunctionsConstants.IconMemory, SettingsDescriptionFunctionsConstants.SingleColumnLayout,
                Select(SettingsConstants.Ram, EmulationMachineTab.Ram,
                    SettingsDescriptionFunctionsConstants.Memory,
                    SettingsDescriptionFunctionsConstants.ResourceMemoryMain, ram,
                    new[] { ModelConstants.Ram64Kib, ModelConstants.Ram128Kib, ModelConstants.Ram192Kib, ModelConstants.Ram576Kib }.Where(value => value >= model.RamKib)
                        .Select(value => Invariant(value.ToString(), $"{value}{SettingsDescriptionFunctionsConstants.RamUnitSuffix}", value)), true)),
            Block(SettingsDescriptionFunctionsConstants.Firmware, EmulationMachineTab.Rom,
                SettingsDescriptionFunctionsConstants.ResourceRom,
                SettingsDescriptionFunctionsConstants.IconFirmware, SettingsDescriptionFunctionsConstants.TwoColumnLayout,
                Information(SettingsConstants.FirmwareIntegrated, EmulationMachineTab.Rom,
                    SettingsDescriptionFunctionsConstants.Firmware,
                    SettingsDescriptionFunctionsConstants.ResourceFirmwareIntegrated,
                    SettingsDescriptionFunctionsConstants.Caprice32)),
            Block(SettingsDescriptionFunctionsConstants.Video, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.ResourceVideo,
                SettingsDescriptionFunctionsConstants.IconVideo, SettingsDescriptionFunctionsConstants.TwoColumnLayout,
                Select(SettingsConstants.VideoResolution, EmulationMachineTab.Video,
                    SettingsDescriptionFunctionsConstants.Video,
                    SettingsDescriptionFunctionsConstants.ResourceVideoResolution,
                    options.GetValueOrDefault(SettingsConstants.VideoResolution,
                        SettingsDescriptionFunctionsConstants.Resolution384),
                    InvariantChoices(SettingsDescriptionFunctionsConstants.Resolution384,
                        SettingsDescriptionFunctionsConstants.Resolution400)),
                Select(SettingsConstants.VideoMonitor, EmulationMachineTab.Video,
                    SettingsDescriptionFunctionsConstants.Video,
                    SettingsDescriptionFunctionsConstants.ResourceVideoMonitor,
                    options.GetValueOrDefault(SettingsConstants.VideoMonitor,
                        SettingsDescriptionFunctionsConstants.Color),
                    [new(SettingsDescriptionFunctionsConstants.Color,
                        SettingsDescriptionFunctionsConstants.ResourceColor),
                     new(SettingsDescriptionFunctionsConstants.Green,
                        SettingsDescriptionFunctionsConstants.ResourceGreen),
                     new(SettingsDescriptionFunctionsConstants.White,
                        SettingsDescriptionFunctionsConstants.ResourceWhite)]),
                Select(SettingsConstants.VideoIntensity, EmulationMachineTab.Video,
                    SettingsDescriptionFunctionsConstants.Video,
                    SettingsDescriptionFunctionsConstants.ResourceVideoIntensity,
                    options.GetValueOrDefault(SettingsConstants.VideoIntensity, SettingsDescriptionFunctionsConstants.DefaultVideoIntensity),
                    Enumerable.Range(SettingsDescriptionFunctionsConstants.MinimumVideoIntensity, SettingsDescriptionFunctionsConstants.VideoIntensityChoiceCount).Select(value => Invariant(value.ToString(), value.ToString()))),
                Toggle(SettingsConstants.VideoCrop, EmulationMachineTab.Video,
                    SettingsDescriptionFunctionsConstants.Video,
                    SettingsDescriptionFunctionsConstants.ResourceVideoCrop,
                    options.GetValueOrDefault(SettingsConstants.VideoCrop,
                        SettingsDescriptionFunctionsConstants.Disabled)
                        == SettingsDescriptionFunctionsConstants.Enabled)),
            Block(SettingsDescriptionFunctionsConstants.Audio, EmulationMachineTab.Audio,
                SettingsDescriptionFunctionsConstants.ResourceAudio,
                SettingsDescriptionFunctionsConstants.IconAudio, SettingsDescriptionFunctionsConstants.TwoColumnLayout,
                Toggle(SettingsConstants.AudioEnabled, EmulationMachineTab.Audio,
                    SettingsDescriptionFunctionsConstants.Audio,
                    SettingsDescriptionFunctionsConstants.ResourceAudioEnabled,
                    configuration.AudioEnabled),
                AudioOutput(configuration.Audio?.OutputDeviceId),
                Select(SettingsConstants.AudioLatency, EmulationMachineTab.Audio,
                    SettingsDescriptionFunctionsConstants.Audio,
                    SettingsDescriptionFunctionsConstants.ResourceAudioLatency,
                    (configuration.Audio?.LatencyMilliseconds ?? AudioConstants.DefaultLatencyMilliseconds).ToString(),
                    AudioConstants.LatencyChoicesMilliseconds
                        .Select(value => Invariant(value.ToString(), $"{value}{SettingsDescriptionFunctionsConstants.LatencyUnitSuffix}", value))),
                Toggle(SettingsConstants.FloppySound, EmulationMachineTab.Audio,
                    SettingsDescriptionFunctionsConstants.Audio,
                    SettingsDescriptionFunctionsConstants.ResourceAudioFloppySound,
                    options.GetValueOrDefault(SettingsConstants.FloppySound,
                        SettingsDescriptionFunctionsConstants.Enabled)
                        == SettingsDescriptionFunctionsConstants.Enabled))
        ];
    }
}
