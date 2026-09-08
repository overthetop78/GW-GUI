using GWGUI.App.Functions.Emulation.Machine;
using GWGUI.Emulation.Atari.Constants;
using GWGUI.Emulation.Atari.Contracts;
using GWGUI.Emulation.Atari.Enums;
using GWGUI.Emulation.Atari.Functions;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.Tests.Emulation.EmulationContracts;

internal static class RuntimeOptionScenarios
{
    internal static void Atari800HotAndRestartOptions()
    {
        Assert.False(AtariRuntimeOptionFunctions.RequiresRestart(
            AtariEmulator.Atari800, AtariEightBitSettingsConstants.ShowActivityOptionKey));
        Assert.False(AtariRuntimeOptionFunctions.RequiresRestart(
            AtariEmulator.Atari800, AtariEightBitSettingsConstants.ShowSectorOptionKey));
        Assert.False(AtariRuntimeOptionFunctions.RequiresRestart(
            AtariEmulator.Atari800, AtariEightBitSettingsConstants.ShowSpeedOptionKey));
        Assert.True(AtariRuntimeOptionFunctions.RequiresRestart(
            AtariEmulator.Atari800, AtariEightBitSettingsConstants.RealTimeClockOptionKey));
        Assert.True(AtariRuntimeOptionFunctions.RequiresRestart(
            AtariEmulator.Atari800, AtariEightBitSettingsConstants.CassetteBootOptionKey));
        Assert.True(AtariRuntimeOptionFunctions.RequiresRestart(
            AtariEmulator.Atari800, AtariEightBitSettingsConstants.PrinterDeviceOptionKey));
        Assert.True(AtariRuntimeOptionFunctions.RequiresRestart(
            AtariEmulator.Atari800, AtariEightBitSettingsConstants.SerialDeviceOptionKey));

        var settings = AtariSettingsDescriptionFunctions.Create(
            new AtariMachineConfiguration(AtariMachineModel.Atari800));
        var fields = settings.SelectMany(block => block.Fields)
            .ToDictionary(field => field.Id, StringComparer.Ordinal);
        Assert.True(fields[AtariEightBitSettingsConstants.CassetteBootOptionKey].RequiresRestart);
        Assert.True(fields[AtariEightBitSettingsConstants.RealTimeClockOptionKey].RequiresRestart);
        Assert.True(fields[AtariEightBitSettingsConstants.PrinterDeviceOptionKey].RequiresRestart);
        Assert.True(fields[AtariEightBitSettingsConstants.SerialDeviceOptionKey].RequiresRestart);
        Assert.DoesNotContain(AtariEightBitSettingsConstants.SioAccelerationOptionKey, fields.Keys);

        var artifacting = fields[AtariEightBitSettingsConstants.ArtifactingModeOptionKey].Choices!
            .ToDictionary(choice => choice.Id, StringComparer.Ordinal);
        Assert.Equal("Emulation.Value.None",
            artifacting[AtariEightBitSettingsConstants.None].DisplayResourceKey);
        Assert.Equal("Emulation.Atari.Video.Artifacting.BlueBrown1",
            artifacting["blue/brown 1"].DisplayResourceKey);
        Assert.Equal("GTIA", artifacting["GTIA"].InvariantDisplayValue);

        var palettes = fields[AtariEightBitSettingsConstants.ExternalPaletteOptionKey].Choices!
            .ToDictionary(choice => choice.Id, StringComparer.Ordinal);
        Assert.Equal("Emulation.Value.Default", palettes["default"].DisplayResourceKey);
        Assert.Equal("Emulation.Value.Gray", palettes["gray"].DisplayResourceKey);
        Assert.Equal("jakub", palettes["jakub"].InvariantDisplayValue);

        var controllerCompatibility = fields[
            AtariEightBitSettingsConstants.ControllerCompatibilityOptionKey].Choices!
            .ToDictionary(choice => choice.Id, StringComparer.Ordinal);
        Assert.Equal("Emulation.Atari.Controller.DualStick",
            controllerCompatibility[AtariEightBitSettingsConstants.DualStick].DisplayResourceKey);
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
        var expectedFields = new Dictionary<AtariMachineModel, string[]>
        {
            [AtariMachineModel.Atari400] = [AtariSettingsConstants.SystemFirmware],
            [AtariMachineModel.Atari800] = [AtariSettingsConstants.SystemFirmware],
            [AtariMachineModel.Atari800Xl] =
                [AtariSettingsConstants.SystemFirmware, AtariSettingsConstants.BasicFirmware],
            [AtariMachineModel.Atari130Xe] =
                [AtariSettingsConstants.SystemFirmware, AtariSettingsConstants.BasicFirmware],
            [AtariMachineModel.XlXe] =
                [AtariSettingsConstants.SystemFirmware, AtariSettingsConstants.BasicFirmware],
            [AtariMachineModel.Xegs] =
                [AtariSettingsConstants.SystemFirmware, AtariSettingsConstants.BasicFirmware,
                    AtariSettingsConstants.XegsFirmware],
            [AtariMachineModel.Atari5200] = [AtariSettingsConstants.SystemFirmware]
        };
        foreach (var expected in expectedFields)
        {
            var actual = AtariSettingsDescriptionFunctions.Create(new AtariMachineConfiguration(expected.Key))
                .SelectMany(block => block.Fields)
                .Where(field => field.DefaultFolderCategory == EmulationDefaultFolderCategory.Firmware)
                .Select(field => field.Id).ToArray();
            Assert.Equal(expected.Value, actual);
        }

        foreach (var model in new[]
                 {
                     AtariMachineModel.Atari400, AtariMachineModel.Atari800,
                     AtariMachineModel.Atari800Xl, AtariMachineModel.Atari130Xe,
                     AtariMachineModel.XlXe, AtariMachineModel.Xegs, AtariMachineModel.Atari5200
                 })
        {
            var fields = AtariSettingsDescriptionFunctions.Create(new AtariMachineConfiguration(model))
                .SelectMany(block => block.Fields).ToArray();
            Assert.Single(fields, field => field.Id == AtariSettingsConstants.SystemFirmware);
        }

        var basic = new AtariFirmwareConfiguration(AtariFirmwareCategory.AtariBasic, "basic.rom", false);
        var previous = new AtariFirmwareConfiguration(AtariFirmwareCategory.AtariXlOs, "previous.rom", false);
        var xegs = new AtariFirmwareConfiguration(AtariFirmwareCategory.AtariXegsBios, "xegs.rom", false);
        var selected = new AtariFirmwareConfiguration(AtariFirmwareCategory.AtariXlOs, "selected.rom", false);
        var configured = AtariFirmwareSelectionFunctions.ReplaceField(AtariMachineModel.Xegs,
            [basic, previous, xegs], AtariSettingsConstants.SystemFirmware, selected);

        Assert.Contains(basic, configured);
        Assert.Contains(xegs, configured);
        Assert.Contains(selected, configured);
        Assert.DoesNotContain(previous, configured);
        Assert.Single(configured,
            item => AtariFirmwareSelectionFunctions.IsSystemRom(AtariMachineModel.Xegs, item.Category));
    }

    private static EmulationOption Option(string key, bool restart) =>
        new(key, key, null, null, "disabled", "disabled", [], RequiresRestart: restart);
}
