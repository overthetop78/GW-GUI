using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.ModelA.Services;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Factories;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Enums;

namespace GWGUI.Tests.Emulation;

/// <summary>Protocole WD1770 exercé avec une image empruntée entièrement en mémoire.</summary>
public sealed class AcornEmuWd1770Tests
{
    [Fact]
    public void AcornControlLatchAndRegistersReachTheSelectedSideAndNmi()
    {
        using var disk = new Disk(80);
        var machine = new ModelAMachine(new byte[16384], new byte[16384], diskInterface: true, wd1770: true);
        try
        {
            machine.AttachFloppy(1, disk); machine.ConfigureFloppySpeed(1, 0);
            machine.Write(0xfe80, 0x26); // Reset désactivé, lecteur 1, face 1, double densité.
            machine.Write(0xfe86, 0); machine.Write(0xfe84, 0x88);
            for (var tick = 0; tick < 100; tick++) machine.Tick();
            Assert.True(machine.Nmi); Assert.True(machine.FloppyActive(1)); Assert.False(machine.FloppyActive(0));
            Assert.Equal(disk.Bytes(0, 1, 0)[0], machine.Read(0xfe87)); Assert.False(machine.Nmi);
            machine.Write(0xfe80, 0); Assert.False(machine.Nmi); Assert.False(machine.FloppyActive(1)); Assert.False(disk.Disposed);
        }
        finally { machine.AttachFloppy(1, null); }
    }

