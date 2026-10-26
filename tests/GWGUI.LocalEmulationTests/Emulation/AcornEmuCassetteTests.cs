using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Enums;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Factories;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.ModelA.Services;
using BbcCassette = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Services.Cassette;

namespace GWGUI.Tests.Emulation;

/// <summary>Vérifications cassette autonomes : niveaux FSK et ROM synthétiques, sans fichiers ni lecteur externe.</summary>
public sealed class AcornEmuCassetteTests
{
    [Theory]
    [InlineData(1200, 0x95)]
    [InlineData(300, 0x96)]
    public void CassettePassesRealSerialBitsThroughAciaAndSignalsCarrier(int baud, byte control)
    {
        var machine = new ModelAMachine(new byte[16384], new byte[16384]);
        byte[] expected = [0x2a, 0, 0x55, 0xaa, 0xff, 0x34];
        var signal = Signal(expected, baud);
        machine.InsertCassette(signal);
        machine.Write(0xfe08, control); machine.Write(0xfe10, 0x80);
        var received = new List<byte>(); var carrierInterrupt = false;
        for (var tick = 0; tick < (long)signal.Samples.Length * 2_000_000 / signal.SampleRate + 2000; tick++)
        {
            machine.Tick();
            var status = machine.Read(0xfe08);
            if ((status & 4) != 0 && (status & 128) != 0) { carrierInterrupt = true; Assert.True(machine.Irq); }
            if ((status & 1) != 0) { Assert.Equal(0, status & 0x70); received.Add(machine.Read(0xfe09)); }
            else if ((status & 0x84) == 0x84) machine.Read(0xfe09);
        }
        Assert.True(carrierInterrupt); Assert.Equal(expected, received);
        Assert.Equal(EmulationCassetteState.EndOfTape, machine.CassetteTransport.CassetteState);
    }

    [Fact]
    public void MotorAndTransportControlPositionWithoutChangingTheSignal()
    {
        var acia = new Acia6850(2_000_000, 19200);
        var tape = new BbcCassette(acia);
        Assert.Empty(tape.CassetteCommands);
        tape.Insert(Signal([42], 1200));
        for (var tick = 0; tick < 5000; tick++) tape.Tick();
        Assert.Equal(0, tape.Position); Assert.False(tape.CassetteActive); // Play seul ne fournit pas l'alimentation moteur.
        tape.WriteUla(0x80);
        for (var tick = 0; tick < 5000; tick++) tape.Tick();
        Assert.True(tape.Position > 0); Assert.True(tape.CassetteActive); var position = tape.Position;
        tape.ExecuteCassette(EmulationCassetteCommand.Pause);
        Assert.False(tape.CassetteActive);
        for (var tick = 0; tick < 5000; tick++) tape.Tick();
        Assert.Equal(position, tape.Position);
        tape.ExecuteCassette(EmulationCassetteCommand.Play); tape.WriteUla(0);
        for (var tick = 0; tick < 5000; tick++) tape.Tick();
        Assert.Equal(position, tape.Position);
        tape.ExecuteCassette(EmulationCassetteCommand.Rewind);
        for (var tick = 0; tick < 5000; tick++) tape.Tick();
        Assert.Equal(0, tape.Position); Assert.Equal(EmulationCassetteState.Stopped, tape.CassetteState);
        Assert.DoesNotContain(EmulationCassetteCommand.Record, tape.CassetteCommands);
        tape.Eject(); Assert.Equal(EmulationCassetteState.Empty, tape.CassetteState);
    }

    [Fact]
    public void AciaPreservesUnreadDataOnOverrunAndAcknowledgesInterrupts()
    {
        var acia = new Acia6850(2_000_000, 19200); acia.Write(false, 0x95);
        void Receive(byte value) { acia.BeginReceive(); for (var bit = 0; bit < 8; bit++) acia.ReceiveBit((value & (1 << bit)) != 0); acia.ReceiveBit(true); }
        Receive(42); Receive(99);
        Assert.True(acia.Irq); Assert.Equal(0x20, acia.Read(false) & 0x20);
        Assert.Equal(42, acia.Read(true)); Assert.False(acia.Irq);
        acia.SetDcd(true); acia.SetDcd(false); Assert.True(acia.Irq);
        acia.Read(true); Assert.False(acia.Irq);
    }

