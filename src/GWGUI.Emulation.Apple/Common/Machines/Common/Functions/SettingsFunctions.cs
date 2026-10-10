using System.Globalization;

namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        var blocks = new List<EmulationSettingsBlock>
        {
            Block(EmulationMachineTab.Cpu, SettingsConstants.ResourceCpu,
                Information(SettingsConstants.Cpu, EmulationMachineTab.Cpu, SettingsConstants.ResourceCpu,
                    hardware.DefaultCpu.ToString()),
                Information(SettingsConstants.CpuFrequency, EmulationMachineTab.Cpu, SettingsConstants.ResourceFrequency,
                    $"{hardware.DefaultCpuFrequencyHz / (double)SettingsConstants.Megahertz:0.##} MHz")),
            Block(EmulationMachineTab.Ram, SettingsConstants.ResourceMemory,
                Information(SettingsConstants.MainMemory, EmulationMachineTab.Ram, SettingsConstants.ResourceMemory,
                    $"{hardware.MainMemoryBytes / SettingsConstants.Kibibyte} KiB")),
            Audio(configuration),
            Block(EmulationMachineTab.General, SettingsConstants.ResourceGeneral,
                Folder(SettingsConstants.SharedFolder, SettingsConstants.ResourceSharedFolder, configuration.Folders.Shared),
                Folder(SettingsConstants.FloppyFolder, SettingsConstants.ResourceFloppyFolder, configuration.Folders.Floppies,
                    EmulationDefaultFolderCategory.Floppy),
                Folder(SettingsConstants.HardDiskFolder, SettingsConstants.ResourceHardDiskFolder, configuration.Folders.HardDisks,
                    EmulationDefaultFolderCategory.HardDisk),
                Folder(SettingsConstants.StateFolder, SettingsConstants.ResourceStateFolder, configuration.Folders.States,
                    EmulationDefaultFolderCategory.State),
                Folder(SettingsConstants.CaptureFolder, SettingsConstants.ResourceCaptureFolder, configuration.Folders.Captures,
                    EmulationDefaultFolderCategory.Capture))
        };
        if (configuration.Family == MachineFamily.AppleII) AddAppleII(blocks, configuration);
        else AddMacintosh(blocks, configuration);
        return blocks;
    }

    private static EmulationSettingsBlock Audio(MachineConfiguration configuration) =>
        Block(EmulationMachineTab.Audio, SettingsConstants.ResourceAudio,
            new EmulationSettingsField(SettingsConstants.AudioEnabled, EmulationMachineTab.Audio,
                EmulationMachineTab.Audio.ToString(), SettingsConstants.ResourceAudioEnabled,
                EmulationSettingsEditor.Toggle, configuration.AudioEnabled
                    ? EmulationSettingConstants.Enabled : EmulationSettingConstants.Disabled),
            new EmulationSettingsField(SettingsConstants.AudioOutput, EmulationMachineTab.Audio,
                EmulationMachineTab.Audio.ToString(), SettingsConstants.ResourceAudioOutput,
                EmulationSettingsEditor.Selection, configuration.Options.GetValueOrDefault(SettingsConstants.AudioOutput,
                    SettingsConstants.DefaultOutput),
                [new(SettingsConstants.DefaultOutput, SettingsConstants.ResourceDefaultOutput)],
                ChoiceSource: EmulationSettingsChoiceSource.AudioOutputDevices),
            AudioSelection(configuration, SettingsConstants.AudioLatency, SettingsConstants.ResourceAudioLatency,
                SettingsConstants.DefaultLatency, SettingsConstants.Latencies, "ms"),
            AudioSelection(configuration, VideoAudioSettingsConstants.AudioVolumeOption, SettingsConstants.ResourceAudioVolume,
                SettingsConstants.DefaultVolume, SettingsConstants.Volumes, "%"));

    private static EmulationSettingsField AudioSelection(MachineConfiguration configuration, string id,
        string resource, int fallback, IReadOnlyList<int> values, string unit) =>
        new(id, EmulationMachineTab.Audio, EmulationMachineTab.Audio.ToString(), resource,
            EmulationSettingsEditor.Selection, configuration.Options.GetValueOrDefault(id,
                fallback.ToString(CultureInfo.InvariantCulture)),
            values.Select(value => new EmulationSettingsChoice(value.ToString(CultureInfo.InvariantCulture),
                string.Empty, $"{value} {unit}")).ToArray());

    private static EmulationSettingsField Native(MachineConfiguration configuration, CoreOption option,
        EmulationMachineTab tab)
    {
        var resource = SettingsConstants.NativeOptionResourcePrefix + option.Key;
        var value = configuration.Options.GetValueOrDefault(option.Key, option.DefaultValue);
        var choices = option.Values.Select((choice, index) => new EmulationSettingsChoice(choice.Value,
            string.Format(CultureInfo.InvariantCulture, SettingsConstants.NativeChoiceResourceFormat, resource, index)))
            .ToArray();
        return new(option.Key, tab, tab.ToString(), resource, EmulationSettingsEditor.Selection, value, choices,
            RequiresRestart: RuntimeOptionFunctions.RequiresRestart(configuration.Core, option.Key));
    }

    private static EmulationSettingsField Folder(string id, string resource, string? value,
        EmulationDefaultFolderCategory? category = null) =>
        new(id, EmulationMachineTab.General, EmulationMachineTab.General.ToString(), resource,
            EmulationSettingsEditor.DirectoryPath, value, DefaultFolderCategory: category);

    private static EmulationSettingsField Information(string id, EmulationMachineTab tab, string resource, string value) =>
        new(id, tab, tab.ToString(), resource, EmulationSettingsEditor.Information, value, IsEnabled: false);

    private static EmulationSettingsBlock Block(EmulationMachineTab tab, string resource,
        params EmulationSettingsField[] fields) => new(resource, tab, resource, fields);
}
