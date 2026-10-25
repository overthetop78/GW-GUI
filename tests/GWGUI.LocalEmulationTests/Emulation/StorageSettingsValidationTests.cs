using GWGUI.Emulation;
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
        Assert.Equal(invalid, EmulationSdk.MediaRules.HasInvalidStorageSlots(settings, [Drive]));
    }

    [Theory]
    [InlineData("disk.img", false)]
    [InlineData("disk.IMG", false)]
    [InlineData("disk.adf", true)]
    [InlineData("disk", true)]
    public void MountValidationUsesTheDeviceExtensionsIgnoringCase(string path, bool invalid)
    {
        Assert.Equal(invalid, EmulationSdk.MediaRules.IsInvalidMountedMedia(Disk with { Path = path }, [Drive], [Drive.Slot]));
    }

    [Fact]
    public void MountValidationRequiresADeviceConfiguredSlotAndMatchingType()
    {
        Assert.True(EmulationSdk.MediaRules.IsInvalidMountedMedia(Disk, [], [Drive.Slot]));
        Assert.True(EmulationSdk.MediaRules.IsInvalidMountedMedia(Disk, [Drive], []));
        Assert.True(EmulationSdk.MediaRules.IsInvalidMountedMedia(Disk with { Type = EmulationMediaType.Cartridge }, [Drive], [Drive.Slot]));
    }

    [Fact]
    public void AmbiguousDeviceDescriptionsKeepTheOriginalSingleOrDefaultFailure()
    {
        Assert.Throws<InvalidOperationException>(() => EmulationSdk.MediaRules.IsInvalidMountedMedia(Disk, [Drive, Drive], [Drive.Slot]));
    }

    [Fact]
    public void DeviceSettingsAndInsertionPoliciesRemainOutsideTheCommonSlotChecks()
    {
        var cartridge = new EmulationMedia("game.img", EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, false, false);
        var device = new EmulationMediaDevice(cartridge.Slot, cartridge.Type, [".img"]);
        var settings = new EmulationStorageSettings([device], [device.Slot], [cartridge], [new(device.Slot)]);
        Assert.False(EmulationSdk.MediaRules.HasInvalidStorageSlots(settings, [device]));
        Assert.False(EmulationSdk.MediaRules.IsInvalidMountedMedia(cartridge, [device], [device.Slot]));
        Assert.Throws<ArgumentException>(() => EmulationSdk.MediaRules.Validate([cartridge]));
    }

    [Fact]
    public void ExtensionRestrictionsPreserveHardwareConstraintsAndOtherStorageData()
    {
        var cartridge = new EmulationMediaDevice(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, [".ROM", ".bin", ".zip"], DisplayLabel: "cartridge");
        var mounted = new EmulationMedia("game.ROM", cartridge.Slot, cartridge.MediaType, true, true);
        var settings = new EmulationStorageSettings([Drive, cartridge], [Drive.Slot, cartridge.Slot], [mounted], [new(cartridge.Slot)]);
        var rules = EmulationSdk.MediaRules;
        Assert.Same(settings, rules.RestrictAcceptedExtensions(settings, EmulationMediaCategory.CartridgeSlot, null));
        var allowed = new HashSet<string>(StringComparer.Ordinal) { "rom", ".ZIP", "not-in-the-hardware-catalog" };
        var filtered = rules.RestrictAcceptedExtensions(settings, EmulationMediaCategory.CartridgeSlot, allowed);
        Assert.Equal(new[] { ".ROM", ".zip" }, filtered.AvailableDevices[1].AcceptedExtensions);
        Assert.Same(Drive, filtered.AvailableDevices[0]);
        Assert.Equal(cartridge with { AcceptedExtensions = filtered.AvailableDevices[1].AcceptedExtensions }, filtered.AvailableDevices[1]);
        Assert.Same(settings.ConfiguredSlots, filtered.ConfiguredSlots);
        Assert.Same(settings.MountedMedia, filtered.MountedMedia);
        Assert.Same(settings.DeviceSettings, filtered.DeviceSettings);
        Assert.Equal(new[] { ".ROM", ".bin", ".zip" }, cartridge.AcceptedExtensions);
        Assert.Equal(3, allowed.Count);
        Assert.Same(filtered, rules.RestrictAcceptedExtensions(filtered, EmulationMediaCategory.CartridgeSlot, allowed));
        var empty = rules.RestrictAcceptedExtensions(settings, EmulationMediaCategory.CartridgeSlot, new HashSet<string>());
        Assert.Empty(empty.AvailableDevices[1].AcceptedExtensions);
        Assert.Equal(2, empty.AvailableDevices.Count);
        Assert.Same(Drive, empty.AvailableDevices[0]);
        Assert.True(rules.IsInvalidMountedMedia(mounted, empty.AvailableDevices, empty.ConfiguredSlots));
    }
}
