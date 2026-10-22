using CommonCoreHostFunctions = GWGUI.Emulation.Panasonic.Common.Functions.CoreHostFunctions;
using System.Globalization;

namespace GWGUI.Emulation.Panasonic.Common.Machines.Common.Functions;
internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        var blocks = new List<EmulationSettingsBlock>
        {
            FirmwareFunctions.Describe(configuration)
        };
        var nativeBlocks = configuration.Core != Emulator.None && new Engine().Adapter(configuration)is IEmulatorSettings settings ? settings.DescribeSettings(configuration) : [];
        blocks.Add(new(
                SettingsConstants.ResourceCpuProcessor,
                EmulationMachineTab.Cpu,
                SettingsConstants.ResourceCpuProcessor,
                [
                    Information(SettingsConstants.CpuModel, EmulationMachineTab.Cpu, SettingsConstants.ResourceCpuProcessor, SettingsConstants.ResourceCpuModel, hardware.CpuName),
                    Information(
                        SettingsConstants.CpuFrequency,
                        EmulationMachineTab.Cpu,
                        SettingsConstants.ResourceCpuProcessor,
                        SettingsConstants.ResourceCpuSpeedOriginal,
                        CommonCoreHostFunctions.HardwareSettings.FormatFrequency(hardware.CpuFrequencyHz)
                    )
                ],
                IsCompact: true
            ));
        blocks.Add(new(
                SettingsConstants.ResourceMemoryMain,
                EmulationMachineTab.Ram,
                SettingsConstants.ResourceMemoryMain,
                [Information(
                        SettingsConstants.MainMemory,
                        EmulationMachineTab.Ram,
                        SettingsConstants.ResourceMemoryMain,
                        SettingsConstants.ResourceMemoryMain,
                        $"{hardware.MainMemoryBytes / SettingsConstants.BytesPerKibibyte} {SettingsConstants.KibibyteUnit}"
                    )],
                IsCompact: true
            ));
        blocks.Add(new(
                SettingsConstants.ResourceMemoryVideo,
                EmulationMachineTab.Ram,
                SettingsConstants.ResourceMemoryVideo,
                [Information(
                        SettingsConstants.VideoMemory,
                        EmulationMachineTab.Ram,
                        SettingsConstants.ResourceMemoryVideo,
                        SettingsConstants.ResourceMemoryVideo,
                        $"{hardware.VideoMemoryBytes / SettingsConstants.BytesPerKibibyte} {SettingsConstants.KibibyteUnit}"
                    )],
                IsCompact: true
            ));
        blocks.Add(new(
                SettingsConstants.NonVolatileMemory,
                EmulationMachineTab.Storage,
                SettingsConstants.ResourceNonVolatileMemory,
                [Information(
                        SettingsConstants.NonVolatileMemory,
                        EmulationMachineTab.Storage,
                        SettingsConstants.NonVolatileMemory,
                        SettingsConstants.ResourceNonVolatileMemory,
                        $"{hardware.SaveMemoryBytes / SettingsConstants.BytesPerKibibyte} {SettingsConstants.KibibyteUnit}"
                    )],
                IsCompact: true
            ));
        blocks.AddRange(nativeBlocks.Where(block => block.Tab != EmulationMachineTab.Audio));
        blocks.AddRange(nativeBlocks.Where(block => block.Tab == EmulationMachineTab.Audio));
        blocks.Add(Audio(configuration));
        return blocks;
    }

    private static EmulationSettingsField Information(string id, EmulationMachineTab tab, string block, string label, string value) => new(id, tab, block, label, EmulationSettingsEditor.Information, value);
    private static EmulationSettingsBlock Audio(MachineConfiguration configuration) => new(
        SettingsConstants.ResourceAudio,
        EmulationMachineTab.Audio,
        SettingsConstants.ResourceAudio,
        [
            CommonCoreHostFunctions.HardwareSettings.Toggle(SettingsConstants.AudioEnabled, EmulationMachineTab.Audio, SettingsConstants.ResourceAudio, SettingsConstants.ResourceAudioEnabled, configuration.AudioEnabled, null, null),
            CommonCoreHostFunctions.HardwareSettings.AudioOutput(SettingsConstants.AudioOutput, SettingsConstants.ResourceAudio, SettingsConstants.ResourceAudioOutput, configuration.Options.GetValueOrDefault(SettingsConstants.AudioOutput, SettingsConstants.DefaultOutput), [new(SettingsConstants.DefaultOutput, SettingsConstants.ResourceDefaultOutput)], null, null),
            AudioSelection(configuration, SettingsConstants.AudioLatency, SettingsConstants.ResourceAudioLatency, EmulationSettingConstants.DefaultAudioLatencyMilliseconds, EmulationSettingConstants.AudioLatencyMillisecondsChoices, SettingsConstants.MillisecondsUnit),
            AudioSelection(configuration, VideoAudioSettingsConstants.AudioVolumeOption, SettingsConstants.ResourceAudioVolume, EmulationSettingConstants.DefaultAudioVolumePercent, EmulationSettingConstants.AudioVolumePercentChoices, SettingsConstants.PercentUnit)
        ],
        IsCompact: true
    );
    private static EmulationSettingsField AudioSelection(MachineConfiguration configuration, string id, string resource, int fallback, IReadOnlyList<int> values, string unit) => CommonCoreHostFunctions.HardwareSettings.AudioSelection(configuration.Options, id, SettingsConstants.ResourceAudio, resource, fallback, values, unit, CultureInfo.InvariantCulture);
}