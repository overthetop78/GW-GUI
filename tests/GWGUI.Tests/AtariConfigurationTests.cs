using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.Tests;

public sealed class AtariConfigurationTests
{
    public static TheoryData<AtariMachineModel, AtariCoreKind, AtariMachineFamily> ModelMappings => new()
    {
        { AtariMachineModel.St, AtariCoreKind.Hatari, AtariMachineFamily.St },
        { AtariMachineModel.Stf, AtariCoreKind.Hatari, AtariMachineFamily.St },
        { AtariMachineModel.Stfm, AtariCoreKind.Hatari, AtariMachineFamily.St },
        { AtariMachineModel.MegaSt, AtariCoreKind.Hatari, AtariMachineFamily.St },
        { AtariMachineModel.Ste, AtariCoreKind.Hatari, AtariMachineFamily.St },
        { AtariMachineModel.MegaSte, AtariCoreKind.Hatari, AtariMachineFamily.St },
        { AtariMachineModel.Tt, AtariCoreKind.Hatari, AtariMachineFamily.St },
        { AtariMachineModel.Falcon, AtariCoreKind.Hatari, AtariMachineFamily.St },
        { AtariMachineModel.Atari400, AtariCoreKind.Atari800, AtariMachineFamily.EightBit },
        { AtariMachineModel.Atari800, AtariCoreKind.Atari800, AtariMachineFamily.EightBit },
        { AtariMachineModel.Atari800Xl, AtariCoreKind.Atari800, AtariMachineFamily.EightBit },
        { AtariMachineModel.Atari130Xe, AtariCoreKind.Atari800, AtariMachineFamily.EightBit },
        { AtariMachineModel.XlXe, AtariCoreKind.Atari800, AtariMachineFamily.EightBit },
        { AtariMachineModel.Xegs, AtariCoreKind.Atari800, AtariMachineFamily.EightBit },
        { AtariMachineModel.Atari5200, AtariCoreKind.Atari800, AtariMachineFamily.Atari5200 },
        { AtariMachineModel.Atari2600, AtariCoreKind.Stella, AtariMachineFamily.Atari2600 },
        { AtariMachineModel.Atari7800, AtariCoreKind.ProSystem, AtariMachineFamily.Atari7800 },
        { AtariMachineModel.Lynx, AtariCoreKind.BeetleLynx, AtariMachineFamily.Lynx },
        { AtariMachineModel.Jaguar, AtariCoreKind.VirtualJaguar, AtariMachineFamily.Jaguar },
        { AtariMachineModel.JaguarCd, AtariCoreKind.VirtualJaguar, AtariMachineFamily.Jaguar }
    };

    [Theory]
    [MemberData(nameof(ModelMappings))]
    public void Model_DeterminesCoreAndFamily(AtariMachineModel model, AtariCoreKind core, AtariMachineFamily family)
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
            Enum.GetNames<AtariCoreKind>());
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
            new("one.st", AtariMediaKind.Floppy, EmulationMediaSlot.Floppy0),
            new("two.st", AtariMediaKind.Floppy, EmulationMediaSlot.Floppy0)
        ];

        Assert.Throws<ArgumentException>(() => new AtariMachineConfiguration(AtariMachineModel.St, media: media));
    }

    [Fact]
    public void ManifestlyIncompatibleFirmware_IsRejected()
    {
        var firmware = new AtariFirmwareConfiguration(AtariFirmwareKind.Tos, "tos.img", true);

        Assert.Throws<ArgumentException>(() => new AtariMachineConfiguration(
            AtariMachineModel.Atari2600, firmwares: [firmware]));
    }

    [Theory]
    [InlineData(AtariMachineModel.Atari2600, AtariMediaKind.Floppy, EmulationMediaSlot.Floppy0)]
    [InlineData(AtariMachineModel.Lynx, AtariMediaKind.CompactDisc, EmulationMediaSlot.Cd0)]
    [InlineData(AtariMachineModel.Jaguar, AtariMediaKind.CompactDisc, EmulationMediaSlot.Cd0)]
    [InlineData(AtariMachineModel.St, AtariMediaKind.Cartridge, EmulationMediaSlot.Cartridge0)]
    public void ManifestlyIncompatibleMedia_IsRejected(AtariMachineModel model, AtariMediaKind kind,
        EmulationMediaSlot slot)
    {
        var media = new AtariMediaConfiguration("content.bin", kind, slot);

        Assert.Throws<ArgumentException>(() => new AtariMachineConfiguration(model, media: [media]));
    }

    [Fact]
    public void JaguarCd_AcceptsCdSlot()
    {
        var compactDisc = new AtariMediaConfiguration(
            "game.cue", AtariMediaKind.CompactDisc, EmulationMediaSlot.Cd0);

        var configuration = new AtariMachineConfiguration(
            AtariMachineModel.JaguarCd, media: [compactDisc]);

        Assert.Single(configuration.Media);
    }

    [Fact]
    public void InvalidAndDuplicateControllerPorts_AreRejected()
    {
        var invalid = new AtariInputConfiguration(Controllers:
            [new AtariControllerBinding(AtariConstants.MaximumControllerPortCount, AtariPeripheralKind.Joystick)]);
        var duplicate = new AtariInputConfiguration(Controllers:
        [
            new AtariControllerBinding(0, AtariPeripheralKind.Joystick),
            new AtariControllerBinding(0, AtariPeripheralKind.Keyboard)
        ]);

        Assert.Throws<ArgumentOutOfRangeException>(() => new AtariMachineConfiguration(AtariMachineModel.Atari800, input: invalid));
        Assert.Throws<ArgumentException>(() => new AtariMachineConfiguration(AtariMachineModel.Atari800, input: duplicate));
    }

    [Fact]
    public void StructuredError_PreservesKindCodeAndContext()
    {
        var context = new Dictionary<string, string> { ["core"] = "Hatari" };
        var error = new AtariEmulationException(AtariErrorKind.Core, AtariErrorCode.CoreRejected,
            "Rejected", context);

        Assert.Equal(AtariErrorKind.Core, error.Kind);
        Assert.Equal(AtariErrorCode.CoreRejected, error.Code);
        Assert.Equal("Hatari", error.Context["core"]);
    }
}
