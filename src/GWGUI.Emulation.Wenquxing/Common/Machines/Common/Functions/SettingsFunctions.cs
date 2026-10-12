using System.Globalization;

namespace GWGUI.Emulation.Wenquxing.Common.Machines.Common.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        var blocks = new List<EmulationSettingsBlock>();
        blocks.Add(FirmwareFunctions.Describe(configuration));
        blocks.Add(new(SettingsConstants.ResourceCpuProcessor, EmulationMachineTab.Cpu, SettingsConstants.ResourceCpuProcessor, [Information(SettingsConstants.CpuModel, EmulationMachineTab.Cpu, SettingsConstants.ResourceCpuProcessor, SettingsConstants.ResourceCpuModel, hardware.CpuName), Information(SettingsConstants.CpuFrequency, EmulationMachineTab.Cpu, SettingsConstants.ResourceCpuProcessor, SettingsConstants.ResourceCpuSpeedOriginal, $"{(hardware.CpuFrequencyHz / SettingsConstants.HertzPerMegahertz).ToString("0.######", CultureInfo.CurrentCulture)} {SettingsConstants.MegahertzUnit}")], IsCompact: true));
        blocks.Add(new(SettingsConstants.ResourceMemoryMain, EmulationMachineTab.Ram, SettingsConstants.ResourceMemoryMain, [Information(SettingsConstants.MainMemory, EmulationMachineTab.Ram, SettingsConstants.ResourceMemoryMain, hardware.MemoryResourceKey, $"{hardware.MainMemoryBytes / SettingsConstants.BytesPerKibibyte} {SettingsConstants.KibibyteUnit}")], IsCompact: true));
        blocks.Add(new(SettingsConstants.VideoDisplay, EmulationMachineTab.Video, SettingsConstants.ResourceDisplay, [Information(SettingsConstants.Resolution, EmulationMachineTab.Video, SettingsConstants.VideoDisplay, SettingsConstants.ResourceResolution, SettingsConstants.DisplayResolution)], IsCompact: true));
        blocks.Add(Audio(configuration));
        return blocks;
    }
    private static EmulationSettingsField Information(string id, EmulationMachineTab tab, string block, string label, string value) => new(id, tab, block, label, EmulationSettingsEditor.Information, value);
    private static EmulationSettingsBlock Audio(MachineConfiguration configuration) => new(SettingsConstants.ResourceAudio, EmulationMachineTab.Audio, SettingsConstants.ResourceAudio, [new(SettingsConstants.AudioEnabled, EmulationMachineTab.Audio, SettingsConstants.ResourceAudio, SettingsConstants.ResourceAudioEnabled, EmulationSettingsEditor.Toggle, configuration.AudioEnabled ? EmulationSettingConstants.Enabled : EmulationSettingConstants.Disabled), new(SettingsConstants.AudioOutput, EmulationMachineTab.Audio, SettingsConstants.ResourceAudio, SettingsConstants.ResourceAudioOutput, EmulationSettingsEditor.Selection, configuration.Options.GetValueOrDefault(SettingsConstants.AudioOutput, SettingsConstants.DefaultOutput), [new(SettingsConstants.DefaultOutput, SettingsConstants.ResourceDefaultOutput)], ChoiceSource: EmulationSettingsChoiceSource.AudioOutputDevices), AudioSelection(configuration, SettingsConstants.AudioLatency, SettingsConstants.ResourceAudioLatency, SettingsConstants.DefaultLatency, SettingsConstants.Latencies, SettingsConstants.MillisecondsUnit), AudioSelection(configuration, VideoAudioSettingsConstants.AudioVolumeOption, SettingsConstants.ResourceAudioVolume, SettingsConstants.DefaultVolume, SettingsConstants.Volumes, SettingsConstants.PercentUnit)], IsCompact: true);
    private static EmulationSettingsField AudioSelection(MachineConfiguration configuration, string id, string resource, int fallback, IReadOnlyList<int> values, string unit) => new(id, EmulationMachineTab.Audio, SettingsConstants.ResourceAudio, resource, EmulationSettingsEditor.Selection, configuration.Options.GetValueOrDefault(id, fallback.ToString(CultureInfo.InvariantCulture)), values.Select(value => new EmulationSettingsChoice(value.ToString(CultureInfo.InvariantCulture), string.Empty, $"{value.ToString(CultureInfo.InvariantCulture)} {unit}")).ToArray());
}
