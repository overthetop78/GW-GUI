using System.Globalization;
using GWGUI.Emulation.Acorn.Emulators.B2.Constants;
using GWGUI.Emulation.Acorn.Emulators.B2.Functions;

namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        var variants = B2ModelFunctions.GetModels(configuration.Model);
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
            Block(EmulationMachineTab.Rom, SettingsConstants.ResourceFirmware,
                new EmulationSettingsField(SettingsConstants.Firmware, EmulationMachineTab.Rom,
                    EmulationMachineTab.Rom.ToString(), SettingsConstants.ResourceFirmware,
                    EmulationSettingsEditor.Information, null,
                    ExplanationResourceKey: SettingsConstants.ResourceEmbeddedFirmware)),
            Block(EmulationMachineTab.Video, SettingsConstants.ResourceVideo,
                Information(SettingsConstants.Video, EmulationMachineTab.Video, SettingsConstants.ResourceVideo,
                    string.Join(" / ", hardware.Video))),
            Audio(configuration),
            Block(EmulationMachineTab.General, SettingsConstants.ResourceGeneral,
                Folder(SettingsConstants.SharedFolder, SettingsConstants.ResourceSharedFolder, configuration.Folders.Shared),
                Folder(SettingsConstants.FloppyFolder, SettingsConstants.ResourceFloppyFolder, configuration.Folders.Floppies,
                    EmulationDefaultFolderCategory.Floppy),
                Folder(SettingsConstants.StateFolder, SettingsConstants.ResourceStateFolder, configuration.Folders.States,
                    EmulationDefaultFolderCategory.State),
                Folder(SettingsConstants.CaptureFolder, SettingsConstants.ResourceCaptureFolder, configuration.Folders.Captures,
                    EmulationDefaultFolderCategory.Capture))
        };
        if (variants.Count > BufferConstants.EmptyCollectionCount)
            blocks.Insert(BufferConstants.FirstCollectionIndex,
                Block(EmulationMachineTab.General, SettingsConstants.ResourceNativeModel,
                    new EmulationSettingsField(OptionConstants.Model, EmulationMachineTab.General,
                        EmulationMachineTab.General.ToString(), SettingsConstants.ResourceNativeModel,
                        EmulationSettingsEditor.Selection, configuration.Options.GetValueOrDefault(OptionConstants.Model),
                        variants.Select(variant => new EmulationSettingsChoice(variant.NativeValue,
                            string.Empty, variant.NativeValue)).ToArray(), RequiresRestart: true),
                    new EmulationSettingsField(OptionConstants.AutoBoot, EmulationMachineTab.General,
                        EmulationMachineTab.General.ToString(), SettingsConstants.ResourceAutoBoot,
                        EmulationSettingsEditor.Toggle, configuration.Options.GetValueOrDefault(OptionConstants.AutoBoot),
                        EnabledValue: OptionConstants.Enabled, DisabledValue: OptionConstants.Disabled,
                        RequiresRestart: true)));
        else
            blocks.Insert(BufferConstants.FirstCollectionIndex,
                Block(EmulationMachineTab.General, SettingsConstants.ResourceGeneral,
                    new EmulationSettingsField(OptionConstants.Model, EmulationMachineTab.General,
                        EmulationMachineTab.General.ToString(), SettingsConstants.ResourceUnavailableModel,
                        EmulationSettingsEditor.Information, null)));
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
            Selection(configuration, SettingsConstants.AudioLatency, SettingsConstants.ResourceAudioLatency,
                SettingsConstants.DefaultLatency, SettingsConstants.Latencies, "ms"),
            Selection(configuration, VideoAudioSettingsConstants.AudioVolumeOption, SettingsConstants.ResourceAudioVolume,
                SettingsConstants.DefaultVolume, SettingsConstants.Volumes, "%"));

    private static EmulationSettingsField Selection(MachineConfiguration configuration, string id,
        string resource, int defaultValue, IReadOnlyList<int> values, string unit) =>
        new(id, EmulationMachineTab.Audio, EmulationMachineTab.Audio.ToString(), resource,
            EmulationSettingsEditor.Selection, configuration.Options.GetValueOrDefault(id,
                defaultValue.ToString(CultureInfo.InvariantCulture)),
            values.Select(value => new EmulationSettingsChoice(value.ToString(CultureInfo.InvariantCulture),
                string.Empty, $"{value} {unit}")).ToArray());

    private static EmulationSettingsField Folder(string id, string resource, string? value,
        EmulationDefaultFolderCategory? category = null) =>
        new(id, EmulationMachineTab.General, EmulationMachineTab.General.ToString(), resource,
            EmulationSettingsEditor.DirectoryPath, value, DefaultFolderCategory: category);

    private static EmulationSettingsField Information(string id, EmulationMachineTab tab,
        string resource, string value) =>
        new(id, tab, tab.ToString(), resource, EmulationSettingsEditor.Information, value);

    private static EmulationSettingsBlock Block(EmulationMachineTab tab, string resource,
        params EmulationSettingsField[] fields) => new(resource, tab, resource, fields);
}
