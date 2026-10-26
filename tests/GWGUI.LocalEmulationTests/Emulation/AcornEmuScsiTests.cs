using System.Buffers.Binary;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.ModelA.Services;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Factories;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Enums;
using GWGUI.Emulation.Enums;
using System.Text.Json;

namespace GWGUI.Tests.Emulation;

/// <summary>Commandes Winchester et transitions du bus exercées sans fichiers ni ROM externes.</summary>
public sealed class AcornEmuScsiTests
{
    [Fact]
    public void AdfsProbeReadDuringSelectionReleasesTheBus()
    {
        using var disk = new Disk(); var controller = Controller(_ => disk);
        controller.Write(0, 0x5a); Assert.True(controller.Busy);
        Assert.Equal(0x5a, controller.Read(0)); Assert.False(controller.Busy);
        Assert.Equal(0x20, controller.Read(1));
        Assert.Equal(0, Execute(controller, Cdb(0, 0, 0, 0)).Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void EachWinchesterUnitUsesTheCommonConfigurationAndMediaContracts(int unit)
    {
        var factory = new AcornEmuMachineFactory(); var empty = new MachineConfiguration(MachineModel.BbcMicroModelA, core: Emulator.AcornEmu);
        var available = factory.DescribeStorage(empty);
        Assert.Empty(available.ConfiguredSlots); Assert.Equal(4, available.AvailableDevices.Count(device => device.Slot.Category == EmulationMediaCategory.HardDisk));
        var slot = new EmulationMediaSlot(EmulationMediaCategory.HardDisk, unit);
        var settings = available with { ConfiguredSlots = [slot], MountedMedia = [new("unit.dat", slot, EmulationMediaType.HardDisk, false, true)], DeviceSettings = [new(slot, InterfaceId: "scsi", Options: new Dictionary<string, string> { ["protected"] = bool.TrueString, ["speed"] = "200", ["readLatency"] = "3", ["writeLatency"] = "7" })] };
        var configuration = factory.ApplyStorage(empty, settings);
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(configuration))!;
        var stored = factory.DescribeStorage(restored); Assert.Equal(slot, Assert.Single(stored.ConfiguredSlots));
        Assert.Equal(MediaCategory.HardDisk, Assert.Single(restored.Media).Category);
        Assert.Equal(EmulationMediaType.HardDisk, Assert.Single(factory.ResolveConfiguredMedia(restored)).Type);
        var device = Assert.Single(factory.DescribeMediaDevices(restored)); Assert.Equal(slot, device.Slot); Assert.False(device.IsRemovable);
        Assert.True(Assert.Single(device.HardDiskFormats!).SupportsExplicitGeometry); Assert.NotNull(Assert.Single(device.HardDiskFormats!).ImageCreator);
        var saved = Assert.Single(factory.DescribeDeviceSettings(restored)); Assert.Equal("200", saved.Options!["speed"]); Assert.Equal("7", saved.Options["writeLatency"]);
        Assert.Contains(factory.GetFirmwareSlots(restored), firmware => firmware.FieldId == StorageConstants.DfsFirmware && firmware.IsRequired);
        Assert.True(factory.RequiresOptionRestart(restored, StorageConstants.HardDiskKey(unit, "speed")));
        var activity = factory.MediaActivity(restored, new Dictionary<int, bool> { [StorageConstants.HardDiskLedStart + unit] = true }); Assert.True(activity[slot]);
        var removed = factory.ApplyStorage(restored, stored with { ConfiguredSlots = [], MountedMedia = [], DeviceSettings = [] });
        Assert.Empty(factory.DescribeMediaDevices(removed)); Assert.DoesNotContain(factory.GetFirmwareSlots(removed), firmware => firmware.FieldId == StorageConstants.DfsFirmware);
        Assert.Throws<ArgumentException>(() => factory.ApplyStorage(empty, settings with { DeviceSettings = [new(slot, InterfaceId: "IDE")] }));
    }

    [Fact]
    public void WriteLatencyCanDifferFromReadLatencyOnTheSameUnit()
    {
        using var disk = new Disk(); var controller = Controller(_ => disk); controller.Configure(0, 0, 100, 20);
        Assert.Equal(256, Execute(controller, Cdb(8, 0, 0, 1)).Data.Length);
        Send(controller, Cdb(10, 0, 0, 1));
        for (var tick = 0; tick < 39_999; tick++) controller.Tick();
        Assert.Equal(0, controller.Read(1) & 0x20); controller.Tick(); Assert.Equal(0x22, controller.Read(1));
        controller.Reset(); Assert.False(disk.HasChanges);
    }

    [Fact]
    public void ModelABusReachesWinchesterAndItsIrqWithoutAffectingAnUnexpandedMachine()
    {
        var plain = new ModelAMachine(new byte[16384], new byte[16384]);
        Assert.Equal(255, plain.Read(0xfc41)); plain.Write(0xfc43, 255); Assert.False(plain.Irq);
        using var disk = new Disk();
        var expanded = new ModelAMachine(new byte[16384], new byte[16384], winchesterInterface: true);
        try
        {
            expanded.AttachHardDisk(0, disk); expanded.ConfigureHardDisk(0, 0, 0);
            Assert.Equal(0x20, expanded.Read(0xfc41)); expanded.Write(0xfc43, 255); Assert.True(expanded.Irq);
            expanded.Write(0xfc43, 0); Assert.False(expanded.Irq);
            expanded.Write(0xfc40, 0); expanded.Write(0xfc42, 0);
            foreach (var value in Cdb(8, 0, 1, 1)) { expanded.Tick(); expanded.Tick(); expanded.Write(0xfc40, value); }
            expanded.Tick(); expanded.Tick(); expanded.Tick(); Assert.Equal(0x62, expanded.Read(0xfc41));
            Assert.True(expanded.HardDiskActive(0)); Assert.Equal(disk.Bytes(1)[0], expanded.Read(0xfc40));
            expanded.Reset(false); Assert.False(expanded.Irq); Assert.False(expanded.HardDiskActive(0)); Assert.False(disk.Disposed);
        }
        finally { expanded.AttachHardDisk(0, null); }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void UnitSelectionReadsAndWritesOnlyTheAddressedDisk(int unit)
    {
        using var first = new Disk(10); using var second = new Disk(50); using var third = new Disk(90); using var fourth = new Disk(130);
        var disks = new[] { first, second, third, fourth }; var controller = Controller(index => disks[index]);
        var before = disks.Select(disk => disk.Bytes(3).ToArray()).ToArray();
        var read = Execute(controller, Cdb(8, unit, 3, 2));
        Assert.Equal(unit << 5, read.Status); Assert.Equal(disks[unit].Bytes(3).Concat(disks[unit].Bytes(4)), read.Data);
        var replacement = Enumerable.Repeat((byte)0xcc, 512).ToArray();
        Assert.Equal(unit << 5, Execute(controller, Cdb(10, unit, 3, 2), replacement).Status);
        Assert.All(disks[unit].Bytes(3), value => Assert.Equal(0xcc, value));
        Assert.All(disks[unit].Bytes(4), value => Assert.Equal(0xcc, value));
        for (var index = 0; index < disks.Length; index++) if (index != unit) Assert.Equal(before[index], disks[index].Bytes(3));
        Assert.False(controller.Busy); Assert.Equal(0x20, controller.Read(1));
    }

    [Theory]
    [InlineData(256)]
    [InlineData(512)]
    public void BusBlocksAre256BytesAndPreserveOtherPartsOfLargerContainerSectors(int sectorSize)
    {
        using var disk = new Disk(20, sectorSize); var controller = Controller(_ => disk);
        var original = disk.Bytes(0).ToArray(); var replacement = Enumerable.Repeat((byte)0x79, 256).ToArray();
        Execute(controller, Cdb(10, 0, 1, 1), replacement);
        Assert.Equal(replacement, Execute(controller, Cdb(8, 0, 1, 1)).Data);
        Assert.Equal(original.AsSpan(0, 256).ToArray(), Execute(controller, Cdb(8, 0, 0, 1)).Data);
        var capacity = Execute(controller, [0x25, 0, 0, 0, 0, 0, 0, 0, 0, 0]);
        Assert.Equal(197u, BinaryPrimitives.ReadUInt32BigEndian(capacity.Data));
        Assert.Equal(256u, BinaryPrimitives.ReadUInt32BigEndian(capacity.Data.AsSpan(4)));
    }

    [Fact]
    public void TenByteCommandsUseTheirBlockCountAndAddress()
    {
        using var disk = new Disk(); var controller = Controller(_ => disk);
        var write = new byte[10]; write[0] = 0x2a; BinaryPrimitives.WriteUInt32BigEndian(write.AsSpan(2), 4); BinaryPrimitives.WriteUInt16BigEndian(write.AsSpan(7), 2);
        var data = Enumerable.Repeat((byte)0x5b, 512).ToArray(); Assert.Equal(0, Execute(controller, write, data).Status);
        write[0] = 0x28; Assert.Equal(data, Execute(controller, write).Data);
        BinaryPrimitives.WriteUInt16BigEndian(write.AsSpan(7), 0); Assert.Empty(Execute(controller, write).Data);
    }

    [Fact]
    public void CheckConditionIsFollowedBySenseAndDoesNotPoisonTheNextCommand()
    {
        using var disk = new Disk { IsReadOnly = true }; var controller = Controller(unit => unit == 0 ? disk : null);
        Assert.Equal(2, Execute(controller, Cdb(10, 0, 0, 1)).Status); Assert.False(disk.HasChanges);
        Assert.Equal(0x27, Execute(controller, Cdb(3, 0, 0, 4)).Data[0]);
        Assert.Equal(new byte[4], Execute(controller, Cdb(3, 0, 0, 0)).Data);
        Assert.Equal(2, Execute(controller, Cdb(8, 0, 198, 1)).Status);
        Assert.Equal(new byte[] { 0x21, 0, 0, 198 }, Execute(controller, Cdb(3, 0, 0, 4)).Data);
        Assert.Equal(0, Execute(controller, Cdb(0, 0, 0, 0)).Status);
        Assert.Equal(0x62, Execute(controller, Cdb(0, 3, 0, 0)).Status);
        Assert.Equal(2, Execute(controller, Cdb(0x7f, 0, 0, 0)).Status);
        Assert.Equal(0x20, Execute(controller, Cdb(3, 0, 0, 4)).Data[0]);
    }

    [Fact]
    public void BadSectorDataIsReportedAndUnknownUnitsCannotIndexPastTheImageArray()
    {
        using var disk = new Disk { Bad = true }; var controller = Controller(_ => disk);
        Assert.Equal(2, Execute(controller, Cdb(8, 0, 0, 1)).Status);
        Assert.Equal(0x11, Execute(controller, Cdb(3, 0, 0, 4)).Data[0]);
        Assert.Equal(0xe2, Execute(controller, Cdb(0, 7, 0, 0)).Status);
    }

    [Fact]
    public void GeometrySenseAndSelectionValidateTheCapacityInsteadOfChangingTheImageSize()
    {
        using var disk = new Disk(); var controller = Controller(_ => disk);
        var descriptor = Execute(controller, Cdb(0x1a, 0, 0, 22)).Data;
        Assert.Equal(22, descriptor.Length); Assert.Equal(3, descriptor[14]); Assert.Equal(2, descriptor[15]);
        Assert.Equal(0, Execute(controller, Cdb(0x15, 0, 0, 22), descriptor).Status);
        descriptor[14] = 4;
        Assert.Equal(2, Execute(controller, Cdb(0x15, 0, 0, 22), descriptor).Status);
        Assert.Equal(0x24, Execute(controller, Cdb(3, 0, 0, 4)).Data[0]);
        Assert.Equal(198, disk.SectorCount); Assert.False(disk.HasChanges);
    }

    [Fact]
    public void ModeSelectPassesTheCompleteDescriptorToTheImageAndSenseReadsItBack()
    {
        using var disk = new Disk(); var controller = Controller(_ => disk);
        var descriptor = Execute(controller, Cdb(0x1a, 0, 0, 22)).Data;
        descriptor[5] = 17; descriptor[14] = 6; descriptor[15] = 1;
        Assert.Equal(0, Execute(controller, Cdb(0x15, 0, 0, 22), descriptor).Status);
        Assert.True(disk.HasChanges); Assert.Equal(6, disk.Cylinders); Assert.Equal(1, disk.Heads);
        Assert.Equal(descriptor, Convert.FromBase64String(disk.Metadata[EmulationDiskMetadataKeys.AcornScsiParameters]));
        Assert.Equal(descriptor, Execute(controller, Cdb(0x1a, 0, 0, 22)).Data);
        controller.Reset(); Assert.Equal(descriptor, Execute(controller, Cdb(0x1a, 0, 0, 22)).Data);
    }

    [Fact]
    public void LowLevelFormatClearsAllBlocksAndKeepsTheBorrowedImageAlive()
    {
        using var disk = new Disk(30); var controller = Controller(_ => disk);
        Assert.Equal(0, Execute(controller, Cdb(4, 0, 0, 0)).Status);
        for (var index = 0; index < disk.SectorCount; index++) Assert.All(disk.Bytes(index), value => Assert.Equal(0, value));
        Assert.True(disk.HasChanges); controller.Reset(); Assert.False(disk.Disposed); Assert.Equal(0x20, controller.Read(1));
    }

    [Fact]
    public void IrqAndCommandLatencyAreIndependentOfDataAndStatusAcknowledgement()
    {
        using var disk = new Disk(); var controller = Controller(_ => disk); controller.Configure(0, 20, 100);
        Send(controller, Cdb(0, 0, 0, 0));
        Assert.True(controller.Active(0)); Assert.False(controller.Active(1)); Assert.Equal(0, controller.Read(1) & 0x20);
        for (var tick = 0; tick < 39_999; tick++) controller.Tick();
        Assert.Equal(0, controller.Read(1) & 0x20); controller.Tick(); Assert.Equal(0xe2, controller.Read(1));
        controller.Write(3, 255); Assert.True(controller.Irq); Assert.Equal(0xf2, controller.Read(1));
        Assert.Equal(0, controller.Read(0)); Assert.True(controller.Irq); Ready(controller); Assert.Equal(0, controller.Read(0)); Assert.False(controller.Irq);
        controller.Write(3, 255); controller.Write(3, 0); Assert.False(controller.Irq);
    }

    private static AcornScsi Controller(Func<int, IEmulationDiskImage?> images)
    {
        var controller = new AcornScsi(images);
        for (var unit = 0; unit < 4; unit++) controller.Configure(unit, 0, 0);
        return controller;
    }
    private static byte[] Cdb(byte command, int unit, int address, int count) => [command, (byte)(unit << 5 | address >> 16 & 31), (byte)(address >> 8), (byte)address, (byte)count, 0];
    private static void Ready(AcornScsi controller)
    {
        for (var tick = 0; tick < 2_000_000; tick++) { if ((controller.Read(1) & 0x20) != 0) return; controller.Tick(); }
        Assert.Fail("SCSI request timed out.");
    }
    private static void Send(AcornScsi controller, byte[] command)
    {
        controller.Write(0, 0); Assert.Equal(2, controller.Read(1) & 2);
        controller.Write(2, 0); Assert.Equal(0xa2, controller.Read(1));
        foreach (var value in command) { Ready(controller); controller.Write(0, value); }
    }
    private static (byte Status, byte[] Data) Execute(AcornScsi controller, byte[] command, byte[]? write = null)
    {
        Send(controller, command); var data = new List<byte>(); var offset = 0;
        while (true)
        {
            Ready(controller); var phase = controller.Read(1);
            if ((phase & 0xc0) == 0xc0)
            {
                var status = controller.Read(0); Ready(controller); Assert.Equal(0xe3, controller.Read(1) & 0xef);
                Assert.Equal(0, controller.Read(0)); Assert.False(controller.Busy);
                if (write is not null) Assert.Equal(write.Length, offset);
                return (status, data.ToArray());
            }
            if ((phase & 0x40) != 0) data.Add(controller.Read(0));
            else { Assert.NotNull(write); Assert.True(offset < write.Length); controller.Write(0, write[offset++]); }
        }
    }

    private sealed class Disk(byte seed = 0, int sectorSize = 256) : IEmulationDiskImage
    {
        private readonly Dictionary<long, byte[]> _data = new();
        private readonly Dictionary<string, string> _metadata = new();
        private int _cylinders = 3, _heads = 2;
        public IEmulationDriveMechanics Mechanics { get; } = GWGUI.Emulation.EmulationSdk.MediaProcessing.CreateDriveMechanics(GWGUI.Emulation.Constants.EmulationMechanicalSoundConstants.HardDisk);
        public string FormatId => "hdd.acorn-winchester";
        public int SectorSize => sectorSize;
        public long SectorCount => 198 * 256 / SectorSize;
        public long Cylinders => _cylinders;
        public int Heads => _heads;
        public int SectorsPerTrack => 33;
        public bool IsReadOnly { get; init; }
        public bool HasChanges { get; private set; }
        public IReadOnlyDictionary<string, string> Metadata => _metadata;
        public void UpdateMetadata(IReadOnlyDictionary<string, string> metadata)
        {
            if (IsReadOnly) throw new UnauthorizedAccessException();
            var descriptor = Convert.FromBase64String(metadata[EmulationDiskMetadataKeys.AcornScsiParameters]);
            var cylinders = descriptor[13] << 8 | descriptor[14]; var heads = descriptor[15];
            if (cylinders <= 0 || heads <= 0 || cylinders * heads * 33 != 198) throw new ArgumentException(nameof(metadata));
            HasChanges |= cylinders != _cylinders || heads != _heads;
            _cylinders = cylinders; _heads = heads; _metadata[EmulationDiskMetadataKeys.AcornScsiParameters] = Convert.ToBase64String(descriptor);
        }
        internal bool Bad { get; init; }
        internal bool Disposed { get; private set; }
        internal byte[] Bytes(long index) { if (!_data.TryGetValue(index, out var bytes)) _data[index] = bytes = Enumerable.Repeat((byte)(seed + index), SectorSize).ToArray(); return bytes; }
        public long? FindSector(int cylinder, int head, int number) => (cylinder * 2L + head) * 33 + number;
        public ValueTask<EmulationDiskSector> ReadSectorAsync(long index, CancellationToken cancellationToken = default) => ValueTask.FromResult(new EmulationDiskSector(index, Bytes(index), (int)(index / 66), (int)(index / 33 % 2), (int)(index % 33), !Bad));
        public void WriteSector(long index, ReadOnlySpan<byte> data) { if (IsReadOnly) throw new UnauthorizedAccessException(); if (data.Length != SectorSize) throw new ArgumentException(nameof(data)); _data[index] = data.ToArray(); HasChanges = true; }
        public Task FlushAsync(CancellationToken cancellationToken = default) { HasChanges = false; return Task.CompletedTask; }
        public void Dispose() { Disposed = true; _data.Clear(); }
    }
}
