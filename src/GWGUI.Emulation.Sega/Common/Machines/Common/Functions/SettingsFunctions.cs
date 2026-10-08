namespace GWGUI.Emulation.Sega.Common.Machines.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var adapter = new Engine().Adapter(configuration);
        var options = configuration.Options ?? new Dictionary<string, string>();
        var ram = model.RamBytes is { } bytes ? bytes.ToString(System.Globalization.CultureInfo.InvariantCulture) + ModelConstants.BytesUnit : options.GetValueOrDefault(SettingsConstants.Ram,
            model.RamKib.ToString(System.Globalization.CultureInfo.InvariantCulture));
        var processors = string.Join(" / ", model.Processors);
        var hardwareFields = model.Id switch
        {
            ModelConstants.MasterSystem => new EmulationSettingsField[]
            {
                Select(SettingsConstants.MasterSystemVariant, EmulationMachineTab.General,
                    SettingsDescriptionFunctionsConstants.Hardware,
                    SettingsDescriptionFunctionsConstants.ResourceMasterSystemVariant,
                    options.GetValueOrDefault(SettingsConstants.MasterSystemVariant,
                        ModelConstants.MasterSystemSmsI),
                    [new(ModelConstants.MasterSystemSmsI,
                        SettingsDescriptionFunctionsConstants.ResourceMasterSystemSmsI),
                     new(ModelConstants.MasterSystemSmsIi,
                        SettingsDescriptionFunctionsConstants.ResourceMasterSystemSmsIi)], true),
            },
            ModelConstants.MegaDrive => new EmulationSettingsField[]
            {
                Select(SettingsConstants.MegaDriveModel, EmulationMachineTab.General,
                    SettingsDescriptionFunctionsConstants.Hardware,
                    SettingsDescriptionFunctionsConstants.ResourceMegaDriveModel,
                    options.GetValueOrDefault(SettingsConstants.MegaDriveModel,
                        ModelConstants.MegaDriveModelI),
                    [new(ModelConstants.MegaDriveModelI,
                        SettingsDescriptionFunctionsConstants.ResourceMegaDriveModelI),
                     new(ModelConstants.MegaDriveModelII,
                        SettingsDescriptionFunctionsConstants.ResourceMegaDriveModelII)], true),
                Select(SettingsConstants.MegaCdModel, EmulationMachineTab.General,
                    SettingsDescriptionFunctionsConstants.Hardware,
                    SettingsDescriptionFunctionsConstants.ResourceMegaCdModel,
                    options.GetValueOrDefault(SettingsConstants.MegaCdModel,
                        ModelConstants.MegaCdI),
                    [new(ModelConstants.MegaCdI,
                        SettingsDescriptionFunctionsConstants.ResourceMegaCdI),
                     new(ModelConstants.MegaCdII,
                        SettingsDescriptionFunctionsConstants.ResourceMegaCdII)], true),
                Toggle(SettingsConstants.MegaCdEnabled, EmulationMachineTab.General,
                    SettingsDescriptionFunctionsConstants.Hardware,
                    SettingsDescriptionFunctionsConstants.ResourceMegaCdEnabled,
                    options.GetValueOrDefault(SettingsConstants.MegaCdEnabled,
                        SettingsDescriptionFunctionsConstants.Disabled)
                        == SettingsDescriptionFunctionsConstants.Enabled, true),
                Toggle(SettingsConstants.MegaDriveThirtyTwoX, EmulationMachineTab.General,
                    SettingsDescriptionFunctionsConstants.Hardware,
                    SettingsDescriptionFunctionsConstants.ResourceMegaDriveThirtyTwoX,
                    options.GetValueOrDefault(SettingsConstants.MegaDriveThirtyTwoX,
                        SettingsDescriptionFunctionsConstants.Disabled)
                        == SettingsDescriptionFunctionsConstants.Enabled, true),
                Select(SettingsConstants.MegaDriveRegion, EmulationMachineTab.General,
                    SettingsDescriptionFunctionsConstants.Hardware,
                    SettingsDescriptionFunctionsConstants.ResourceMegaDriveRegion,
                    options.GetValueOrDefault(SettingsConstants.MegaDriveRegion,
                        ModelConstants.RegionAutomatic),
                    [new(ModelConstants.RegionAutomatic,
                        SettingsDescriptionFunctionsConstants.ResourceMegaDriveRegionAutomatic),
                     new(ModelConstants.RegionNtscU,
                        SettingsDescriptionFunctionsConstants.ResourceMegaDriveRegionNtscU),
                     new(ModelConstants.RegionNtscJ,
                        SettingsDescriptionFunctionsConstants.ResourceMegaDriveRegionNtscJ),
                     new(ModelConstants.RegionPal,
                        SettingsDescriptionFunctionsConstants.ResourceMegaDriveRegionPal),
                     new(ModelConstants.RegionSecam,
                        SettingsDescriptionFunctionsConstants.ResourceMegaDriveRegionSecam)], true),
                Select(SettingsConstants.MegaDriveVideoStandard, EmulationMachineTab.General,
                    SettingsDescriptionFunctionsConstants.Hardware,
                    SettingsDescriptionFunctionsConstants.ResourceMegaDriveVideoStandard,
                    options.GetValueOrDefault(SettingsConstants.MegaDriveVideoStandard,
                        ModelConstants.VideoStandardAutomatic),
                    [new(ModelConstants.VideoStandardAutomatic,
                        SettingsDescriptionFunctionsConstants.ResourceMegaDriveVideoStandardAutomatic),
                     new(ModelConstants.VideoStandard50Hz,
                        SettingsDescriptionFunctionsConstants.ResourceMegaDriveVideoStandard50Hz),
                     new(ModelConstants.VideoStandard60Hz,
                        SettingsDescriptionFunctionsConstants.ResourceMegaDriveVideoStandard60Hz)], true)
            },
            _ => []
        };
        EmulationSettingsBlock[] blocks =
        [
            Block(SettingsDescriptionFunctionsConstants.General, EmulationMachineTab.General,
                SettingsDescriptionFunctionsConstants.ResourceGeneral,
                SettingsDescriptionFunctionsConstants.IconGeneral, 2,
                Information(SettingsConstants.Model, EmulationMachineTab.General,
                    SettingsDescriptionFunctionsConstants.General,
                    SettingsDescriptionFunctionsConstants.ResourceMachineModel, model.DisplayName),
                Information(SettingsConstants.Emulator, EmulationMachineTab.General,
                    SettingsDescriptionFunctionsConstants.General,
                    SettingsDescriptionFunctionsConstants.ResourceEmulator,
                    adapter.Definition.DisplayName)),
            Block(SettingsDescriptionFunctionsConstants.Hardware, EmulationMachineTab.General,
                SettingsDescriptionFunctionsConstants.ResourceHardware,
                SettingsDescriptionFunctionsConstants.IconGeneral, 2, hardwareFields),
            Block(SettingsDescriptionFunctionsConstants.Cpu, EmulationMachineTab.Cpu,
                SettingsDescriptionFunctionsConstants.ResourceCpuProcessor,
                SettingsDescriptionFunctionsConstants.IconCpu, 2,
                Information(SettingsConstants.Model + ".cpu", EmulationMachineTab.Cpu,
                    SettingsDescriptionFunctionsConstants.Cpu,
                    SettingsDescriptionFunctionsConstants.ResourceCpuModel,
                    processors),
                Information(SettingsConstants.Model + ".frequency", EmulationMachineTab.Cpu,
                    SettingsDescriptionFunctionsConstants.Cpu,
                    SettingsDescriptionFunctionsConstants.ResourceCpuSpeed,
                    model.CpuFrequency ?? SettingsDescriptionFunctionsConstants.CpuFrequency)),
            Block(SettingsDescriptionFunctionsConstants.Memory, EmulationMachineTab.Ram,
                SettingsDescriptionFunctionsConstants.ResourceMemoryMain,
                SettingsDescriptionFunctionsConstants.IconMemory, 1,
                model.SupportsRamConfiguration
                    ? Select(SettingsConstants.Ram, EmulationMachineTab.Ram,
                        SettingsDescriptionFunctionsConstants.Memory,
                        SettingsDescriptionFunctionsConstants.ResourceMemoryMain, ram,
                        MemoryChoices(model), true)
                    : Information(SettingsConstants.Ram, EmulationMachineTab.Ram,
                        SettingsDescriptionFunctionsConstants.Memory,
                        SettingsDescriptionFunctionsConstants.ResourceMemoryMain, ram)),
            Block(SettingsDescriptionFunctionsConstants.Firmware, EmulationMachineTab.Rom,
                SettingsDescriptionFunctionsConstants.ResourceRom,
                SettingsDescriptionFunctionsConstants.IconFirmware, 2,
                FirmwareConfigurationFunctions.Fields(configuration, adapter).ToArray()),
            Block(SettingsDescriptionFunctionsConstants.Video, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.ResourceVideo,
                SettingsDescriptionFunctionsConstants.IconVideo, 2,
                Information(SettingsConstants.VideoChip, EmulationMachineTab.Video,
                    SettingsDescriptionFunctionsConstants.Video,
                    SettingsDescriptionFunctionsConstants.ResourceVideoChip,
                    model.VideoChip ?? SettingsDescriptionFunctionsConstants.Unknown)),
            Block(SettingsDescriptionFunctionsConstants.Audio, EmulationMachineTab.Audio,
                SettingsDescriptionFunctionsConstants.ResourceAudio,
                SettingsDescriptionFunctionsConstants.IconAudio, 2,
                Toggle(SettingsConstants.AudioEnabled, EmulationMachineTab.Audio,
                    SettingsDescriptionFunctionsConstants.Audio,
                    SettingsDescriptionFunctionsConstants.ResourceAudioEnabled,
                    configuration.AudioEnabled),
                AudioOutput(configuration.Audio?.OutputDeviceId),
                Select(SettingsConstants.AudioLatency, EmulationMachineTab.Audio,
                    SettingsDescriptionFunctionsConstants.Audio,
                    SettingsDescriptionFunctionsConstants.ResourceAudioLatency,
                    (configuration.Audio?.LatencyMilliseconds ?? 50).ToString(),
                    new[] { 20, 35, 50, 75, 100, 150, 250 }
                        .Select(value => Invariant(value.ToString(), $"{value} ms", value))),
                Information(SettingsConstants.AudioChip, EmulationMachineTab.Audio,
                    SettingsDescriptionFunctionsConstants.Audio,
                    SettingsDescriptionFunctionsConstants.ResourceAudioChip,
                    model.AudioChip ?? SettingsDescriptionFunctionsConstants.Unknown))
        ];
        var result = blocks.Where(block => block.Fields.Count > 0).ToList();
        foreach (var coreBlock in adapter.GetSettingsBlocks(configuration))
        {
            var existingIndex = result.FindIndex(block => block.Tab == coreBlock.Tab);
            if (existingIndex < 0)
            {
                result.Add(coreBlock);
                continue;
            }
            var existing = result[existingIndex];
            result[existingIndex] = existing with
            {
                Fields = existing.Fields.Concat(coreBlock.Fields.Select(field => field with
                {
                    BlockId = existing.Id
                })).ToArray(),
                Columns = Math.Min(existing.Columns, coreBlock.Columns)
            };
        }
        return result;
    }

    private static IEnumerable<EmulationSettingsChoice> MemoryChoices(Model model)
    {
        var values = new[] { model.RamKib, 64, 128, 192, 576 }
            .Where(value => value >= model.RamKib).Distinct().Order();
        return values.Select(value => Invariant(value.ToString(), $"{value} KiB", value));
    }

}
