using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Interfaces;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Contracts;
using System.IO;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Enums;
using GWGUI.Emulation.Acorn.Common.Services;

namespace GWGUI.Tests.Emulation;

/// <summary>Vérifications autonomes : aucun firmware, fichier, processus ou périphérique externe.</summary>
public sealed class AcornEmuTests
{
    [Fact]
    public void HostSpacePreservesHeldShift()
    {
        var keyboard = new Keyboard();
        keyboard.SetInput(new HashSet<EmulationKey> { EmulationKey.Space, EmulationKey.LeftShift }, new Dictionary<EmulationKey, char> { [EmulationKey.Space] = ' ' });
        Assert.Equal(128, keyboard.Read(0) & 128);
        Assert.Equal(128, keyboard.Read(0x62) & 128);
    }
    [Fact]
    public void HostCharactersSurviveProcessProtocolWithoutConsumingNextSnapshot()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true);
        var input = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.D3 }, HostCharacters = new Dictionary<EmulationKey, char> { [EmulationKey.D3] = '"' } };
        GWGUI.Emulation.EmulationSdk.HostProtocol.WriteInput(writer, input);
        GWGUI.Emulation.EmulationSdk.HostProtocol.WriteInput(writer, EmulationInputSnapshot.Empty);
        writer.Flush(); stream.Position = 0;
        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, true);
        var received = GWGUI.Emulation.EmulationSdk.HostProtocol.ReadInput(reader, "test");
        Assert.Equal('"', received.HostCharacters![EmulationKey.D3]);
        Assert.Contains(EmulationKey.D3, received.Keys);
        Assert.Null(GWGUI.Emulation.EmulationSdk.HostProtocol.ReadInput(reader, "test").HostCharacters);
        Assert.Equal(stream.Length, stream.Position);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(513)]
    public void ProcessProtocolRejectsInvalidHostCharacterCounts(int count)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true);
        writer.Write(0); writer.Write(count); writer.Flush(); stream.Position = 0;
        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, true);
        Assert.Throws<InvalidDataException>(() => GWGUI.Emulation.EmulationSdk.HostProtocol.ReadInput(reader, "test"));
    }
    [Theory]
    [InlineData(EmulationKey.D3, '"', false, 0x31, true)]
    [InlineData(EmulationKey.D3, '3', true, 0x11, false)]
    [InlineData(EmulationKey.D2, '@', true, 0x47, false)]
    [InlineData(EmulationKey.Equals, '=', false, 0x17, true)]
    [InlineData(EmulationKey.Oem102, '<', false, 0x66, true)]
    public void HostSymbolsChooseBbcContactAndShift(EmulationKey source, char character, bool hostShift, int position, bool bbcShift)
    {
        var keyboard = new Keyboard();
        var keys = new HashSet<EmulationKey> { source };
        if (hostShift) keys.Add(EmulationKey.LeftShift);
        keyboard.SetInput(keys, new Dictionary<EmulationKey, char> { [source] = character });
        Assert.Equal(128, keyboard.Read((byte)position) & 128);
        Assert.Equal(bbcShift ? 128 : 0, keyboard.Read(0) & 128);
        keyboard.SetInput(new HashSet<EmulationKey>());
        Assert.Equal(0, keyboard.Read((byte)position) & 128);
        Assert.Equal(0, keyboard.Read(0) & 128);
    }

    [Theory]
    [InlineData(EmulationKey.Numpad3, 0x11, false)]
    [InlineData(EmulationKey.NumpadPlus, 0x57, true)]
    [InlineData(EmulationKey.NumpadMultiply, 0x48, true)]
    [InlineData(EmulationKey.NumpadDivide, 0x68, false)]
    [InlineData(EmulationKey.NumpadPeriod, 0x67, false)]
    [InlineData(EmulationKey.NumpadEnter, 0x49, false)]
    public void NumericKeypadUsesExistingBbcKeys(EmulationKey key, int position, bool shift)
    {
        var keyboard = new Keyboard();
        keyboard.SetInput(new HashSet<EmulationKey> { key });
        Assert.Equal(128, keyboard.Read((byte)position) & 128);
        Assert.Equal(shift ? 128 : 0, keyboard.Read(0) & 128);
    }

    [Fact]
    public void UnsupportedHostCharacterDoesNotTypeItsPhysicalDigit()
    {
        var keyboard = new Keyboard();
        keyboard.SetInput(new HashSet<EmulationKey> { EmulationKey.D2 }, new Dictionary<EmulationKey, char> { [EmulationKey.D2] = 'é' });
        Assert.False(keyboard.ColumnPressed(1));
    }

    [Fact]
    public void HostCharactersSurviveCommonAndAreCopiedByFrameStorage()
    {
        var characters = new Dictionary<EmulationKey, char> { [EmulationKey.D3] = '"' };
        var source = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.D3 }, HostCharacters = characters };
        var mapped = GWGUI.Emulation.Acorn.Common.Machines.Common.Functions.InputSnapshotFunctions.Apply(source, (InputConfiguration?)null);
        var store = new GWGUI.Emulation.Services.EmulationInputFrameStore();
        store.Update(mapped);
        characters[EmulationKey.D3] = '3';
        store.Poll();
        Assert.Contains(EmulationKey.D3, store.Polled.Keys);
        Assert.Equal('"', store.Polled.HostCharacters![EmulationKey.D3]);
        store.Update(EmulationInputSnapshot.Empty); store.Poll();
        Assert.Null(store.Polled.HostCharacters);
    }

    [Fact]
    public void CommonDiscoversModelAAndPreservesExternalFirmwareOptions()
    {
        var engine = new Engine();
        var configuration = new MachineConfiguration(MachineModel.BbcMicroModelA);
        Assert.Equal(Emulator.AcornEmu, configuration.Core);
        var updated = Assert.IsType<MachineConfiguration>(engine.ApplySettings(configuration, new Dictionary<string, string?>
        {
            ["acornemu.mos"] = "external-mos.rom",
            ["acornemu.basic"] = "external-basic.rom"
        }));
        Assert.Equal("external-mos.rom", updated.Options["acornemu.mos"]);
        Assert.Equal("external-basic.rom", updated.Options["acornemu.basic"]);
    }
    [Fact]
    public void CpuAddsWithCarryAndWritesResult()
    {
        var bus = new MemoryBus();
        byte[] program = [0xa9, 0xff, 0x18, 0x69, 0x01, 0x85, 0x20];
        program.CopyTo(bus.Bytes, 0x200);
        var cpu = Start(bus);
        for (var i = 0; i < 4; i++) cpu.Step();
        Assert.Equal(0, bus.Bytes[0x20]);
        Assert.Equal(3, cpu.P & 3);
    }
    [Fact]
    public void IndirectJumpRetainsNmosPageWrap()
    {
        var bus = new MemoryBus();
        bus.Bytes[0x200] = 0x6c; bus.Bytes[0x201] = 0xff; bus.Bytes[0x202] = 0x12;
        bus.Bytes[0x12ff] = 0x34; bus.Bytes[0x1200] = 0x56; bus.Bytes[0x1300] = 0x78;
        var cpu = Start(bus);
        cpu.Step();
        Assert.Equal(0x5634, cpu.PC);
    }
    [Fact]
    public void DecimalSubtractionAndAdditionUseBcd()
    {
        var bus = new MemoryBus();
        byte[] program = [0xf8, 0x18, 0xa9, 0x45, 0x69, 0x55, 0x85, 0x20, 0x38, 0xe9, 0x01, 0x85, 0x21];
        program.CopyTo(bus.Bytes, 0x200);
        var cpu = Start(bus);
        for (var i = 0; i < 8; i++) cpu.Step();
        Assert.Equal(0, bus.Bytes[0x20]);
        Assert.Equal(0x99, bus.Bytes[0x21]);
    }
    [Fact]
    public void ViaAcknowledgesTimerInterruptOnlyThroughItsRegister()
    {
        var via = new Via6522();
        via.Reset(); via.Write(14, 0xc0); via.Write(4, 1); via.Write(5, 0);
        via.Tick(); via.Tick();
        Assert.True(via.Irq);
        Assert.Equal(0xc0, via.Read(13));
        via.Read(4);
        Assert.False(via.Irq);
    }
    [Fact]
    public void ModelAHasOneRamBankAndDoesNotWriteRom()
    {
        var mos = new byte[16384];
        var basic = new byte[16384];
        basic[0] = 0x55;
        var machine = new ModelAMachine(mos, basic);
        machine.Write(0x4001, 0x6a);
        machine.Write(0x8000, 0xff);
        Assert.Equal(0x6a, machine.Read(0x0001));
        Assert.Equal(0x55, machine.Read(0x8000));
    }
    private static Cpu6502 Start(MemoryBus bus)
    {
        bus.Bytes[0xfffc] = 0; bus.Bytes[0xfffd] = 2;
        var cpu = new Cpu6502(bus); cpu.Reset(); return cpu;
    }
    [Fact]
    public void ModeFourExpandsEachMemoryBitToTwoDisplayPixels()
    {
        var machine = new ModelAMachine(new byte[16384], new byte[16384]);
        void Register(byte index, byte value) { machine.Write(0xfe00, index); machine.Write(0xfe01, value); }
        Register(1, 40); Register(6, 32); Register(9, 7); Register(12, 0x0b); Register(13, 0);
        machine.Write(0xfe20, 0x88); machine.Write(0xfe21, 0xf0); machine.Write(0x5800, 0x80);
        var image = machine.Video.Render();
        Assert.Equal(255, image[2]);
        Assert.Equal(255, image[6]);
        Assert.Equal(0, image[10]);
    }
    private sealed class MemoryBus : ICpuBus
    {
        internal byte[] Bytes { get; } = new byte[65536];
        public bool Irq => false;
        public byte Read(ushort address) => Bytes[address];
        public void Write(ushort address, byte value) => Bytes[address] = value;
        public void Tick() { }
    }
}
