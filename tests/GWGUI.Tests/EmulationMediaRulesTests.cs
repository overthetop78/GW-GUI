using GWGUI.Emulation;

namespace GWGUI.Tests;

public sealed class EmulationMediaRulesTests
{
    [Fact]
    public void StructuredSlotsExposeTheirCategoryAndIndex()
    {
        Assert.Equal(EmulationMediaCategory.FloppyDrive, EmulationMediaSlot.Floppy0.Category);
        Assert.Equal(0, EmulationMediaSlot.Floppy0.Index);
        Assert.Equal(EmulationMediaCategory.CompactDiscDrive, EmulationMediaSlot.Cd0.Category);
        Assert.Equal(0, (int)EmulationMediaType.Floppy);
    }

    [Theory]
    [InlineData(EmulationMediaCategory.CartridgeSlot, EmulationMediaType.Cartridge)]
    [InlineData(EmulationMediaCategory.CassetteDrive, EmulationMediaType.Cassette)]
    [InlineData(EmulationMediaCategory.CompactDiscDrive, EmulationMediaType.CompactDisc)]
    [InlineData(EmulationMediaCategory.HardDisk, EmulationMediaType.HardDisk)]
    public void NewAndExistingSlotsAcceptTheirSupportedMedia(EmulationMediaCategory category,
        EmulationMediaType type) =>
        Assert.True(EmulationMediaRules.IsCompatible(new EmulationMediaSlot(category, 0), type));

    [Fact]
    public void ValidationRejectsDuplicateAndIncompatibleSlots()
    {
        var cartridge = new EmulationMedia("game.a26", EmulationMediaSlot.Cartridge0,
            EmulationMediaType.Cartridge, true, true);

        Assert.Throws<ArgumentException>(() => EmulationMediaRules.Validate([cartridge, cartridge]));
        Assert.Throws<ArgumentException>(() => EmulationMediaRules.Validate([
            cartridge with { Slot = EmulationMediaSlot.Cassette0 }
        ]));
    }

    [Fact]
    public void ReplaceKeepsOneMediumPerSlotAndEjectPreservesRemovableMedium()
    {
        var first = new EmulationMedia("first.atr", EmulationMediaSlot.Floppy0,
            EmulationMediaType.Floppy, false, true);
        var second = first with { Path = "second.atr" };

        var replaced = EmulationMediaRules.Replace([first], second);
        var ejected = EmulationMediaRules.Eject(replaced, EmulationMediaSlot.Floppy0);

        Assert.Equal("second.atr", Assert.Single(ejected).Path);
        Assert.False(Assert.Single(ejected).IsInserted);
    }

    [Fact]
    public void ReadOnlyAndNonRemovableRulesAreEnforced()
    {
        Assert.Throws<ArgumentException>(() => EmulationMediaRules.Validate([
            new EmulationMedia("game.j64", EmulationMediaSlot.Cartridge0,
                EmulationMediaType.Cartridge, false, true)
        ]));
        Assert.Throws<InvalidOperationException>(() => EmulationMediaRules.Eject([
            new EmulationMedia("disk.vhd", EmulationMediaSlot.HardDisk0,
                EmulationMediaType.HardDisk, false, true)
        ], EmulationMediaSlot.HardDisk0));
    }

    [Fact]
    public void StructuredMediaRoundTripsThroughProtocolJson()
    {
        var floppy = new EmulationMedia("disk.adf", EmulationMediaSlot.Floppy0,
            EmulationMediaType.Floppy, false, true);
        var cartridge = new EmulationMedia("game.j64", EmulationMediaSlot.Cartridge0,
            EmulationMediaType.Cartridge, true, true);

        Assert.Equal(floppy, Assert.Single(EmulationMediaProtocol.Deserialize(
            EmulationMediaProtocol.Serialize([floppy]))));
        Assert.Equal(cartridge, Assert.Single(EmulationMediaProtocol.Deserialize(
            EmulationMediaProtocol.Serialize([cartridge]))));
    }
}
