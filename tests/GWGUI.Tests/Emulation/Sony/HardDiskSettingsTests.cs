using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.Sony.Common.Machines.Common.Constants;
using GWGUI.Emulation.Sony.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sony.Common.Machines.Common.Functions;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;
using GWGUI.Emulation.Services;
using System.Reflection;
using System.Text.Json;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class HardDiskSettingsTests
{
    [Fact]
    public void ExistingModuleBinariesRetainTheirVirtualFileSystemConstructor() => Assert.NotNull(typeof(ExternalCoreVirtualFileSystem)
        .GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, binder: null, types: [typeof(Action<string, long>)], modifiers: null));

    [Theory]
    [InlineData(8, true)]
    [InlineData(40, true)]
    [InlineData(120, true)]
    [InlineData(7, false)]
    [InlineData(32, false)]
    [InlineData(121, false)]
    public void Dev9AcceptsOnlyCapacitiesTheNativeAtaDeviceCanAdvertise(int gibibytes, bool supported)
    {
        var storage = StorageSettingsFunctions.Describe(new MachineConfiguration(ModelConstants.PlayStation2, "pcsx2"));
        var disk = Assert.Single(storage.AvailableDevices, device => device.MediaType == EmulationMediaType.HardDisk);
        Assert.False(disk.IsRemovable);
        Assert.True(disk.RequiresMachineRecreation);
        Assert.Equal(EmulationStorageConfigurationKind.HardDiskDrive, disk.ConfigurationKind);
        Assert.All(disk.HardDiskFormats!, format =>
        {
            Assert.Equal(new[] { HardDiskConstants.SectorBytes }, format.LogicalSectorSizes);
            Assert.Equal(supported, HardDiskImageValidation.IsSupportedCapacity(gibibytes * 1024L * 1024 * 1024, format));
            Assert.False(HardDiskImageValidation.IsSupportedCapacity(format.DefaultBytes + 512, format));
            Assert.Equal(new[] { HardDiskPreparation.Blank }, format.SupportedPreparations);
        });
    }

    [Theory]
    [InlineData(ModelConstants.PlayStation, "swanstation")]
    [InlineData(ModelConstants.PlayStation2, "pcee2")]
    [InlineData(ModelConstants.PlayStation2, "play")]
    public void OtherAdaptersDoNotInheritDev9Storage(string model, string core) => Assert.DoesNotContain(
        StorageSettingsFunctions.Describe(new MachineConfiguration(model, core)).AvailableDevices, device => device.MediaType == EmulationMediaType.HardDisk);

    [Fact]
    public void EmptyDev9DriveCanBeAddedReloadedConfiguredAndRemoved()
    {
        var configuration = new MachineConfiguration(ModelConstants.PlayStation2, "pcsx2");
        var initial = StorageSettingsFunctions.Describe(configuration);
        Assert.DoesNotContain(EmulationMediaSlot.HardDisk0, initial.ConfiguredSlots);
        var added = StorageSettingsFunctions.Apply(configuration, initial with
        {
            ConfiguredSlots = initial.ConfiguredSlots.Append(EmulationMediaSlot.HardDisk0).ToArray()
        });
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(added, jsonOptions), jsonOptions)!;
        var reloaded = StorageSettingsFunctions.Describe(restored);
        Assert.Contains(EmulationMediaSlot.HardDisk0, reloaded.ConfiguredSlots);
        Assert.Empty(reloaded.MountedMedia);
        Assert.Equal(EmulationStorageConfigurationKind.HardDiskDrive,
            Assert.Single(reloaded.AvailableDevices, device => device.Slot == EmulationMediaSlot.HardDisk0).ConfigurationKind);
        var native = GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions.HardDiskRuntimeFunctions.Configure(restored);
        Assert.Equal("disabled", native.Options![OptionConstants.Dev9HddKey]);
        var removed = StorageSettingsFunctions.Apply(restored, reloaded with
        {
            ConfiguredSlots = reloaded.ConfiguredSlots.Where(slot => slot != EmulationMediaSlot.HardDisk0).ToArray()
        });
        Assert.DoesNotContain(EmulationMediaSlot.HardDisk0, StorageSettingsFunctions.Describe(removed).ConfiguredSlots);
        Assert.Single(StorageSettingsFunctions.Describe(removed).AvailableDevices, device => device.Slot == EmulationMediaSlot.HardDisk0);
    }

    [Fact]
    public void FormatsWithoutDiscreteCapacitiesKeepSectorAlignedSizes()
    {
        var format = new HardDiskImageFormat("raw", ".raw", "ATA", 64L * 1024 * 1024 * 1024, 32L * 1024 * 1024 * 1024);
        Assert.True(HardDiskImageValidation.IsSupportedCapacity(format.DefaultBytes, format));
        Assert.True(HardDiskImageValidation.IsSupportedCapacity(format.DefaultBytes + 512, format));
        Assert.False(HardDiskImageValidation.IsSupportedCapacity(format.DefaultBytes + 1, format));
    }
}
