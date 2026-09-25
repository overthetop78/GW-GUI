using GWGUI.App.Functions.Emulation.Machine;
using GWGUI.Emulation.Atari.Common.Constants;
using GWGUI.Emulation.Atari.Common.Contracts;
using GWGUI.Emulation.Atari.Common.Dictionaries;
using GWGUI.Emulation.Atari.Common.Enums;
using GWGUI.Emulation.Atari.Common.Machines.Atari8Bit.Constants;
using GWGUI.Emulation.Atari.Common.Functions;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.Tests.Emulation.EmulationContracts;

internal static class RuntimeOptionScenarios
{
    internal static void Atari800HotAndRestartOptions()
    {
        Assert.False(RuntimeOptionFunctions.RequiresRestart(
            Emulator.Atari800, EightBitSettingsConstants.ShowActivityOptionKey));
        Assert.False(RuntimeOptionFunctions.RequiresRestart(
            Emulator.Atari800, EightBitSettingsConstants.ShowSectorOptionKey));
        Assert.False(RuntimeOptionFunctions.RequiresRestart(
            Emulator.Atari800, EightBitSettingsConstants.ShowSpeedOptionKey));
        Assert.True(RuntimeOptionFunctions.RequiresRestart(
            Emulator.Atari800, EightBitSettingsConstants.RealTimeClockOptionKey));
        Assert.True(RuntimeOptionFunctions.RequiresRestart(
            Emulator.Atari800, EightBitSettingsConstants.CassetteBootOptionKey));
        Assert.True(RuntimeOptionFunctions.RequiresRestart(
            Emulator.Atari800, EightBitSettingsConstants.PrinterDeviceOptionKey));
        Assert.True(RuntimeOptionFunctions.RequiresRestart(
            Emulator.Atari800, EightBitSettingsConstants.SerialDeviceOptionKey));

        var settings = SettingsDescriptionFunctions.Create(
            new MachineConfiguration(MachineModel.Atari800));
        var fields = settings.SelectMany(block => block.Fields)
            .ToDictionary(field => field.Id, StringComparer.Ordinal);
        Assert.True(fields[EightBitSettingsConstants.CassetteBootOptionKey].RequiresRestart);
        Assert.True(fields[EightBitSettingsConstants.RealTimeClockOptionKey].RequiresRestart);
        Assert.True(fields[EightBitSettingsConstants.PrinterDeviceOptionKey].RequiresRestart);
        Assert.True(fields[EightBitSettingsConstants.SerialDeviceOptionKey].RequiresRestart);
        Assert.DoesNotContain(EightBitSettingsConstants.SioAccelerationOptionKey, fields.Keys);

        var artifacting = fields[EightBitSettingsConstants.ArtifactingModeOptionKey].Choices!
            .ToDictionary(choice => choice.Id, StringComparer.Ordinal);
        Assert.Equal("Emulation.Value.None",
            artifacting[EightBitSettingsConstants.None].DisplayResourceKey);
        Assert.Equal("Emulation.Atari.Video.Artifacting.BlueBrown1",
            artifacting["blue/brown 1"].DisplayResourceKey);
        Assert.Equal("GTIA", artifacting["GTIA"].InvariantDisplayValue);

        var palettes = fields[EightBitSettingsConstants.ExternalPaletteOptionKey].Choices!
            .ToDictionary(choice => choice.Id, StringComparer.Ordinal);
        Assert.Equal("Emulation.Value.Default", palettes["default"].DisplayResourceKey);
        Assert.Equal("Emulation.Value.Gray", palettes["gray"].DisplayResourceKey);
        Assert.Equal("jakub", palettes["jakub"].InvariantDisplayValue);

        var controllerCompatibility = fields[
            EightBitSettingsConstants.ControllerCompatibilityOptionKey].Choices!
            .ToDictionary(choice => choice.Id, StringComparer.Ordinal);
        Assert.Equal("Emulation.Atari.Controller.DualStick",
            controllerCompatibility[EightBitSettingsConstants.DualStick].DisplayResourceKey);
    }

    internal static void OnlyRecognizedHotOptionsArePropagated()
    {
        var available = new[]
        {
            Option("hot", false),
            Option("restart", true)
        };
        var changes = EmulationRuntimeOptionFunctions.HotChanges(
            new Dictionary<string, string>
            {
                ["hot"] = "enabled",
                ["restart"] = "enabled",
                ["unknown"] = "enabled"
            }, available);

        Assert.Equal("enabled", Assert.Single(changes).Value);
        Assert.True(changes.ContainsKey("hot"));
    }

