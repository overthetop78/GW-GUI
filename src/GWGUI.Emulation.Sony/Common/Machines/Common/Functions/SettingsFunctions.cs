namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var adapter = EmulatorCatalog.CreateAdapters().FirstOrDefault(item => item.EmulatorId == configuration.EmulatorId);
        var blocks = new List<EmulationSettingsBlock>
        {
            Block(SettingsDescriptionFunctionsConstants.General, EmulationMachineTab.General,
                SettingsDescriptionFunctionsConstants.ResourceGeneral,
                SettingsDescriptionFunctionsConstants.IconGeneral, 2,
                Information(SettingsConstants.Model, EmulationMachineTab.General,
                    SettingsDescriptionFunctionsConstants.General,
                    SettingsDescriptionFunctionsConstants.ResourceMachineModel, model.DisplayName),
                Information(SettingsConstants.Emulator, EmulationMachineTab.General,
                    SettingsDescriptionFunctionsConstants.General,
                    SettingsDescriptionFunctionsConstants.ResourceEmulator,
                    adapter?.Definition.DisplayName ?? configuration.EmulatorId)),
            Block(SettingsDescriptionFunctionsConstants.Cpu, EmulationMachineTab.Cpu,
                SettingsDescriptionFunctionsConstants.ResourceCpuProcessor,
                SettingsDescriptionFunctionsConstants.IconCpu, 2,
                Information(SettingsConstants.Model + ".cpu", EmulationMachineTab.Cpu,
                    SettingsDescriptionFunctionsConstants.Cpu,
                    SettingsDescriptionFunctionsConstants.ResourceCpuModel,
                    string.Join(" / ", model.Processors)),
                Information(SettingsConstants.Model + ".frequency", EmulationMachineTab.Cpu,
                    SettingsDescriptionFunctionsConstants.Cpu,
                    SettingsDescriptionFunctionsConstants.ResourceCpuSpeed,
                    model.CpuFrequency ?? SettingsDescriptionFunctionsConstants.CpuFrequency)),
            Block(SettingsDescriptionFunctionsConstants.Memory, EmulationMachineTab.Ram,
                SettingsDescriptionFunctionsConstants.ResourceMemoryMain,
                SettingsDescriptionFunctionsConstants.IconMemory, 1,
                Information(SettingsConstants.Ram, EmulationMachineTab.Ram,
                    SettingsDescriptionFunctionsConstants.Memory,
                    SettingsDescriptionFunctionsConstants.ResourceMemoryMain,
                    $"{model.RamKib} KiB")),
            Block(SettingsDescriptionFunctionsConstants.Video, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.ResourceVideo,
                SettingsDescriptionFunctionsConstants.IconVideo, 2,
                Information(SettingsConstants.Model + ".video", EmulationMachineTab.Video,
                    SettingsDescriptionFunctionsConstants.Video,
                    SettingsDescriptionFunctionsConstants.ResourceVideo,
                    model.VideoChip ?? SettingsDescriptionFunctionsConstants.CpuFrequency)),
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
                Information(SettingsConstants.Model + ".audio", EmulationMachineTab.Audio,
                    SettingsDescriptionFunctionsConstants.Audio,
                    SettingsDescriptionFunctionsConstants.ResourceAudio,
                    model.AudioChip ?? SettingsDescriptionFunctionsConstants.CpuFrequency))
        };
        if (adapter is not null)
        {
            var firmware = FirmwareConfigurationFunctions.Fields(configuration, adapter);
            if (firmware.Count != 0)
                blocks.Add(Block(SettingsDescriptionFunctionsConstants.Firmware, EmulationMachineTab.Rom,
                    SettingsDescriptionFunctionsConstants.ResourceRom, SettingsDescriptionFunctionsConstants.IconFirmware,
                    SettingsDescriptionFunctionsConstants.SingleColumnLayout, [.. firmware]));
            foreach (var coreBlock in adapter.GetSettingsBlocks(configuration))
            {
                var index = blocks.FindIndex(block => block.Tab == coreBlock.Tab);
                if (index < 0) blocks.Add(coreBlock);
                else
                {
                    var existing = blocks[index];
                    blocks[index] = existing with { Fields = [.. existing.Fields,
                        .. coreBlock.Fields.Select(field => field with { BlockId = existing.Id })],
                        Columns = Math.Min(existing.Columns, coreBlock.Columns) };
                }
            }
        }
        return blocks;
    }
}
