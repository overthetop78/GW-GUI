using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.Tests;

public sealed class AtariConfigurationTests
{
    public static TheoryData<AtariMachineModel, AtariEmulator, AtariMachineFamily> ModelMappings => new()
    {
        { AtariMachineModel.St, AtariEmulator.Hatari, AtariMachineFamily.St },
        { AtariMachineModel.Stf, AtariEmulator.Hatari, AtariMachineFamily.St },
        { AtariMachineModel.Stfm, AtariEmulator.Hatari, AtariMachineFamily.St },
        { AtariMachineModel.MegaSt, AtariEmulator.Hatari, AtariMachineFamily.St },
        { AtariMachineModel.Ste, AtariEmulator.Hatari, AtariMachineFamily.St },
        { AtariMachineModel.MegaSte, AtariEmulator.Hatari, AtariMachineFamily.St },
        { AtariMachineModel.Tt, AtariEmulator.Hatari, AtariMachineFamily.St },
        { AtariMachineModel.Falcon, AtariEmulator.Hatari, AtariMachineFamily.St },
        { AtariMachineModel.Atari400, AtariEmulator.Atari800, AtariMachineFamily.EightBit },
        { AtariMachineModel.Atari800, AtariEmulator.Atari800, AtariMachineFamily.EightBit },
        { AtariMachineModel.Atari800Xl, AtariEmulator.Atari800, AtariMachineFamily.EightBit },
        { AtariMachineModel.Atari130Xe, AtariEmulator.Atari800, AtariMachineFamily.EightBit },
        { AtariMachineModel.XlXe, AtariEmulator.Atari800, AtariMachineFamily.EightBit },
        { AtariMachineModel.Xegs, AtariEmulator.Atari800, AtariMachineFamily.EightBit },
        { AtariMachineModel.Atari5200, AtariEmulator.Atari800, AtariMachineFamily.Atari5200 },
        { AtariMachineModel.Atari2600, AtariEmulator.Stella, AtariMachineFamily.Atari2600 },
        { AtariMachineModel.Atari7800, AtariEmulator.ProSystem, AtariMachineFamily.Atari7800 },
        { AtariMachineModel.Lynx, AtariEmulator.BeetleLynx, AtariMachineFamily.Lynx },
        { AtariMachineModel.Jaguar, AtariEmulator.VirtualJaguar, AtariMachineFamily.Jaguar },
        { AtariMachineModel.JaguarCd, AtariEmulator.VirtualJaguar, AtariMachineFamily.Jaguar }
    };

    [Theory]
    [MemberData(nameof(ModelMappings))]
    public void Model_DeterminesCoreAndFamily(AtariMachineModel model, AtariEmulator core, AtariMachineFamily family)
    {
        var configuration = new AtariMachineConfiguration(model);

        Assert.Equal(core, configuration.Core);
        Assert.Equal(family, configuration.Family);
        Assert.NotEqual(Guid.Empty, configuration.Id);
    }

    [Fact]
    public void SixCoreIdentifiers_AreExplicitAndStable()
    {
        Assert.Equal(
            ["Hatari", "Atari800", "Stella", "ProSystem", "BeetleLynx", "VirtualJaguar"],
            Enum.GetNames<AtariEmulator>());
    }

    [Fact]
    public void UnsupportedSchema_IsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new AtariMachineConfiguration(
            AtariMachineModel.Atari2600, schemaVersion: AtariConstants.CurrentConfigurationSchemaVersion + 1));
    }

    [Fact]
    public void DuplicateMediaSlot_IsRejected()
    {
        AtariMediaConfiguration[] media =
        [
            new("one.st", AtariMediaCategory.Floppy, EmulationMediaSlot.Floppy0),
            new("two.st", AtariMediaCategory.Floppy, EmulationMediaSlot.Floppy0)
        ];

        Assert.Throws<ArgumentException>(() => new AtariMachineConfiguration(AtariMachineModel.St, media: media));
    }

    [Fact]
    public void ManifestlyIncompatibleFirmware_IsRejected()
    {
        var firmware = new AtariFirmwareConfiguration(AtariFirmwareCategory.Tos, "tos.img", true);

        Assert.Throws<ArgumentException>(() => new AtariMachineConfiguration(
            AtariMachineModel.Atari2600, firmwares: [firmware]));
    }

    public static TheoryData<AtariMachineModel, AtariMediaCategory, EmulationMediaSlot> IncompatibleMedia => new()
    {
        { AtariMachineModel.Atari2600, AtariMediaCategory.Floppy, EmulationMediaSlot.Floppy0 },
        { AtariMachineModel.Lynx, AtariMediaCategory.CompactDisc, EmulationMediaSlot.Cd0 },
        { AtariMachineModel.Jaguar, AtariMediaCategory.CompactDisc, EmulationMediaSlot.Cd0 },
        { AtariMachineModel.St, AtariMediaCategory.Cartridge, EmulationMediaSlot.Cartridge0 }
    };

    [Theory]
    [MemberData(nameof(IncompatibleMedia))]
    public void ManifestlyIncompatibleMedia_IsRejected(AtariMachineModel model, AtariMediaCategory kind,
        EmulationMediaSlot slot)
    {
        var media = new AtariMediaConfiguration("content.bin", kind, slot);

        Assert.Throws<ArgumentException>(() => new AtariMachineConfiguration(model, media: [media]));
    }

    [Fact]
    public void JaguarCd_AcceptsCdSlot()
    {
        var compactDisc = new AtariMediaConfiguration(
            "game.cue", AtariMediaCategory.CompactDisc, EmulationMediaSlot.Cd0);

        var configuration = new AtariMachineConfiguration(
            AtariMachineModel.JaguarCd, media: [compactDisc]);

        Assert.Single(configuration.Media);
    }

    [Fact]
    public void InvalidAndDuplicateControllerPorts_AreRejected()
    {
        var invalid = new AtariInputConfiguration(Controllers:
            [new AtariControllerBinding(AtariConstants.MaximumControllerPortCount, AtariPeripheralCategory.Joystick)]);
        var duplicate = new AtariInputConfiguration(Controllers:
        [
            new AtariControllerBinding(0, AtariPeripheralCategory.Joystick),
            new AtariControllerBinding(0, AtariPeripheralCategory.Keyboard)
        ]);

        Assert.Throws<ArgumentOutOfRangeException>(() => new AtariMachineConfiguration(AtariMachineModel.Atari800, input: invalid));
        Assert.Throws<ArgumentException>(() => new AtariMachineConfiguration(AtariMachineModel.Atari800, input: duplicate));
    }

    [Fact]
    public void StructuredError_PreservesKindCodeAndContext()
    {
        var context = new Dictionary<string, string> { ["core"] = "Hatari" };
        var error = new AtariEmulationException(AtariErrorCategory.Core, AtariErrorCode.CoreRejected,
            "Rejected", context);

        Assert.Equal(AtariErrorCategory.Core, error.Category);
        Assert.Equal(AtariErrorCode.CoreRejected, error.Code);
        Assert.Equal("Hatari", error.Context["core"]);
    }
}
