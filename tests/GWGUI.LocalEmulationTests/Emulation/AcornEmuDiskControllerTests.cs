using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.ModelA.Services;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Factories;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Enums;
using GWGUI.Emulation.Enums;
using System.IO;
using NativeStorage = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions.StorageFunctions;

namespace GWGUI.Tests.Emulation;

/// <summary>Exerce le protocole matériel sur des secteurs en mémoire, sans fichiers ni firmware externes.</summary>
public sealed class AcornEmuDiskControllerTests
{
    [Theory]
    [InlineData("ss40", 40, 1, 10, true)]
    [InlineData("ss40", 80, 1, 10, false)]
    [InlineData("ss40", 40, 2, 10, false)]
    [InlineData("ss40", 40, 1, 16, false)]
    [InlineData("ds80", 80, 2, 10, true)]
    [InlineData("ds80", 80, 2, 16, false)]
    [InlineData("ds80-dd", 80, 2, 16, true)]
    [InlineData("ds80-dd", 40, 1, 10, true)]
    [InlineData("ss80-dd", 80, 2, 16, false)]
    [InlineData("ds40-dd", 80, 1, 16, false)]
    [InlineData("ds40-dd", 40, 2, 16, true)]
    [InlineData("ds80-dd", 0, 2, 16, false)]
    public void DriveRejectsUnsupportedFacesDensityOrGeometry(string model, int cylinders, int heads, int sectors, bool accepted)
    {
        using var image = new MemoryDisk { Cylinders = cylinders, Heads = heads, SectorsPerTrack = sectors };
        var settings = new FloppyDriveSettings(model, "100", false, false);
        if (accepted) NativeStorage.ValidateMedia(image, settings);
        else Assert.Throws<InvalidDataException>(() => NativeStorage.ValidateMedia(image, settings));
    }

    [Fact]
    public void DriveRejectsInconsistentImageCapacity()
    {
        using var image = new MemoryDisk { Cylinders = 40, Heads = 1, SectorsPerTrack = 10, DeclaredSectorCount = 399 };
        Assert.Throws<InvalidDataException>(() => NativeStorage.ValidateMedia(image, new("ss40", "100", false, false)));
    }

    [Theory]
    [InlineData("Cassette", "Play")]
    [InlineData("Floppy", "Motor")]
    [InlineData("Floppy", "Read")]
    [InlineData("Floppy", "Write")]
    [InlineData("Floppy", "Seek")]
    [InlineData("Floppy", "Insert")]
    [InlineData("Floppy", "Eject")]
    [InlineData("HardDisk", "Motor")]
    [InlineData("HardDisk", "Read")]
    [InlineData("HardDisk", "Write")]
    [InlineData("HardDisk", "Seek")]
    [InlineData("Optical", "Motor")]
    [InlineData("Optical", "Read")]
    [InlineData("Optical", "Write")]
    [InlineData("Optical", "Seek")]
    [InlineData("Optical", "Insert")]
    [InlineData("Optical", "Eject")]
    public void SharedMechanicalSoundIsIndependentOfAudioChunkBoundaries(string device, string operation)
    {
        var service = GWGUI.Emulation.EmulationSdk.MediaProcessing;
        var complete = service.RenderMechanicalSound(device, operation, 48000, 4800, 0);
        var first = service.RenderMechanicalSound(device, operation, 48000, 137, 0);
        var rest = service.RenderMechanicalSound(device, operation, 48000, 4663, 137);
        Assert.Contains(complete, sample => sample != 0);
        Assert.Equal(complete, first.Concat(rest).ToArray());
        if (operation is "Seek" or "Insert" or "Eject") Assert.All(service.RenderMechanicalSound(device, operation, 48000, 100, 48000), sample => Assert.Equal(0, sample));
    }