    [Fact]
    public void ControllerChoiceBelongsToExtensionsAndDoesNotFilterImagesForTheSameDrive()
    {
        var factory = new AcornEmuMachineFactory();
        var configuration = new MachineConfiguration(MachineModel.BbcMicroModelA, core: Emulator.AcornEmu);
        var storage = factory.DescribeStorage(configuration);
        configuration = factory.ApplyStorage(configuration, storage with { ConfiguredSlots = [EmulationMediaSlot.Floppy0, EmulationMediaSlot.Floppy1] });
        storage = factory.DescribeStorage(configuration);
        var settings = storage.DeviceSettings!.Select(device => device with
        {
            Floppy = device.Floppy! with { Model = StorageConstants.Double80Dd }
        }).ToArray();
        configuration = factory.ApplyStorage(configuration, storage with { DeviceSettings = settings });
        var intelExtensions = factory.DescribeStorage(configuration).AvailableDevices.SelectMany(device => device.AcceptedExtensions).ToArray();
        Assert.Contains(".adl", intelExtensions);
        configuration = configuration with { Options = new Dictionary<string, string>(configuration.Options)
        {
            [StorageConstants.ControllerOption] = StorageConstants.AcornWd1770
        } };
        factory.ValidateOptions(configuration);
        Assert.Equal(intelExtensions, factory.DescribeStorage(configuration).AvailableDevices.SelectMany(device => device.AcceptedExtensions).ToArray());
        Assert.True(factory.RequiresOptionRestart(configuration, StorageConstants.ControllerOption));
        var field = Assert.Single(factory.DescribeSettings(configuration).SelectMany(block => block.Fields), field => field.Id == StorageConstants.ControllerOption);
        Assert.Equal(EmulationMachineTab.Extensions, field.Tab);
        Assert.True(field.UseRadioButtons);
        Assert.Equal(StorageConstants.AcornWd1770, field.Value);
        Assert.All(factory.DescribeStorage(configuration).AvailableDevices.Where(device => device.MediaType == EmulationMediaType.Floppy),
            device => Assert.DoesNotContain(device.ConfigurationFields!, field => field.Id == StorageConstants.ControllerOption));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    public void ReadsAndWritesTheSelectedPhysicalSide(int drive, int side)
    {
        using var first = new Disk(1); using var second = new Disk(70);
        var controller = new Wd1770(index => index == 0 ? first : second) { Drive = drive, Side = side, MechanicalSpeedPercent = 0 };
        var disk = drive == 0 ? first : second;
        Seek(controller, 2); controller.Write(2, 3); controller.Write(0, 0x88);
        var read = Transfer(controller); Assert.Equal(0, read.Status & 0x7c); Assert.Equal(disk.Bytes(2, side, 3), read.Data);
        var replacement = Enumerable.Repeat((byte)0xc7, 256).ToArray();
        controller.Write(0, 0xa8); Assert.Equal(0, Transfer(controller, replacement).Status & 0x7c);
        Assert.Equal(replacement, disk.Bytes(2, side, 3)); Assert.NotEqual(replacement, disk.Bytes(2, 1 - side, 3));
        Assert.True(disk.HasChanges);
    }

    [Fact]
    public void MultiSectorReadStopsAtTheMissingNextIdAndForcedInterruptCanCancelIt()
    {
        using var disk = new Disk(); var controller = new Wd1770(_ => disk) { MechanicalSpeedPercent = 0 };
        controller.Write(2, 14); controller.Write(0, 0x98);
        var read = Transfer(controller); Assert.Equal(16, read.Status & 16); Assert.Equal(disk.Bytes(0, 0, 14).Concat(disk.Bytes(0, 0, 15)), read.Data);
        controller.Write(2, 0); controller.Write(0, 0x98);
        while (!controller.Drq) controller.Tick();
        Assert.True(controller.Nmi); controller.Write(0, 0xd0);
        Assert.False(controller.Nmi); Assert.False(controller.Active);
        controller.Write(0, 0xd8); Assert.True(controller.Irq); controller.Read(0); Assert.False(controller.Irq);
    }

    [Fact]
    public void ReadAddressAndTrackContainThePhysicalIdsAndDataAndTrackCanBeRewritten()
    {
        using var disk = new Disk(); var controller = new Wd1770(_ => disk) { MechanicalSpeedPercent = 0, Side = 1 };
        Seek(controller, 1); controller.Write(0, 0xc8);
        Assert.Equal(new byte[] { 1, 1, 0, 1 }, Transfer(controller).Data.Take(4));
        controller.Write(0, 0xe8); var track = Transfer(controller);
        Assert.Equal(6250, track.Data.Length); Assert.Equal(0, track.Status & 0x7c);
        var mark = Find(track.Data, [0xa1, 0xa1, 0xa1, 0xfb]); Assert.True(mark >= 0);
        track.Data[mark + 4] = 0x99;
        controller.Write(0, 0xf8); var written = Transfer(controller, track.Data);
        Assert.Equal(0, written.Status & 0x7c); Assert.Equal(0x99, disk.Bytes(1, 1, 0)[0]);
        Assert.NotEqual(0x99, disk.Bytes(1, 0, 0)[0]);
    }

    [Fact]
    public void ProtectionDensityCrcMissingMediaAndUnservicedBytesAreReported()
    {
        using var disk = new Disk { IsReadOnly = true };
        var controller = new Wd1770(_ => disk) { MechanicalSpeedPercent = 0 };
        controller.Write(0, 0xa8); Assert.Equal(64, Transfer(controller).Status & 64); Assert.False(disk.HasChanges);
        controller.DoubleDensity = false; controller.Write(0, 0x88); Assert.Equal(16, Transfer(controller).Status & 16);
        controller.DoubleDensity = true; disk.Bad = true; controller.Write(2, 0); controller.Write(0, 0x88);
        var crc = Transfer(controller); Assert.Equal(8, crc.Status & 8); Assert.Equal(256, crc.Data.Length);
        disk.Bad = false; controller.Write(0, 0x88);
        for (var tick = 0; tick < 20_000; tick++) controller.Tick();
        Assert.False(controller.Active); Assert.Equal(4, controller.Read(0) & 4);
        var absent = new Wd1770(_ => null) { MechanicalSpeedPercent = 0 }; absent.Write(0, 0x88); Assert.Equal(16, Transfer(absent).Status & 16);
    }

    [Fact]
    public void StepRegisterUpdatesAndResetRespectTheMechanicalPositionAndBorrowedImage()
    {
        using var disk = new Disk(); var controller = new Wd1770(_ => disk) { MechanicalSpeedPercent = 0 };
        controller.Write(0, 0x48); Transfer(controller); Assert.Equal(0, controller.Read(1));
        controller.Write(0, 0x58); Transfer(controller); Assert.Equal(1, controller.Read(1));
        controller.Write(0, 0x08); Transfer(controller); Assert.Equal(0, controller.Read(1)); Assert.Equal(4, controller.Read(0) & 4);
        controller.Reset(); Assert.False(controller.Nmi); Assert.False(controller.Motor); Assert.False(disk.Disposed);
    }

    [Fact]
    public void RealSpeedWaitsForMotorSpinupBeforeRequestingBytes()
    {
        using var disk = new Disk(); var controller = new Wd1770(_ => disk);
        controller.Write(2, 0); controller.Write(0, 0x80);
        for (var tick = 0; tick < 400_000; tick++) controller.Tick();
        Assert.True(controller.Active); Assert.False(controller.Drq);
        Assert.Equal(256, Transfer(controller).Data.Length);
    }

    private static void Seek(Wd1770 controller, byte track) { controller.Write(3, track); controller.Write(0, 0x18); Transfer(controller); }
    private static (byte Status, byte[] Data) Transfer(Wd1770 controller, byte[]? write = null)
    {
        var bytes = new List<byte>(); var offset = 0;
        for (var tick = 0; tick < 4_000_000; tick++)
        {
            controller.Tick();
            if (controller.Drq)
            {
                Assert.True(controller.Nmi);
                if (write is null) bytes.Add(controller.Read(3)); else { Assert.True(offset < write.Length); controller.Write(3, write[offset++]); }
                Assert.False(controller.Drq);
            }
            if (!controller.Active)
            {
                Assert.True(controller.Irq); var status = controller.Read(0); Assert.False(controller.Irq);
                if (write is not null) Assert.Equal(write.Length, offset);
                return (status, bytes.ToArray());
            }
        }
        Assert.Fail("WD1770 command timed out."); return default;
    }
    private static int Find(byte[] bytes, byte[] pattern)
    {
        for (var index = 0; index <= bytes.Length - pattern.Length; index++) if (bytes.AsSpan(index, pattern.Length).SequenceEqual(pattern)) return index;
        return -1;
    }

    private sealed class Disk(byte seed = 0) : IEmulationDiskImage
    {
        private readonly Dictionary<long, byte[]> _data = new();
        public IEmulationDriveMechanics Mechanics { get; } = GWGUI.Emulation.EmulationSdk.MediaProcessing.CreateDriveMechanics(GWGUI.Emulation.Constants.EmulationMechanicalSoundConstants.Floppy);
        public string FormatId => "acorn.adfs.640";
        public int SectorSize => 256;
        public long SectorCount => 96;
        public long Cylinders => 3;
        public int Heads => 2;
        public int SectorsPerTrack => 16;
        public bool IsReadOnly { get; init; }
        public bool HasChanges { get; private set; }
        public IReadOnlyDictionary<string, string> Metadata { get; } = new Dictionary<string, string>();
        public void UpdateMetadata(IReadOnlyDictionary<string, string> metadata) => throw new NotSupportedException();
        internal bool Bad { get; set; }
        internal bool Disposed { get; private set; }
        internal byte[] Bytes(int cylinder, int head, int number)
        {
            var index = FindSector(cylinder, head, number)!.Value;
            if (!_data.TryGetValue(index, out var data)) _data[index] = data = Enumerable.Repeat((byte)(seed + index), 256).ToArray();
            return data;
        }
        public long? FindSector(int cylinder, int head, int number) => cylinder is >= 0 and < 3 && head is >= 0 and < 2 && number is >= 0 and < 16 ? (cylinder * 2L + head) * 16 + number : null;
        public ValueTask<EmulationDiskSector> ReadSectorAsync(long index, CancellationToken cancellationToken = default)
        {
            var cylinder = (int)(index / 32); var head = (int)(index / 16 % 2); var number = (int)(index % 16);
            return ValueTask.FromResult(new EmulationDiskSector(index, Bytes(cylinder, head, number), cylinder, head, number, !Bad));
        }
        public void WriteSector(long index, ReadOnlySpan<byte> data) { if (IsReadOnly) throw new UnauthorizedAccessException(); if (data.Length != 256) throw new ArgumentException(nameof(data)); _data[index] = data.ToArray(); HasChanges = true; }
        public Task FlushAsync(CancellationToken cancellationToken = default) { HasChanges = false; return Task.CompletedTask; }
        public void Dispose() { Disposed = true; _data.Clear(); }
    }
}
