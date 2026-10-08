using System.IO;
using System.Globalization;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(Model model, MachineConfiguration configuration)
    {

        var firmware = EmulatorCatalog.CreateAdapter(configuration.Core).GetFirmwareSlots(configuration);
        var blocks = EmulatorCatalog.CreateAdapter(configuration.Core).GetSettingsBlocks(model, configuration).ToList();
        if (firmware.Count != BufferConstants.EmptyCollectionCount)
            blocks.Add(new EmulationSettingsBlock(SettingsBlocks.Firmware, EmulationMachineTab.Rom,
                SettingsResourceKeys.FirmwareRomSystem, firmware.Select(slot =>
                    new EmulationSettingsField(slot.FieldId, EmulationMachineTab.Rom, SettingsBlocks.Firmware,
                        slot.ResourceKey, EmulationSettingsEditor.Path, configuration.FirmwarePath(slot.FieldId),
                        DefaultFolderCategory: EmulationDefaultFolderCategory.Firmware,
                        ExplanationResourceKey: slot.ExplanationResourceKey,
                        DetailedExplanationResourceKey: slot.DetailedExplanationResourceKey)).ToArray(),
                SettingsGlyphs.Firmware, MachineSettingsConstants.InformationColumnCount));
        if (EmulatorCatalog.CreateAdapter(configuration.Core).GetMediaDevices(configuration)
            .Any(device => device.MediaType == EmulationMediaType.HardDisk))
            blocks.Add(new EmulationSettingsBlock(StorageDirectorySettings.BlockId, EmulationMachineTab.General,
                StorageDirectorySettings.TitleResource,
                [new EmulationSettingsField(StorageDirectorySettings.HardDiskDirectory, EmulationMachineTab.General,
                    StorageDirectorySettings.BlockId, StorageDirectorySettings.HardDiskResource,
                    EmulationSettingsEditor.DirectoryPath, configuration.HardDiskDirectory ?? string.Empty,
                    DefaultFolderCategory: EmulationDefaultFolderCategory.HardDisk,
                    ExplanationResourceKey: StorageDirectorySettings.ShortHelp,
                    DetailedExplanationResourceKey: StorageDirectorySettings.DetailedHelp)],
                StorageDirectorySettings.Icon, MachineSettingsConstants.InformationColumnCount));
        return blocks;
    }

    internal static IReadOnlyList<EmulationSettingsBlock> CreateBasic(Model model, MachineConfiguration configuration)
    {
        var blocks = new List<EmulationSettingsBlock>
        {
            Information(SettingsBlocks.Cpu, EmulationMachineTab.Cpu, SettingsResourceKeys.CpuProcessor,
                SettingsGlyphs.Processor, MachineSettingsConstants.ProcessorField,
                string.Join(MachineSettingsConstants.ProcessorSeparator, model.CpuModels.Select(CpuDisplayFunctions.DisplayName))),
            Information(SettingsBlocks.MainMemory, EmulationMachineTab.Ram, SettingsResourceKeys.MemoryMain,
                SettingsGlyphs.Memory, MachineSettingsConstants.RamField,
                string.Format(CultureInfo.InvariantCulture, MachineSettingsConstants.RamKibFormat, model.RamKib)),
            Information(SettingsBlocks.Display, EmulationMachineTab.Video, SettingsResourceKeys.VideoSettingsDisplay,
                SettingsGlyphs.Video, MachineSettingsConstants.ChipsetField, model.Chipset)
        };
        blocks.Add(new EmulationSettingsBlock(SettingsBlocks.Audio, EmulationMachineTab.Audio,
            SettingsResourceKeys.Audio,
            [new EmulationSettingsField(MachineSettingsConstants.AudioEnabled, EmulationMachineTab.Audio,
                SettingsBlocks.Audio, SettingsResourceKeys.AudioEnabled, EmulationSettingsEditor.Toggle,
                configuration.AudioEnabled ? MachineSettingsConstants.Enabled : MachineSettingsConstants.Disabled),
             new EmulationSettingsField(MachineSettingsConstants.AudioOutput, EmulationMachineTab.Audio,
                SettingsBlocks.Audio, SettingsResourceKeys.AudioDevice, EmulationSettingsEditor.Selection,
                configuration.Audio?.OutputDeviceId ?? string.Empty,
                [new EmulationSettingsChoice(string.Empty, SettingsResourceKeys.AudioDefaultOutput)],
                ChoiceSource: EmulationSettingsChoiceSource.AudioOutputDevices)],
            SettingsGlyphs.Audio, MachineSettingsConstants.InformationColumnCount));
        return blocks;
    }

    private static EmulationSettingsBlock Information(string id, EmulationMachineTab tab, string label,
        string glyph, string fieldId, string value) => new(id, tab, label,
        [new EmulationSettingsField(fieldId, tab, id, label, EmulationSettingsEditor.Information, value)],
        glyph, MachineSettingsConstants.InformationColumnCount);
}
