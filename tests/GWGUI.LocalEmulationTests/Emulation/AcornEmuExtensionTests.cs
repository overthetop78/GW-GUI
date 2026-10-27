using System.Text.Json;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Enums;
using GWGUI.Emulation.Acorn.Common.Services;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Factories;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.Tests.Emulation;

/// <summary>Configuration des extensions et des deux ROM sans fichier, firmware ni périphérique externe.</summary>
public sealed class AcornEmuExtensionTests
{
    [Theory]
    [InlineData(StorageConstants.Intel8271, false, true, false)]
    [InlineData(StorageConstants.AcornWd1770, false, false, true)]
    [InlineData(StorageConstants.Intel8271, true, true, true)]
    [InlineData(StorageConstants.AcornWd1770, true, true, true)]
    public void ControllerAndSolidiskDetermineWhichRomFieldsCanBeEdited(string controller, bool solidisk, bool intelEnabled, bool wdEnabled)
    {
        var factory = new AcornEmuMachineFactory();
        var configuration = Configuration(controller, solidisk);
        var block = Assert.Single(factory.DescribeSettings(configuration), block => block.Tab == EmulationMachineTab.Extensions);
        Assert.Equal(intelEnabled, Assert.Single(block.Fields, field => field.Id == FloppyExtensionConstants.IntelRom).IsEnabled);
        Assert.Equal(wdEnabled, Assert.Single(block.Fields, field => field.Id == FloppyExtensionConstants.WdRom).IsEnabled);
        Assert.True(Assert.Single(block.Fields, field => field.Id == StorageConstants.ControllerOption).UseRadioButtons);
        Assert.DoesNotContain(factory.DescribeSettings(configuration).Where(block => block.Tab == EmulationMachineTab.Rom).SelectMany(block => block.Fields),
            field => field.Id is FloppyExtensionConstants.IntelRom or FloppyExtensionConstants.WdRom);
    }

    [Fact]
    public void DisabledRomPathSurvivesEditsControllerChangesAndSerialization()
    {
        var engine = new Engine();
        var configuration = Configuration(StorageConstants.Intel8271, true);
        configuration = (MachineConfiguration)engine.ApplySettings(configuration, new Dictionary<string, string?>
        {
            [FloppyExtensionConstants.IntelRom] = "intel.rom",
            [FloppyExtensionConstants.WdRom] = "wd.rom",
            [FloppyExtensionConstants.Solidisk] = bool.FalseString
        });
        configuration = (MachineConfiguration)engine.ApplySettings(configuration, new Dictionary<string, string?>
        {
            [FloppyExtensionConstants.WdRom] = "must-not-replace.rom",
            [StorageConstants.ControllerOption] = StorageConstants.AcornWd1770
        });
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(configuration))!;
        Assert.Equal("intel.rom", restored.Options[FloppyExtensionConstants.IntelRom]);
        Assert.Equal("wd.rom", restored.Options[FloppyExtensionConstants.WdRom]);
        Assert.Equal(FloppyExtensionConstants.WdRom, FloppyExtensionFunctions.ActiveRom(restored));
        Assert.False(FloppyExtensionFunctions.Allows(restored, FloppyExtensionConstants.IntelRom));
    }

    [Theory]
    [InlineData(StorageConstants.Intel8271, FloppyExtensionConstants.IntelRom, FloppyExtensionConstants.WdRom)]
    [InlineData(StorageConstants.AcornWd1770, FloppyExtensionConstants.WdRom, FloppyExtensionConstants.IntelRom)]
    public void LegacySingleRomIsPreservedInItsOriginalControllerField(string controller, string original, string other)
    {
        var engine = new Engine();
        var configuration = Configuration(controller, false) with { Options = new Dictionary<string, string>
        {
            [StorageConstants.ControllerOption] = controller,
            [StorageConstants.DfsFirmware] = "legacy.rom"
        } };
        Assert.Equal("legacy.rom", FloppyExtensionFunctions.RomPath(configuration, original));
        Assert.Empty(FloppyExtensionFunctions.RomPath(configuration, other));
        configuration = (MachineConfiguration)engine.ApplySettings(configuration, new Dictionary<string, string?>
        {
            [original] = "legacy.rom",
            [StorageConstants.ControllerOption] = controller == StorageConstants.Intel8271 ? StorageConstants.AcornWd1770 : StorageConstants.Intel8271
        });
        Assert.Equal("legacy.rom", FloppyExtensionFunctions.RomPath(configuration, original));
        Assert.Empty(FloppyExtensionFunctions.RomPath(configuration, other));
    }

    [Fact]
    public void Known8271FirmwareHasASeparateIncompatibleWdDestination()
    {
        var source = new EmulationFirmwareCandidate("dbbec4d2540a854c120be3194c7566a2b79d153b:DiscFilingSystem", "renamed.rom", "DFS", null,
            EmulationFirmwareCompatibility.PartiallyCompatible, StorageConstants.DfsFirmware);
        var candidates = FloppyExtensionFunctions.Destinations(source);
        Assert.Equal(EmulationFirmwareCompatibility.PartiallyCompatible, Assert.Single(candidates, item => item.DestinationFieldId == FloppyExtensionConstants.IntelRom).Compatibility);
        Assert.Equal(EmulationFirmwareCompatibility.Incompatible, Assert.Single(candidates, item => item.DestinationFieldId == FloppyExtensionConstants.WdRom).Compatibility);
        Assert.All(FloppyExtensionFunctions.Destinations(source with { Id = "unidentified:Application", Compatibility = EmulationFirmwareCompatibility.Unknown }),
            item => Assert.Equal(EmulationFirmwareCompatibility.Unknown, item.Compatibility));
    }

    private static MachineConfiguration Configuration(string controller, bool solidisk) => new(MachineModel.BbcMicroModelA,
        core: Emulator.AcornEmu, options: new Dictionary<string, string>
        {
            [StorageConstants.ControllerOption] = controller,
            [FloppyExtensionConstants.Solidisk] = solidisk.ToString()
        });
}