    [Theory]
    [InlineData("Floppy")]
    [InlineData("HardDisk")]
    [InlineData("Optical")]
    public void EachMechanicalProfileProducesItsOwnSignalAndRejectsAnotherDeviceProfile(string device)
    {
        var service = GWGUI.Emulation.EmulationSdk.MediaProcessing;
        var profiles = service.MechanicalSoundProfiles(device);
        Assert.True(profiles.Count > 1);
        var first = service.RenderMechanicalSound(device, "Motor", 48000, 4800, 0, profiles[0]);
        var second = service.RenderMechanicalSound(device, "Motor", 48000, 4800, 0, profiles[1]);
        Assert.False(first.SequenceEqual(second));
        Assert.Throws<ArgumentOutOfRangeException>(() => service.RenderMechanicalSound(device, "Motor", 48000, 10, 0, "Standard"));
    }
    [Fact]
    public void DiskDriveConfigurationUsesTheExistingStorageContractAndRemainsOptional()
    {
        var factory = new AcornEmuMachineFactory();
        var configuration = new MachineConfiguration(MachineModel.BbcMicroModelA, core: Emulator.AcornEmu);
        var available = factory.DescribeStorage(configuration);
        Assert.Empty(available.ConfiguredSlots); Assert.Equal(2, available.AvailableDevices.Count(device => device.MediaType == EmulationMediaType.Floppy));
        var selected = factory.ApplyStorage(configuration, available with
        {
            ConfiguredSlots = [EmulationMediaSlot.Floppy0, EmulationMediaSlot.Floppy1],
            DeviceSettings = [new(EmulationMediaSlot.Floppy0, new("ss40", "400", true, true)), new(EmulationMediaSlot.Floppy1, new("ds80", "100", false, false))]
        });
        var restored = new MachineConfiguration(MachineModel.BbcMicroModelA, options: selected.Options, core: Emulator.AcornEmu);
        Assert.Equal(2, factory.DescribeStorage(restored).ConfiguredSlots.Count);
        var first = Assert.Single(factory.DescribeDeviceSettings(restored), item => item.Slot == EmulationMediaSlot.Floppy0).Floppy!;
        Assert.Equal("ss40", first.Model); Assert.Equal("400", first.Speed); Assert.True(first.WriteProtected); Assert.True(first.RedirectWrites);
        Assert.Contains(factory.GetFirmwareSlots(restored), slot => slot.FieldId == "acornemu.dfs" && slot.IsRequired);
        Assert.True(factory.RequiresOptionRestart(restored, "acornemu.floppy.0.model"));
        var removed = factory.ApplyStorage(restored, factory.DescribeStorage(restored) with { ConfiguredSlots = [], DeviceSettings = [] });
        Assert.Empty(factory.DescribeMediaDevices(removed)); Assert.DoesNotContain(factory.GetFirmwareSlots(removed), slot => slot.FieldId == "acornemu.dfs");
        Assert.Throws<ArgumentException>(() => factory.ApplyStorage(configuration, available with { ConfiguredSlots = [EmulationMediaSlot.Floppy0], DeviceSettings = [new(EmulationMediaSlot.Floppy0, new("invalid", "100", false, false))] }));
        var activity = factory.MediaActivity(restored, new Dictionary<int, bool> { [0] = true, [4] = true });
        Assert.True(activity[EmulationMediaSlot.Floppy0]); Assert.True(activity[EmulationMediaSlot.Floppy1]); Assert.False(activity[EmulationMediaSlot.Cassette0]);
    }