    [Fact]
    public void ModelAOffersCassetteImagesWithoutAddingFloppyHardware()
    {
        var factory = new AcornEmuMachineFactory();
        var configuration = new MachineConfiguration(MachineModel.BbcMicroModelA, core: Emulator.AcornEmu);
        var storage = factory.DescribeStorage(configuration);
        var device = Assert.Single(storage.AvailableDevices, item => item.Slot == EmulationMediaSlot.Cassette0);
        Assert.Empty(storage.ConfiguredSlots); Assert.Empty(factory.DescribeMediaDevices(configuration));
        Assert.Equal(EmulationMediaSlot.Cassette0, device.Slot);
        Assert.Equal(new[] { ".uef", ".csw", ".wav", ".raw" }, device.AcceptedExtensions);
        Assert.Equal(EmulationStorageConfigurationKind.CassetteDrive, device.ConfigurationKind);
        var connected = factory.ApplyStorage(configuration, storage with { ConfiguredSlots = [device.Slot] });
        Assert.Single(factory.DescribeMediaDevices(connected));
        var restored = new MachineConfiguration(MachineModel.BbcMicroModelA, options: connected.Options, core: Emulator.AcornEmu);
        Assert.Equal(device.Slot, Assert.Single(factory.DescribeStorage(restored).ConfiguredSlots));
        Assert.Empty(restored.Media); // Un lecteur vide reste branché après relecture de sa configuration.
        var mounted = new MachineConfiguration(MachineModel.BbcMicroModelA, media: [new("synthetic.uef", MediaCategory.Cassette, EmulationMediaSlot.Cassette0)], core: Emulator.AcornEmu);
        Assert.Equal("synthetic.uef", Assert.Single(factory.ResolveConfiguredMedia(mounted)).Path);
        var removed = factory.ApplyStorage(mounted, factory.DescribeStorage(mounted) with { ConfiguredSlots = [], MountedMedia = [], DeviceSettings = [] });
        Assert.Empty(factory.DescribeStorage(removed).ConfiguredSlots);
        Assert.Empty(factory.DescribeMediaDevices(removed)); Assert.Empty(removed.Media);
        Assert.Contains(factory.DescribeStorage(removed).AvailableDevices, item => item.Slot == EmulationMediaSlot.Cassette0); // Le lecteur peut être ajouté de nouveau.
    }

    [Theory]
    [InlineData(1200, 0x95)]
    [InlineData(300, 0x96)]
    public void RecordingProducesAReplayableSerialSignalWithoutErasingTheRestOfTheTape(int baud, byte control)
    {
        var original = new short[48000];
        original[^1] = 1234;
        var transmitter = new Acia6850(2_000_000, 19200);
        var writer = new BbcCassette(transmitter);
        writer.Insert(new(original, 48000), writable: true);
        transmitter.Write(false, control); writer.WriteUla(0x80); writer.ExecuteCassette(EmulationCassetteCommand.Record);
        Assert.Equal(baud, transmitter.BaudRate);
        Assert.True(writer.CassetteActive);
        for (var tick = 0; tick < 500000; tick++) writer.Tick();
        transmitter.Write(true, 0xa5);
        for (var tick = 0; tick < 300000; tick++) writer.Tick();
        writer.ExecuteCassette(EmulationCassetteCommand.Stop);
        Assert.True(writer.HasChanges); Assert.False(writer.CassetteActive);
        var signal = writer.RecordedSignal;
        Assert.Equal(48000, signal.Samples.Length); Assert.Equal(1234, signal.Samples.Span[^1]); Assert.Equal(0, original[0]);
        var receiver = new Acia6850(2_000_000, 19200); receiver.Write(false, control);
        var reader = new BbcCassette(receiver); reader.Insert(signal); reader.WriteUla(0x80);
        var bytes = new List<byte>();
        for (var tick = 0; tick < 2_000_000; tick++)
        {
            reader.Tick(); var status = receiver.Read(false);
            if ((status & 1) != 0) bytes.Add(receiver.Read(true));
            else if ((status & 0x84) == 0x84) receiver.Read(true);
        }
        Assert.Equal(new byte[] { 0xa5 }, bytes);
        writer.MarkSaved(); Assert.False(writer.HasChanges);
    }

