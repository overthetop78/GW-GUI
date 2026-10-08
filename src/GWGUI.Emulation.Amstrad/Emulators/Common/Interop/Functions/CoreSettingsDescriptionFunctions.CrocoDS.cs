using CrocoDSProfile = GWGUI.Emulation.Amstrad.Emulators.CrocoDS.Constants.EmulatorConstants;
namespace GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Functions;

internal static partial class CoreSettingsDescriptionFunctions
{
    private static IReadOnlyList<EmulationSettingsBlock> CreateCrocoDS(
        MachineConfiguration configuration, IReadOnlyList<CoreOption> coreOptions)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var blocks = new List<EmulationSettingsBlock>
        {
            Block(SettingsDescriptionFunctionsConstants.General, EmulationMachineTab.General,
                SettingsDescriptionFunctionsConstants.ResourceGeneral,
                SettingsDescriptionFunctionsConstants.IconGeneral, SettingsDescriptionFunctionsConstants.TwoColumnLayout,
                Information(SettingsConstants.Model, EmulationMachineTab.General, SettingsDescriptionFunctionsConstants.General,
                    SettingsDescriptionFunctionsConstants.ResourceMachineModel, model.DisplayName),
                Information(SettingsConstants.Emulator, EmulationMachineTab.General, SettingsDescriptionFunctionsConstants.General,
                    SettingsDescriptionFunctionsConstants.ResourceEmulator, CrocoDSProfile.DisplayName)),
            Block(SettingsDescriptionFunctionsConstants.Cpu, EmulationMachineTab.Cpu,
                SettingsDescriptionFunctionsConstants.ResourceCpuProcessor,
                SettingsDescriptionFunctionsConstants.IconCpu, SettingsDescriptionFunctionsConstants.TwoColumnLayout,
                Information(SettingsConstants.Model + SettingsHelpDictionaryConstants.CpuSettingSuffix, EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.Cpu,
                    SettingsDescriptionFunctionsConstants.ResourceCpuModel, SettingsDescriptionFunctionsConstants.Z80A),
                Information(SettingsConstants.Model + SettingsHelpDictionaryConstants.CpuFrequencySettingSuffix, EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.Cpu,
                    SettingsDescriptionFunctionsConstants.ResourceCpuSpeed, SettingsDescriptionFunctionsConstants.CpuFrequency)),
            Block(SettingsDescriptionFunctionsConstants.Memory, EmulationMachineTab.Ram,
                SettingsDescriptionFunctionsConstants.ResourceMemoryMain,
                SettingsDescriptionFunctionsConstants.IconMemory, SettingsDescriptionFunctionsConstants.SingleColumnLayout,
                new EmulationSettingsField(SettingsConstants.Ram, EmulationMachineTab.Ram, SettingsDescriptionFunctionsConstants.Memory,
                    SettingsDescriptionFunctionsConstants.ResourceMemoryMain,
                    EmulationSettingsEditor.Information, SettingsDescriptionFunctionsConstants.Cpc6128RamDisplay)),
            Block(SettingsDescriptionFunctionsConstants.Firmware, EmulationMachineTab.Rom,
                SettingsDescriptionFunctionsConstants.ResourceRom,
                SettingsDescriptionFunctionsConstants.IconFirmware, SettingsDescriptionFunctionsConstants.SingleColumnLayout,
                new EmulationSettingsField(SettingsConstants.FirmwareIntegrated, EmulationMachineTab.Rom,
                    SettingsDescriptionFunctionsConstants.Firmware, SettingsDescriptionFunctionsConstants.ResourceFirmwareIntegrated,
                    EmulationSettingsEditor.Information, CrocoDSProfile.DisplayName)),
            Block(SettingsDescriptionFunctionsConstants.Audio, EmulationMachineTab.Audio,
                SettingsDescriptionFunctionsConstants.ResourceAudio,
                SettingsDescriptionFunctionsConstants.IconAudio, SettingsDescriptionFunctionsConstants.TwoColumnLayout,
                Toggle(SettingsConstants.AudioEnabled, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio,
                    SettingsDescriptionFunctionsConstants.ResourceAudioEnabled, configuration.AudioEnabled),
                AudioOutput(configuration.Audio?.OutputDeviceId),
                Select(SettingsConstants.AudioLatency, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio,
                    SettingsDescriptionFunctionsConstants.ResourceAudioLatency,
                    (configuration.Audio?.LatencyMilliseconds ?? AudioConstants.DefaultLatencyMilliseconds).ToString(),
                    AudioConstants.LatencyChoicesMilliseconds
                        .Select(value => Invariant(value.ToString(), $"{value}{SettingsDescriptionFunctionsConstants.LatencyUnitSuffix}", value))))
        };
        var fields = coreOptions.Where(option => option.IsVisible && option.Values.Count > BufferConstants.EmptyCollectionCount)
            .Select(option => new EmulationSettingsField(option.Key,
                option.Key == CrocoDSProfile.SpeedHackOption ? EmulationMachineTab.Cpu : EmulationMachineTab.Video,
                option.Key == CrocoDSProfile.SpeedHackOption ? SettingsDescriptionFunctionsConstants.Cpu : CrocoDSProfile.Id,
                SettingsDescriptionFunctionsConstants.CrocoDSOptionResourcePrefix + option.Key,
                EmulationSettingsEditor.Selection,
                configuration.Options?.GetValueOrDefault(option.Key) ?? option.DefaultValue,
                option.Values.Select(value => CrocoDSChoice(value.Value)).ToArray())).ToArray();
        var cpuFields = fields.Where(field => field.Tab == EmulationMachineTab.Cpu).ToArray();
        if (cpuFields.Length > BufferConstants.EmptyCollectionCount)
        {
            var cpuIndex = blocks.FindIndex(block => block.Tab == EmulationMachineTab.Cpu);
            blocks[cpuIndex] = blocks[cpuIndex] with { Fields = [.. blocks[cpuIndex].Fields, .. cpuFields] };
        }
        var videoFields = fields.Where(field => field.Tab == EmulationMachineTab.Video).ToArray();
        if (videoFields.Length > BufferConstants.EmptyCollectionCount)
            blocks.Add(Block(CrocoDSProfile.Id, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.ResourceVideo,
                SettingsDescriptionFunctionsConstants.IconVideo, SettingsDescriptionFunctionsConstants.TwoColumnLayout, videoFields));
        return blocks;
    }

    private static EmulationSettingsChoice CrocoDSChoice(string value) => value switch
    {
        SettingsDescriptionFunctionsConstants.Color => new(value, SettingsDescriptionFunctionsConstants.ResourceColor),
        SettingsDescriptionFunctionsConstants.Green => new(value, SettingsDescriptionFunctionsConstants.ResourceGreen),
        SettingsDescriptionFunctionsConstants.Auto or SettingsDescriptionFunctionsConstants.Overscan => new(value, SettingsDescriptionFunctionsConstants.CrocoDSValueResourcePrefix + value),
        SettingsDescriptionFunctionsConstants.Yes => new(value, SettingsDescriptionFunctionsConstants.CrocoDSEnabledResourceKey),
        SettingsDescriptionFunctionsConstants.No => new(value, SettingsDescriptionFunctionsConstants.CrocoDSDisabledResourceKey),
        _ => Invariant(value, value)
    };
}
