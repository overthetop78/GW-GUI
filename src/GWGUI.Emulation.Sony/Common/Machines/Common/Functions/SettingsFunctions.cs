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
                    string.Join(" / ", model.Processors))),
            Block(SettingsDescriptionFunctionsConstants.Acceleration, EmulationMachineTab.Cpu,
                SettingsDescriptionFunctionsConstants.ResourceAcceleration,
                SettingsDescriptionFunctionsConstants.IconCpu, 1,
                Information(SettingsConstants.Model + ".frequency", EmulationMachineTab.Cpu,
                    SettingsDescriptionFunctionsConstants.Acceleration,
                    SettingsDescriptionFunctionsConstants.ResourceCpuSpeed,
                    model.CpuFrequency ?? SettingsDescriptionFunctionsConstants.CpuFrequency)),
            Block(SettingsDescriptionFunctionsConstants.Memory, EmulationMachineTab.Ram,
                SettingsDescriptionFunctionsConstants.ResourceMemoryMain,
                SettingsDescriptionFunctionsConstants.IconMemory, 1,
                Information(SettingsConstants.Ram, EmulationMachineTab.Ram,
                    SettingsDescriptionFunctionsConstants.Memory,
                    SettingsDescriptionFunctionsConstants.ResourceMemoryMain,
                    $"{(model.Id == ModelConstants.Psp
                        && configuration.Options?.GetValueOrDefault(Emulators.Ppsspp.Constants.OptionConstants.PspModelKey)
                            != "psp_1000" ? 64 * 1024 : model.RamKib)} KiB")),
            Block(SettingsDescriptionFunctionsConstants.Video, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.ResourceVideo,
                SettingsDescriptionFunctionsConstants.IconVideo, 2,
                Information(SettingsConstants.Model + ".video", EmulationMachineTab.Video,
                    SettingsDescriptionFunctionsConstants.Video,
                    SettingsDescriptionFunctionsConstants.ResourceVideoChip,
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
                    SettingsDescriptionFunctionsConstants.ResourceAudioChip,
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
                if (coreBlock.Tab == EmulationMachineTab.Storage) continue;
                var index = blocks.FindIndex(block => block.Id == coreBlock.Id);
                if (index < 0) blocks.Add(coreBlock);
                else
                {
                    var existing = blocks[index];
                    blocks[index] = existing with { Fields = [.. existing.Fields,
                        .. coreBlock.Fields.Select(field => field with { BlockId = existing.Id })],
                        Columns = Math.Min(existing.Columns, coreBlock.Columns) };
                }
            }
            if (model.Id == ModelConstants.PlayStation)
            {
                var clock = blocks.SelectMany(block => block.Fields).FirstOrDefault(field =>
                    Emulators.Common.Interop.Constants.CoreSettingsConstants.IsCpuClock(field.Id));
                if (clock is not null)
                {
                    var index = blocks.FindIndex(block => block.Id == SettingsDescriptionFunctionsConstants.Acceleration);
                    blocks[index] = blocks[index] with { Fields = [.. blocks[index].Fields,
                        new EmulationSettingsField(SettingsDescriptionFunctionsConstants.EffectiveFrequencyField,
                            EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.Acceleration,
                            SettingsDescriptionFunctionsConstants.ResourceEffectiveFrequency,
                            EmulationSettingsEditor.Information, model.CpuFrequency,
                            NumericValue: SettingsDescriptionFunctionsConstants.PlayStationFrequencyHz,
                            ExplanationResourceKey: SettingsDescriptionFunctionsConstants.EffectiveFrequencyHelpShort,
                            DetailedExplanationResourceKey: SettingsDescriptionFunctionsConstants.EffectiveFrequencyHelpDetailed,
                            FrequencyScaleFieldId: clock.Id)] };
                }
            }
        }
        var hardwareVideo = blocks.FindIndex(block => block.Id == SettingsDescriptionFunctionsConstants.Video);
        var renderingVideo = blocks.FindIndex(block => block.Tab == EmulationMachineTab.Video
            && block.IsCollapsible);
        if (hardwareVideo >= 0 && renderingVideo >= 0)
        {
            var rendering = blocks[renderingVideo];
            blocks[renderingVideo] = rendering with { Fields = [.. blocks[hardwareVideo].Fields
                .Select(field => field with { BlockId = rendering.Id }), .. rendering.Fields] };
            blocks.RemoveAt(hardwareVideo);
        }
        var audio = blocks.FindIndex(block => block.Id == SettingsDescriptionFunctionsConstants.Audio);
        var nativeAudio = blocks.FindIndex(block => block.Id == Emulators.Common.Interop.Constants.CoreSettingsConstants.AudioOptionsBlock);
        if (audio >= 0 && nativeAudio >= 0)
        {
            var chip = blocks[audio].Fields.Single(field => field.Id == SettingsConstants.Model + ".audio");
            blocks[audio] = blocks[audio] with { Fields = blocks[audio].Fields.Where(field => field != chip).ToArray() };
            blocks[nativeAudio] = blocks[nativeAudio] with { Fields =
                [chip with { BlockId = blocks[nativeAudio].Id }, .. blocks[nativeAudio].Fields] };
        }
        return blocks;
    }
}
