using CommonCoreHostFunctions = GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions;
using System.Globalization;

namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        var memoryKib = new Engine().SelectedAdapter(configuration) is { } memoryAdapter ? memoryAdapter.GetMemoryKib(configuration) : hardware.MainMemoryBytes / 1024m;
        var cpuResource = SettingsConstants.CpuResourcePrefix + hardware.DefaultCpu;
        var blocks = new List<EmulationSettingsBlock>
        {
            Block(EmulationMachineTab.Cpu, SettingsConstants.ResourceCpu,
                new EmulationSettingsField(SettingsConstants.Cpu, EmulationMachineTab.Cpu, EmulationMachineTab.Cpu.ToString(), SettingsConstants.ResourceCpu, EmulationSettingsEditor.Information, cpuResource, [new(cpuResource, cpuResource)]),
                Information(SettingsConstants.CpuFrequency, EmulationMachineTab.Cpu, SettingsConstants.ResourceFrequency, CommonCoreHostFunctions.HardwareSettings.FormatMegahertz(hardware.DefaultCpuFrequencyHz, "0.##"))),
            Block(EmulationMachineTab.Ram, SettingsConstants.ResourceMemory,
                Information(SettingsConstants.MainMemory, EmulationMachineTab.Ram, SettingsConstants.ResourceMemory, $"{memoryKib} KiB")),
            Block(EmulationMachineTab.Video, SettingsConstants.ResourceVideo,
                Information(SettingsConstants.Video + "_controller", EmulationMachineTab.Video, SettingsConstants.ResourceVideoController, "Motorola MC6845"),
                Information(SettingsConstants.Video + "_pixels", EmulationMachineTab.Video, SettingsConstants.ResourceVideoPixels, "Acorn Video ULA"),
                Information(SettingsConstants.Video + "_teletext", EmulationMachineTab.Video, SettingsConstants.ResourceTeletext, "Mullard SAA5050")),
            Audio(configuration)
        };
        if (configuration.Core != Emulator.None && new Engine().Adapter(configuration) is { } settings)
        {
            var nativeBlocks = settings.GetMachineSettingsBlocks(configuration);
            // Un éditeur de capacité remplace l'information fixe, sans supprimer les autres réglages RAM du cœur.
            if (nativeBlocks.SelectMany(block => block.Fields).Any(field => field.Id == SettingsConstants.MainMemory && field.Editor != EmulationSettingsEditor.Information))
                blocks.RemoveAll(block => block.Tab == EmulationMachineTab.Ram && block.Fields.All(field => field.Id == SettingsConstants.MainMemory));
            blocks.InsertRange(BufferConstants.FirstCollectionIndex, nativeBlocks);
        }
        return blocks;
    }

    private static EmulationSettingsBlock Audio(MachineConfiguration configuration) =>
        Block(EmulationMachineTab.Audio, SettingsConstants.ResourceAudio,
            CommonCoreHostFunctions.HardwareSettings.Toggle(SettingsConstants.AudioEnabled, EmulationMachineTab.Audio, EmulationMachineTab.Audio.ToString(), SettingsConstants.ResourceAudioEnabled, configuration.AudioEnabled, null, null),
            CommonCoreHostFunctions.HardwareSettings.AudioOutput(SettingsConstants.AudioOutput, EmulationMachineTab.Audio.ToString(), SettingsConstants.ResourceAudioOutput, configuration.Options.GetValueOrDefault(SettingsConstants.AudioOutput, SettingsConstants.DefaultOutput), [new(SettingsConstants.DefaultOutput, SettingsConstants.ResourceDefaultOutput)], null, null),
            Selection(configuration, SettingsConstants.AudioLatency, SettingsConstants.ResourceAudioLatency, EmulationSettingConstants.DefaultAudioLatencyMilliseconds, EmulationSettingConstants.AudioLatencyMillisecondsChoices, "ms"),
            Selection(configuration, VideoAudioSettingsConstants.AudioVolumeOption, SettingsConstants.ResourceAudioVolume, EmulationSettingConstants.DefaultAudioVolumePercent, EmulationSettingConstants.AudioVolumePercentChoices, "%")) with { IsCompact = true };

    private static EmulationSettingsField Selection(MachineConfiguration configuration, string id, string resource, int defaultValue, IReadOnlyList<int> values, string unit) =>
        CommonCoreHostFunctions.HardwareSettings.AudioSelection(configuration.Options, id, EmulationMachineTab.Audio.ToString(), resource, defaultValue, values, unit, null);


    private static EmulationSettingsField Information(string id, EmulationMachineTab tab, string resource, string value) => new(id, tab, tab.ToString(), resource, EmulationSettingsEditor.Information, value);

    private static EmulationSettingsBlock Block(EmulationMachineTab tab, string resource, params EmulationSettingsField[] fields) => new(resource, tab, resource, fields);
}
