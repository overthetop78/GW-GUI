using System.Globalization;

namespace GWGUI.Emulation.Wenquxing.Common.Machines.Common.Functions;
internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        var blocks = new List<EmulationSettingsBlock>();
        blocks.Add(FirmwareFunctions.Describe(configuration));
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
                        EmulationHardwareSettingsFunctions.FormatFrequency(hardware.CpuFrequencyHz)
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
                        hardware.MemoryResourceKey,
                        $"{hardware.MainMemoryBytes / SettingsConstants.BytesPerKibibyte} {SettingsConstants.KibibyteUnit}"
                    )],
                IsCompact: true
            ));
        blocks.Add(new(
                SettingsConstants.VideoDisplay,
                EmulationMachineTab.Video,
                SettingsConstants.ResourceDisplay,
                [Information(SettingsConstants.Resolution, EmulationMachineTab.Video, SettingsConstants.VideoDisplay, SettingsConstants.ResourceResolution, SettingsConstants.DisplayResolution)],
                IsCompact: true
            ));
        blocks.Add(Audio(configuration));
        return blocks;
    }

    private static EmulationSettingsField Information(string id, EmulationMachineTab tab, string block, string label, string value) => new(id, tab, block, label, EmulationSettingsEditor.Information, value);
    private static EmulationSettingsBlock Audio(MachineConfiguration configuration) => new(
        SettingsConstants.ResourceAudio,
        EmulationMachineTab.Audio,
        SettingsConstants.ResourceAudio,
        [
            EmulationHardwareSettingsFunctions.Toggle(SettingsConstants.AudioEnabled, EmulationMachineTab.Audio, SettingsConstants.ResourceAudio, SettingsConstants.ResourceAudioEnabled, configuration.AudioEnabled, null, null),
            EmulationHardwareSettingsFunctions.AudioOutput(SettingsConstants.AudioOutput, SettingsConstants.ResourceAudio, SettingsConstants.ResourceAudioOutput, configuration.Options.GetValueOrDefault(SettingsConstants.AudioOutput, SettingsConstants.DefaultOutput), [new(SettingsConstants.DefaultOutput, SettingsConstants.ResourceDefaultOutput)], null, null),
            AudioSelection(configuration, SettingsConstants.AudioLatency, SettingsConstants.ResourceAudioLatency, EmulationSettingConstants.DefaultAudioLatencyMilliseconds, EmulationSettingConstants.AudioLatencyMillisecondsChoices, SettingsConstants.MillisecondsUnit),
            AudioSelection(configuration, VideoAudioSettingsConstants.AudioVolumeOption, SettingsConstants.ResourceAudioVolume, EmulationSettingConstants.DefaultAudioVolumePercent, EmulationSettingConstants.AudioVolumePercentChoices, SettingsConstants.PercentUnit)
        ],
        IsCompact: true
    );
    private static EmulationSettingsField AudioSelection(MachineConfiguration configuration, string id, string resource, int fallback, IReadOnlyList<int> values, string unit) => EmulationHardwareSettingsFunctions.AudioSelection(configuration.Options, id, SettingsConstants.ResourceAudio, resource, fallback, values, unit, CultureInfo.InvariantCulture);
}