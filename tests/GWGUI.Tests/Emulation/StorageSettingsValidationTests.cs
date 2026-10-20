using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class StorageSettingsValidationTests
{
    private static readonly EmulationMediaDevice Drive = new(EmulationMediaSlot.Floppy0, EmulationMediaType.Floppy, [".img"]);
    private static readonly EmulationMedia Disk = new("disk.img", EmulationMediaSlot.Floppy0, EmulationMediaType.Floppy, false, true);

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    public void SlotValidationRejectsDuplicateUnknownAndRepeatedMountSlots(int scenario, bool invalid)
    {
        var slots = scenario switch
        {
            1 => new[] { Drive.Slot, Drive.Slot },
            2 => new[] { EmulationMediaSlot.Cd0 },
            _ => new[] { Drive.Slot }
        };
        EmulationMedia[] mounted = scenario == 3 ? [Disk, Disk] : [Disk];
        var settings = new EmulationStorageSettings([Drive], slots, mounted);
        Assert.Equal(invalid, EmulationMediaRules.HasInvalidStorageSlots(settings, [Drive]));
    }

    [Theory]
    [InlineData("disk.img", false)]
    [InlineData("disk.IMG", false)]
    [InlineData("disk.adf", true)]
    [InlineData("disk", true)]
    public void MountValidationUsesTheDeviceExtensionsIgnoringCase(string path, bool invalid)
    {
        Assert.Equal(invalid, EmulationMediaRules.IsInvalidMountedMedia(Disk with { Path = path }, [Drive], [Drive.Slot]));
    }

    [Fact]
    public void MountValidationRequiresADeviceConfiguredSlotAndMatchingType()
    {
        Assert.True(EmulationMediaRules.IsInvalidMountedMedia(Disk, [], [Drive.Slot]));
        Assert.True(EmulationMediaRules.IsInvalidMountedMedia(Disk, [Drive], []));
        Assert.True(EmulationMediaRules.IsInvalidMountedMedia(Disk with { Type = EmulationMediaType.Cartridge }, [Drive], [Drive.Slot]));
    }

    [Fact]
    public void AmbiguousDeviceDescriptionsKeepTheOriginalSingleOrDefaultFailure()
    {
        Assert.Throws<InvalidOperationException>(() => EmulationMediaRules.IsInvalidMountedMedia(Disk, [Drive, Drive], [Drive.Slot]));
    }

    [Fact]
    public void DeviceSettingsAndInsertionPoliciesRemainOutsideTheCommonSlotChecks()
    {
        var cartridge = new EmulationMedia("game.img", EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, false, false);
        var device = new EmulationMediaDevice(cartridge.Slot, cartridge.Type, [".img"]);
        var settings = new EmulationStorageSettings([device], [device.Slot], [cartridge], [new(device.Slot)]);
        Assert.False(EmulationMediaRules.HasInvalidStorageSlots(settings, [device]));
        Assert.False(EmulationMediaRules.IsInvalidMountedMedia(cartridge, [device], [device.Slot]));
        Assert.Throws<ArgumentException>(() => EmulationMediaRules.Validate([cartridge]));
    }
}