    [Fact]
    public void WriteProtectionAndMotorPreventRecording()
    {
        var writer = new BbcCassette(new Acia6850(2_000_000, 19200));
        writer.Insert(new(new short[48000], 48000));
        Assert.Throws<NotSupportedException>(() => writer.ExecuteCassette(EmulationCassetteCommand.Record));
        writer.Insert(new(new short[48000], 48000), writable: true);
        writer.ExecuteCassette(EmulationCassetteCommand.Record);
        for (var tick = 0; tick < 5000; tick++) writer.Tick();
        Assert.False(writer.HasChanges); Assert.Equal(0, writer.Position);
        writer.Configure(new(WriteProtected: true));
        Assert.DoesNotContain(EmulationCassetteCommand.Record, writer.CassetteCommands);
        Assert.Equal(EmulationCassetteState.Stopped, writer.CassetteState);
    }

    [Theory]
    [InlineData(22050)]
    [InlineData(44100)]
    [InlineData(48000)]
    public void FractionalPcmCadencesKeepTheSerialBytesReadableByTheHardware(int rate)
    {
        var samples = new List<short>(); short level = 16000;
        GWGUI.MediaEngine.Functions.FskSerialFunctions.AppendPcmCarrier(samples, ref level, rate, 1200, 1200, 2400, 0.25);
        byte[] expected = [0x2a, 0x5a, 0x81, 0, 0xff];
        foreach (var value in expected) GWGUI.MediaEngine.Functions.FskSerialFunctions.AppendPcmFrame(samples, ref level, rate, 1200, 1200, 2400, 8, 1, value);
        var receiver = new Acia6850(2_000_000, 19200); receiver.Write(false, 0x95);
        var reader = new BbcCassette(receiver); reader.Insert(new(samples.ToArray(), rate)); reader.WriteUla(0x80);
        var bytes = new List<byte>();
        for (var tick = 0; tick < (long)samples.Count * 2_000_000 / rate + 2000; tick++)
        {
            reader.Tick(); var status = receiver.Read(false);
            if ((status & 1) != 0) bytes.Add(receiver.Read(true)); else if ((status & 0x84) == 0x84) receiver.Read(true);
        }
        Assert.Equal(expected, bytes);
    }

    [Fact]
    public void WindingTargetsTheNextBlockWithRealTimeOrInstantMovement()
    {
        var tape = new BbcCassette(new Acia6850(2_000_000, 19200));
        tape.Insert(new(new short[192000], 48000, [0, 48000, 96000, 144000]));
        tape.ExecuteCassette(EmulationCassetteCommand.FastForward);
        Assert.Equal(0, tape.Position); Assert.True(tape.CassetteActive);
        for (var tick = 0; tick < 25000; tick++) tape.Tick();
        Assert.InRange(tape.Position, 1, 47999);
        for (var tick = 0; tick < 75000; tick++) tape.Tick();
        Assert.Equal(48000, tape.Position); Assert.False(tape.CassetteActive);
        tape.Configure(new(Mode: EmulationCassettePlaybackMode.Accelerated));
        tape.ExecuteCassette(EmulationCassetteCommand.FastForward); Assert.Equal(96000, tape.Position);
        tape.ExecuteCassette(EmulationCassetteCommand.Rewind); Assert.Equal(48000, tape.Position);
    }

    [Theory]
    [InlineData(false, 200000)]
    [InlineData(true, 400000)]
    public void ReadAndWriteStartupDelaysUseTheEmulatedClock(bool recording, int delayCycles)
    {
        var tape = new BbcCassette(new Acia6850(2_000_000, 19200));
        tape.Configure(new(ReadLatencyMilliseconds: 100, WriteLatencyMilliseconds: 200));
        tape.Insert(Signal([42], 1200), writable: true); tape.WriteUla(0x80);
        if (recording) tape.ExecuteCassette(EmulationCassetteCommand.Record);
        for (var tick = 0; tick < delayCycles; tick++) tape.Tick();
        Assert.Equal(0, tape.Position); Assert.False(tape.HasChanges);
        for (var tick = 0; tick < 5000; tick++) tape.Tick();
        Assert.True(tape.Position > 0); Assert.Equal(recording, tape.HasChanges);
    }

