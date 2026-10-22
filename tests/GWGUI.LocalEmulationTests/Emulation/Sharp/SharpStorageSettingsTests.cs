using System.IO;
using System.Net.Http;
using System.Text.Json;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.Sharp.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sharp.Common.Machines.Common.Enums;
using GWGUI.Emulation.Sharp.Modules;

namespace GWGUI.Tests.Emulation.Sharp;

public sealed class SharpStorageSettingsTests
{
    [Theory]
    [InlineData(MachineModel.X68000)]
    [InlineData(MachineModel.X68000Xvi)]
    public void HardDiskHasAConstructibleProfileAndPreservesItsMountedPath(MachineModel model)
    {
        using var http = new HttpClient();
        var module = new SharpEmulationModule("unused", "unused", http, "unused");
        var initial = new MachineConfiguration(model, core: Emulator.Px68k,
            folders: new FolderConfiguration(HardDisks: "F:/Retro/HDD/Sharp"));
        var settings = module.DescribeStorageSettings(initial);
        var device = Assert.Single(settings.AvailableDevices, device => device.Slot == EmulationMediaSlot.HardDisk0);
        Assert.Equal(EmulationStorageConfigurationKind.HardDiskDrive, device.ConfigurationKind);
        Assert.Equal(initial.Folders.HardDisks, device.ImageDirectory);
        var format = Assert.Single(device.HardDiskFormats!);
        Assert.Equal(".hdf", format.Extension);
        Assert.Equal("SASI", format.InterfaceName);
        Assert.Equal(256, Assert.Single(format.LogicalSectorSizes));
        Assert.Equal(512L * 1024 * 1024, format.MaximumBytes);
        Assert.Equal(HardDiskPreparation.Blank, Assert.Single(format.SupportedPreparations));
        HardDiskImageValidation.Validate("disk.hdf", 256, format);
        Assert.Throws<ArgumentOutOfRangeException>(() => HardDiskImageValidation.Validate("disk.hdf", 257, format));
        Assert.Throws<ArgumentOutOfRangeException>(() => HardDiskImageValidation.Validate("disk.hdf", format.MaximumBytes + 256, format));
        var changed = Assert.IsType<MachineConfiguration>(module.ApplyStorageSettings(initial, settings with
        {
            ConfiguredSlots = settings.ConfiguredSlots.Append(device.Slot).Distinct().ToArray(),
            MountedMedia = [new("F:/Retro/HDD/Sharp/disk.hdf", device.Slot, device.MediaType, false, true)],
            DeviceSettings = [new(device.Slot, InterfaceId: "sasi")]
        }));
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(changed))!;
        var described = module.DescribeStorageSettings(restored);
        Assert.Equal("F:/Retro/HDD/Sharp/disk.hdf", Assert.Single(described.MountedMedia).Path);
        Assert.Equal("sasi", Assert.Single(described.DeviceSettings!).InterfaceId);
        module.ApplyStorageSettings(restored, described with { MountedMedia = [], DeviceSettings = [new(device.Slot)] });
        Assert.Throws<ArgumentException>(() => module.ApplyStorageSettings(restored, described with { DeviceSettings = [new(device.Slot, InterfaceId: "ide")] }));
    }

    [Fact]
    public void ExistingDefaultSectorValidationRemains512Bytes()
    {
        var format = new HardDiskImageFormat("generic", ".hdf", "SASI", 1024 * 1024, 1024);
        HardDiskImageValidation.Validate("disk.hdf", 512, format);
        Assert.Throws<ArgumentOutOfRangeException>(() => HardDiskImageValidation.Validate("disk.hdf", 256, format));
    }

    [Theory]
    [InlineData(256)]
    [InlineData(768)]
    public void RawWriterUsesTheDeclaredSectorSizeAndPreservesItsOldDefault(int length)
    {
        using var image = new MemoryStream();
        RawHardDiskImageWriter.Write(image, length, stream => { stream.Position = length - 1; stream.WriteByte(42); }, 256);
        Assert.Equal(length, image.Length);
        var bytes = image.ToArray();
        Assert.Equal(42, bytes[^1]);
        Assert.All(bytes.Take(length - 1), value => Assert.Equal(0, value));
        using var incompatible = new MemoryStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => RawHardDiskImageWriter.Write(incompatible, length));
    }
}
