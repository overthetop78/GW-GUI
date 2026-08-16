using GWGUI.Emulation;

namespace GWGUI.Tests;

public sealed class EmulationMediaRulesTests
{
    [Fact]
    public void ExistingPersistedEnumValuesRemainStable()
    {
        Assert.Equal(0, (int)EmulationMediaSlot.Floppy0);
        Assert.Equal(5, (int)EmulationMediaSlot.Cd0);
        Assert.Equal(0, (int)EmulationMediaType.Floppy);
        Assert.Equal(3, (int)EmulationMediaType.Directory);
    }

    [Theory]
    [InlineData(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge)]
    [InlineData(EmulationMediaSlot.Cassette0, EmulationMediaType.Cassette)]
    [InlineData(EmulationMediaSlot.Cd0, EmulationMediaType.CompactDisc)]
    [InlineData(EmulationMediaSlot.HardDisk0, EmulationMediaType.Directory)]
    public void NewAndExistingSlotsAcceptTheirSupportedMedia(EmulationMediaSlot slot, EmulationMediaType type) =>
        Assert.True(EmulationMediaRules.IsCompatible(slot, type));

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
    public void LegacyAndNewMediaRoundTripThroughProtocolJson()
    {
        const string legacy = "[{\"path\":\"disk.adf\",\"slot\":0,\"type\":0,\"isReadOnly\":false,\"isInserted\":true}]";
        var oldMedia = Assert.Single(EmulationMediaProtocol.Deserialize(System.Text.Encoding.UTF8.GetBytes(legacy)));
        var cartridge = new EmulationMedia("game.j64", EmulationMediaSlot.Cartridge0,
            EmulationMediaType.Cartridge, true, true);

        Assert.Equal(EmulationMediaSlot.Floppy0, oldMedia.Slot);
        Assert.Equal(EmulationMediaType.Floppy, oldMedia.Type);
        Assert.Equal(cartridge, Assert.Single(EmulationMediaProtocol.Deserialize(
            EmulationMediaProtocol.Serialize([cartridge]))));
    }
}