    [Fact]
    public void BoardPagesTheExternalRomAndRoutesControllerRequestsToCpuNmi()
    {
        var mos = new byte[16384]; mos[^6] = 0; mos[^5] = 3; mos[^4] = 0; mos[^3] = 2;
        var basic = new byte[16384]; basic[0] = 0x55; var dfs = new byte[16384]; dfs[0] = 0xa5;
        var board = new ModelAMachine(mos, basic, diskInterface: true, optionRoms: new Dictionary<int, byte[]> { [14] = dfs });
        using var disk = new MemoryDisk();
        try
        {
            Assert.Equal(0x55, board.Read(0x8000)); board.Write(0xfe30, 14); Assert.Equal(0xa5, board.Read(0x8000));
            board.Write(0x8000, 0); Assert.Equal(0xa5, board.Read(0x8000)); board.Write(0xfe30, 12); Assert.Equal(255, board.Read(0x8000));
            board.AttachFloppy(0, disk); board.Write(0x200, 0xea); board.Write(0x201, 0x4c); board.Write(0x202, 0); board.Write(0x203, 2);
            board.Write(0xfe80, 0x75); foreach (var value in new byte[] { 0x0d, 0, 0, 0x10 }) board.Write(0xfe81, value);
            board.Write(0xfe80, 0x53); foreach (var value in new byte[] { 0, 0, 0x21 }) board.Write(0xfe81, value);
            for (var instruction = 0; instruction < 500 && board.Cpu.PC != 0x300; instruction++) board.Cpu.Step();
            Assert.Equal(0x300, board.Cpu.PC); Assert.True(board.Nmi); Assert.True(board.FloppyActive(0)); Assert.False(board.FloppyActive(1));
            Assert.Equal(0, board.Read(0xfe84)); Assert.False(board.Nmi);
            board.Reset(true); Assert.False(board.Nmi); Assert.Equal(0x55, board.Read(0x8000)); Assert.False(disk.Disposed);
        }
        finally { board.AttachFloppy(0, null); }
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    public void ReadMultipleSelectsDriveSideAndSectorAndAcknowledgesEveryByte(int drive, int side)
    {
        using var first = new MemoryDisk(); using var second = new MemoryDisk(0x80);
        var controller = new Intel8271(index => index == 0 ? first : second); FastTiming(controller);
        Command(controller, 0x7a, 0x23, (byte)(side << 5));
        Command(controller, (byte)(0x13 | (drive == 0 ? 0x40 : 0x80)), 1, 3, 0x22);
        var read = Transfer(controller);
        var disk = drive == 0 ? first : second;
        Assert.Equal(0, read.Result);
        Assert.Equal(disk.Data((1 * 2 + side) * 10 + 3).Concat(disk.Data((1 * 2 + side) * 10 + 4)), read.Bytes);
        Assert.False(controller.Nmi); Assert.Equal(0, controller.Read(0));
        Assert.True(controller.Motor);
    }

    [Fact]
    public void WriteRequestsEachByteAndPreservesAdjacentSectors()
    {
        using var disk = new MemoryDisk(); var controller = new Intel8271(_ => disk); FastTiming(controller);
        var before = disk.Data(4).ToArray();
        var bytes = Enumerable.Range(0, 512).Select(index => (byte)(index ^ 0xa5)).ToArray();
        Command(controller, 0x4b, 0, 2, 0x22);
        Assert.Equal(0, Transfer(controller, bytes).Result);
        Assert.Equal(bytes[..256], disk.Data(2)); Assert.Equal(bytes[256..], disk.Data(3)); Assert.Equal(before, disk.Data(4));
        Assert.True(disk.HasChanges);
        Command(controller, 0x53, 0, 2, 0x22); Assert.Equal(bytes, Transfer(controller).Bytes);
    }

    [Fact]
    public void FormatUsesTheSuppliedIdsAndProtectsTheOtherSide()
    {
        using var disk = new MemoryDisk(); var controller = new Intel8271(_ => disk); FastTiming(controller);
        var firstSide = disk.Data(2).ToArray();
        Command(controller, 0x7a, 0x23, 0x20);
        Command(controller, 0x63, 0, 0, 2, 0, 0);
        Assert.Equal(0, Transfer(controller, [0, 1, 2, 1, 0, 1, 3, 1]).Result);
        Assert.All(disk.Data(12), value => Assert.Equal(0xe5, value)); Assert.All(disk.Data(13), value => Assert.Equal(0xe5, value));
        Assert.Equal(firstSide, disk.Data(2));
    }

    [Fact]
    public void ReadIdAndVerifyUseTheSamePhysicalSectorAddresses()
    {
        using var disk = new MemoryDisk(); var controller = new Intel8271(_ => disk); FastTiming(controller);
        Command(controller, 0x7a, 0x23, 0x20);
        Command(controller, 0x5b, 1, 0, 2);
        Assert.Equal(new byte[] { 1, 1, 0, 1, 1, 1, 1, 1 }, Transfer(controller).Bytes);
        Command(controller, 0x5f, 1, 0, 0x22);
        var verify = Transfer(controller); Assert.Equal(0, verify.Result); Assert.Empty(verify.Bytes);
    }

    [Fact]
    public void ProtectionMissingMediaAndMissingSectorsHaveResultsInsteadOfHostFailures()
    {
        using var disk = new MemoryDisk { IsReadOnly = true }; var controller = new Intel8271(_ => disk); FastTiming(controller);
        Command(controller, 0x4b, 0, 0, 0x21); Assert.True(controller.Nmi); Assert.Equal(0x12, Transfer(controller).Result); Assert.False(disk.HasChanges);
        Command(controller, 0x53, 0, 99, 0x21); Assert.Equal(0x18, Transfer(controller).Result);
        var absent = new Intel8271(_ => null); Command(absent, 0x53, 0, 0, 0x21); Assert.Equal(0x10, Transfer(absent).Result);
    }

    [Fact]
    public void UnacknowledgedByteReportsLateDmaAndBadDataReportsCrcAfterTransfer()
    {
        using var disk = new MemoryDisk { BadSector = 0 }; var controller = new Intel8271(_ => disk); FastTiming(controller);
        Command(controller, 0x53, 0, 0, 0x21);
        for (var tick = 0; tick < 500; tick++) controller.Tick();
        Assert.True(controller.Nmi); Assert.Equal(0x0a, controller.Read(1)); Assert.False(controller.Nmi);
        Command(controller, 0x53, 0, 0, 0x21); var read = Transfer(controller);
        Assert.Equal(0x0e, read.Result); Assert.Equal(256, read.Bytes.Length);
    }

    [Fact]
    public void SeekAndResetDoNotOwnOrDisposeTheBorrowedImage()
    {
        using var disk = new MemoryDisk(); var controller = new Intel8271(_ => disk); FastTiming(controller);
        Command(controller, 0x69, 1); Assert.Equal(0, Transfer(controller).Result);
        Command(controller, 0x7d, 0x12); Assert.Equal(1, controller.Read(1)); Assert.False(controller.Nmi);
        controller.Write(2, 1); Assert.False(controller.Nmi); Assert.False(controller.Motor); Assert.Equal(0, controller.Read(0)); Assert.False(disk.Disposed);
    }

    [Fact]
    public void FirstReadWaitsForMechanicalLoadBeforeTheFirstDataRequest()
    {
        using var disk = new MemoryDisk(); var controller = new Intel8271(_ => disk);
        Command(controller, 0x53, 0, 0, 0x21);
        for (var tick = 0; tick < 400_000; tick++) controller.Tick();
        Assert.False(controller.Nmi); Assert.Equal(0x80, controller.Read(0));
        Assert.Equal(0, Transfer(controller).Result);
    }

    private static void FastTiming(Intel8271 controller) => Command(controller, 0x75, 0x0d, 0, 0, 0x10);
    private static void Command(Intel8271 controller, byte command, params byte[] parameters) { controller.Write(0, command); foreach (var value in parameters) controller.Write(1, value); }
    private static (byte Result, byte[] Bytes) Transfer(Intel8271 controller, byte[]? write = null)
    {
        var bytes = new List<byte>(); var written = 0;
        for (var tick = 0; tick < 2_000_000; tick++)
        {
            controller.Tick(); var status = controller.Read(0);
            if ((status & 4) != 0)
            {
                if (write is null) bytes.Add(controller.Read(4));
                else { Assert.True(written < write.Length); controller.Write(4, write[written++]); }
            }
            else if ((status & 0x90) == 0x10) { if (write is not null) Assert.Equal(write.Length, written); return (controller.Read(1), bytes.ToArray()); }
        }
        Assert.Fail("The controller did not complete its command."); return default;
    }

    private sealed class MemoryDisk(byte seed = 0) : IEmulationDiskImage
    {
        private readonly Dictionary<long, byte[]> _data = new();
        public string FormatId => "acorn.dfs.ds";
        public int SectorSize => 256;
        public long SectorCount => DeclaredSectorCount ?? Cylinders * Heads * SectorsPerTrack;
        internal long? DeclaredSectorCount { get; init; }
        public long Cylinders { get; init; } = 2;
        public int Heads { get; init; } = 2;
        public int SectorsPerTrack { get; init; } = 10;
        public bool IsReadOnly { get; init; }
        public bool HasChanges { get; private set; }
        public IReadOnlyDictionary<string, string> Metadata { get; } = new Dictionary<string, string>();
        public void UpdateMetadata(IReadOnlyDictionary<string, string> metadata) => throw new NotSupportedException();
        internal long? BadSector { get; init; }
        internal bool Disposed { get; private set; }
        internal byte[] Data(long index) { if (!_data.TryGetValue(index, out var data)) _data[index] = data = Enumerable.Repeat((byte)(seed + index), SectorSize).ToArray(); return data; }
        public long? FindSector(int cylinder, int head, int number) => cylinder is >= 0 and < 2 && head is >= 0 and < 2 && number is >= 0 and < 10 ? (cylinder * 2L + head) * 10 + number : null;
        public ValueTask<EmulationDiskSector> ReadSectorAsync(long index, CancellationToken cancellationToken = default) => ValueTask.FromResult(new EmulationDiskSector(index, Data(index), (int)(index / 20), (int)(index / 10 % 2), (int)(index % 10), index != BadSector));
        public void WriteSector(long index, ReadOnlySpan<byte> data) { if (IsReadOnly) throw new UnauthorizedAccessException(); if (data.Length != SectorSize) throw new ArgumentException(nameof(data)); _data[index] = data.ToArray(); HasChanges = true; }
        public Task FlushAsync(CancellationToken cancellationToken = default) { HasChanges = false; return Task.CompletedTask; }
        public void Dispose() { Disposed = true; _data.Clear(); }
    }
}