    [Fact]
    public void DirectLoadingRequiresCompleteValidatedFileBlocks()
    {
        Dictionary<string, string> Header(int number, string flags) => new() { ["fileName"] = "TEST", ["blockNumber"] = number.ToString(), ["flags"] = flags, ["loadAddress"] = "ffff1900", ["executionAddress"] = "ffff1900" };
        var tape = new BbcCassette(new Acia6850(2_000_000, 19200));
        tape.Configure(new(Mode: EmulationCassettePlaybackMode.Direct));
        tape.Insert(new(new short[1000], 48000, [0, 500], [new(0, new byte[] { 1, 2 }, true, Header(0, "00")), new(500, new byte[] { 3, 4 }, true, Header(1, "80"))]));
        Assert.True(tape.TryReadFile("TEST", out var bytes, out var load, out _, out _));
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, bytes); Assert.Equal(0xffff1900u, load);
        tape.Insert(new(new short[1000], 48000, [0], [new(0, new byte[] { 1, 2 }, false, Header(0, "80"))]));
        Assert.False(tape.TryReadFile("TEST", out _, out _, out _, out _)); Assert.Equal(0, tape.Position);
    }

    [Theory]
    [InlineData(0x1900, true)]
    [InlineData(0xff00, false)]
    public void DirectOsfileLoadingValidatesTheRamDestinationAndBalancesTheCpuStack(int destination, bool accepted)
    {
        var mos = Enumerable.Repeat((byte)0xea, 16384).ToArray();
        mos[0x3ffc] = 0; mos[0x3ffd] = 0x80; mos[0x3fdd] = 0x60; // RESET vers BASIC synthétique ; OSFILE matériel revient par RTS.
        var basic = new byte[16384];
        byte[] program = [0xa2, 0x00, 0xa0, 0x02, 0xa9, 0xff, 0x20, 0xdd, 0xff, 0x85, 0x70, 0x4c, 0x0b, 0x80];
        program.CopyTo(basic, 0);
        var machine = new ModelAMachine(mos, basic); machine.ConfigureCassette(new(Mode: EmulationCassettePlaybackMode.Direct));
        var header = new Dictionary<string, string> { ["fileName"] = "TEST", ["blockNumber"] = "0", ["flags"] = "80", ["loadAddress"] = "ffff1900", ["executionAddress"] = "ffff1900" };
        machine.InsertCassette(new(new short[1000], 48000, [0], [new(0, new byte[] { 7, 8, 9 }, true, header)]));
        machine.Write(0x200, 0); machine.Write(0x201, 3); machine.Write(0x202, (byte)destination); machine.Write(0x203, (byte)(destination >> 8));
        byte[] name = [(byte)'T', (byte)'E', (byte)'S', (byte)'T', 13];
        for (var index = 0; index < name.Length; index++) machine.Write((ushort)(0x300 + index), name[index]);
        machine.RunFrame();
        Assert.Equal(accepted ? 1 : 255, machine.Read(0x70)); Assert.Equal(0xfd, machine.Cpu.S);
        Assert.Equal(accepted ? EmulationCassetteState.Stopped : EmulationCassetteState.Playing, machine.CassetteTransport.CassetteState);
        if (accepted) Assert.Equal(new byte[] { 7, 8, 9 }, new[] { machine.Read(0x1900), machine.Read(0x1901), machine.Read(0x1902) });
    }

    /// <summary>Construit des ondes indépendamment du convertisseur MediaEngine : porteuse, 8N1, puis silence.</summary>
    internal static EmulationCassetteSignal Signal(byte[] bytes, int baud)
    {
        var samples = new List<short>(); short level = 16000;
        void Bit(bool value)
        {
            var frequency = value ? 2400 : 1200;
            for (var half = 0; half < frequency * 2 / baud; half++)
            {
                for (var sample = 0; sample < 48000 / (2 * frequency); sample++) samples.Add(level);
                level = (short)-level;
            }
        }
        for (var bit = 0; bit < baud / 4; bit++) Bit(true);
        foreach (var value in bytes) { Bit(false); for (var bit = 0; bit < 8; bit++) Bit((value & (1 << bit)) != 0); Bit(true); }
        samples.AddRange(new short[480]);
        return new EmulationCassetteSignal(samples.ToArray(), 48000);
    }
}