    internal static void Atari800CoreMachinesExposeOneSystemRomSlot()
    {
        var expectedFields = new Dictionary<MachineModel, string[]>
        {
            [MachineModel.Atari400] = [SettingsConstants.SystemFirmware],
            [MachineModel.Atari800] = [SettingsConstants.SystemFirmware],
            [MachineModel.Atari800Xl] =
                [SettingsConstants.SystemFirmware, SettingsConstants.BasicFirmware],
            [MachineModel.Atari130Xe] =
                [SettingsConstants.SystemFirmware, SettingsConstants.BasicFirmware],
            [MachineModel.XlXe] =
                [SettingsConstants.SystemFirmware, SettingsConstants.BasicFirmware],
            [MachineModel.Xegs] =
                [SettingsConstants.SystemFirmware, SettingsConstants.BasicFirmware,
                    SettingsConstants.XegsFirmware],
            [MachineModel.Atari5200] = [SettingsConstants.SystemFirmware]
        };
        foreach (var expected in expectedFields)
        {
            var actual = SettingsDescriptionFunctions.Create(new MachineConfiguration(expected.Key))
                .SelectMany(block => block.Fields)
                .Where(field => field.DefaultFolderCategory == EmulationDefaultFolderCategory.Firmware)
                .Select(field => field.Id).ToArray();
            Assert.Equal(expected.Value, actual);
        }

        foreach (var model in new[]
                 {
                     MachineModel.Atari400, MachineModel.Atari800,
                     MachineModel.Atari800Xl, MachineModel.Atari130Xe,
                     MachineModel.XlXe, MachineModel.Xegs, MachineModel.Atari5200
                 })
        {
            var fields = SettingsDescriptionFunctions.Create(new MachineConfiguration(model))
                .SelectMany(block => block.Fields).ToArray();
            Assert.Single(fields, field => field.Id == SettingsConstants.SystemFirmware);
        }

        var basic = new FirmwareConfiguration(FirmwareCategory.AtariBasic, "basic.rom", false);
        var previous = new FirmwareConfiguration(FirmwareCategory.AtariXlOs, "previous.rom", false);
        var xegs = new FirmwareConfiguration(FirmwareCategory.AtariXegsBios, "xegs.rom", false);
        var selected = new FirmwareConfiguration(FirmwareCategory.AtariXlOs, "selected.rom", false);
        var configured = FirmwareSelectionFunctions.ReplaceField(MachineModel.Xegs,
            [basic, previous, xegs], SettingsConstants.SystemFirmware, selected);

        Assert.Contains(basic, configured);
        Assert.Contains(xegs, configured);
        Assert.Contains(selected, configured);
        Assert.DoesNotContain(previous, configured);
        Assert.Single(configured,
            item => FirmwareSelectionFunctions.IsSystemRom(MachineModel.Xegs, item.Category));
    }

    internal static void AtariExternalFirmwareModelsExposeOnlyUsableRomTabs()
    {
        foreach (var model in new[]
                 {
                     MachineModel.Atari7800,
                     MachineModel.Lynx,
                     MachineModel.JaguarCd
                 })
        {
            Assert.Contains(SettingsTab.Firmware,
                CompatibilityCatalog.Get(model).VisibleTabs);
            var firmwareFields = SettingsDescriptionFunctions.Create(
                    new MachineConfiguration(model))
                .SelectMany(block => block.Fields)
                .Where(field => field.DefaultFolderCategory == EmulationDefaultFolderCategory.Firmware)
                .ToArray();
            Assert.Equal(SettingsConstants.SystemFirmware, Assert.Single(firmwareFields).Id);
        }

        foreach (var model in new[]
                 {
                     MachineModel.Atari2600,
                     MachineModel.Jaguar
                 })
        {
            Assert.DoesNotContain(SettingsTab.Firmware,
                CompatibilityCatalog.Get(model).VisibleTabs);
            Assert.DoesNotContain(SettingsDescriptionFunctions.Create(
                    new MachineConfiguration(model)).SelectMany(block => block.Fields),
                field => field.DefaultFolderCategory == EmulationDefaultFolderCategory.Firmware);
        }
    }

    private static EmulationOption Option(string key, bool restart) =>
        new(key, key, null, null, "disabled", "disabled", [], RequiresRestart: restart);
}
