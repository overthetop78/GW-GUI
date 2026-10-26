using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Interfaces;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Contracts;
using System.IO;
using BbcFirmware = GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Functions.BbcMicroFirmwareFunctions;
using NativeFirmware = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions.FirmwareFunctions;
using NativeModelCatalog = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Dictionaries.ModelCatalog;
using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Dictionaries;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.ModelA.Services;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Services;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Enums;
using GWGUI.Emulation.Acorn.Common.Services;

namespace GWGUI.Tests.Emulation;

/// <summary>Vérifications autonomes : aucun firmware, fichier, processus ou périphérique externe.</summary>
public sealed class AcornEmuTests
{
    [Fact]
    public void NmiIsEdgeTriggeredAndReturnsThroughTheNormalStack()
    {
        var bus = new MemoryBus(); Array.Fill(bus.Bytes, (byte)0xea, 0x200, 16);
        bus.Bytes[0xfffa] = 0; bus.Bytes[0xfffb] = 3;
        bus.Bytes[0x300] = 0xe6; bus.Bytes[0x301] = 0x10; bus.Bytes[0x302] = 0x40;
        var cpu = Start(bus); bus.Nmi = true;
        cpu.Step(); Assert.Equal(0x201, cpu.PC);
        var cycles = cpu.Cycles;
        cpu.Step(); Assert.Equal(0x300, cpu.PC); Assert.Equal(cycles + 7, cpu.Cycles);
        Assert.Equal(0x02, bus.Bytes[0x1fd]); Assert.Equal(0x01, bus.Bytes[0x1fc]); Assert.Equal(0, bus.Bytes[0x1fb] & 0x10);
        cpu.Step(); cpu.Step(); Assert.Equal(1, bus.Bytes[0x10]); Assert.Equal(0x201, cpu.PC);
        cpu.Step(); Assert.Equal(0x202, cpu.PC); // NMI toujours haute : aucune seconde interruption.
        bus.Nmi = false; cpu.Step(); bus.Nmi = true; cpu.Step(); cpu.Step(); Assert.Equal(0x300, cpu.PC);
    }

    [Fact]
    public void NmiTakesPriorityOverAnUnmaskedIrq()
    {
        var bus = new MemoryBus(); bus.Bytes[0x200] = 0x58; // CLI
        bus.Bytes[0xfffa] = 0; bus.Bytes[0xfffb] = 3; bus.Bytes[0x300] = 0x40;
        bus.Bytes[0xfffe] = 0; bus.Bytes[0xffff] = 4;
        var cpu = Start(bus); bus.Irq = true; bus.Nmi = true;
        cpu.Step(); cpu.Step(); Assert.Equal(0x300, cpu.PC);
        cpu.Step(); Assert.Equal(0x201, cpu.PC);
        cpu.Step(); Assert.Equal(0x400, cpu.PC);
    }

    [Theory]
    [InlineData("0d9bcaf6a393c9ce2359ed700ddb53c232c2c45d", EmulationFirmwareCompatibility.Official)]
    [InlineData("890f6e3e7fab3340f75b85e93ff29332bc9ecb2e", EmulationFirmwareCompatibility.Official)]
    [InlineData("4a7393f3a45ea309f744441c16723e2ef447a281", EmulationFirmwareCompatibility.Official)]
    [InlineData("4b0ece6dc139d5d3f4fabd023716fb6f25149b80", EmulationFirmwareCompatibility.Official)]
    [InlineData("dca33995c0d008a527efe923d03333394b01022c", EmulationFirmwareCompatibility.Official)]
    [InlineData("d35f6723132aabe3c4d00fc16fd9ecc6768df753", EmulationFirmwareCompatibility.Official)]
    [InlineData("2b6aecd33a43f296c20832524e47cc7e3a9c3b17", EmulationFirmwareCompatibility.PartiallyCompatible)]
    [InlineData("b369809275cb67dfd8a749265e91adb2d2558ae6", EmulationFirmwareCompatibility.Incompatible)]
    public void FirmwareCompatibilityBelongsToModelAInsteadOfTheWholeEmulator(string sha1, EmulationFirmwareCompatibility compatibility)
    {
        Assert.Equal(compatibility, BbcMicroFirmwareCompatibilityCatalog.Get(MachineModel.BbcMicroModelA, sha1));
        Assert.Equal(EmulationFirmwareCompatibility.Unknown, BbcMicroFirmwareCompatibilityCatalog.Get(MachineModel.BbcMicroModelB, sha1));
        Assert.Equal(EmulationFirmwareCompatibility.Unknown, BbcMicroFirmwareCompatibilityCatalog.Get(MachineModel.BbcMicroModelA, new string('0', 40)));
    }

    [Fact]
    public void AcornEmuSelectsRegisteredModelsWithoutSubstitutingModelA()
    {
        var factory = new GWGUI.Emulation.Acorn.Emulators.AcornEmu.Factories.AcornEmuMachineFactory();
        Assert.True(factory.Definition.MachineIds.SetEquals(NativeModelCatalog.All.Select(model => model.Model.ToString())));
        var model = NativeModelCatalog.Get(MachineModel.BbcMicroModelA);
        Assert.Equal(640, model.Width); Assert.Equal(256, model.Height);
        Assert.InRange(model.FramesPerSecond, 50.08, 50.09);
        Assert.Throws<NotSupportedException>(() => NativeModelCatalog.Get((MachineModel)int.MaxValue));
        Assert.All(factory.GetFirmwareSlots(new MachineConfiguration(MachineModel.BbcMicroModelA)), slot => Assert.Contains(nameof(MachineModel.BbcMicroModelA), slot.MachineIds!));
    }

    [Theory]
    [InlineData(8192, 1)]
    [InlineData(16384, 2)]
    [InlineData(32768, 2)]
    public void UnknownRomRemainsVisibleAndSelectable(int size, int candidates)
    {
        var image = new byte[size];
        var entries = BbcFirmware.Inspect(image, "unknown.rom", MachineModel.BbcMicroModelA, NativeFirmware.Field);
        Assert.Equal(candidates, entries.Count);
        Assert.All(entries, entry =>
        {
            Assert.Equal(EmulationFirmwareCompatibility.Unknown, entry.Compatibility);
            Assert.NotNull(entry.DestinationFieldId);
            Assert.Equal(16384, BbcFirmware.GetRomBytes(image, NativeFirmware.Role(entry.DestinationFieldId!)).Length);
        });
    }

    [Fact]
    public void UnknownSidewaysRomIsRecognizedFromHeaderInsteadOfFilename()
    {
        var image = new byte[16384];
        image[6] = 0x60; image[7] = 14; image[14] = 0; image[15] = (byte)'('; image[16] = (byte)'C'; image[17] = (byte)')';
        var entry = Assert.Single(BbcFirmware.Inspect(image, "renamed.bin", MachineModel.BbcMicroModelA, NativeFirmware.Field));
        Assert.Equal("acornemu.basic", entry.DestinationFieldId);
        Assert.Equal(EmulationFirmwareCompatibility.Unknown, entry.Compatibility);
    }

    [Fact]
    public void UnknownMosIsRecognizedFromResetVectorInsteadOfFilename()
    {
        var image = new byte[16384]; image[^4] = 0xcd; image[^3] = 0xd9;
        var entry = Assert.Single(BbcFirmware.Inspect(image, "renamed.bin", MachineModel.BbcMicroModelA, NativeFirmware.Field));
        Assert.Equal("acornemu.mos", entry.DestinationFieldId);
    }

    [Fact]
    public void ServiceRomUsesTheOptionalDiskBankAndKnownDfsRequiresTheMachineExtension()
    {
        var image = new byte[8192]; image[6] = 0x82; image[7] = 16;
        image[17] = (byte)'('; image[18] = (byte)'C'; image[19] = (byte)')';
        var candidate = Assert.Single(BbcFirmware.Inspect(image, "service.rom", MachineModel.BbcMicroModelA, NativeFirmware.Field));
        Assert.Equal("acornemu.dfs", candidate.DestinationFieldId); Assert.Equal(EmulationFirmwareCompatibility.Unknown, candidate.Compatibility);
        Assert.Equal(16384, BbcFirmware.GetRomBytes(image, FirmwareCategory.DiscFilingSystem).Length);
        Assert.Equal(EmulationFirmwareCompatibility.PartiallyCompatible, BbcMicroFirmwareCompatibilityCatalog.Get(MachineModel.BbcMicroModelA, "dbbec4d2540a854c120be3194c7566a2b79d153b"));
        Assert.Throws<InvalidDataException>(() => BbcFirmware.GetRomBytes(new byte[32768], FirmwareCategory.DiscFilingSystem));
    }

    [Fact]
    public void CombinedAndEightKibRomsPreserveTheirDocumentedLayout()
    {
        var combined = new byte[32768]; combined[0] = 0x12; combined[16384] = 0x34;
        Assert.Equal(0x12, BbcFirmware.GetRomBytes(combined, FirmwareCategory.Basic)[0]);
        Assert.Equal(0x34, BbcFirmware.GetRomBytes(combined, FirmwareCategory.OperatingSystem)[0]);
        var small = new byte[8192]; small[8191] = 0x56;
        var padded = BbcFirmware.GetRomBytes(small, FirmwareCategory.Basic);
        Assert.Equal(0x56, padded[8191]); Assert.All(padded[8192..], value => Assert.Equal(255, value));
        Assert.Throws<InvalidDataException>(() => BbcFirmware.GetRomBytes(small, FirmwareCategory.OperatingSystem));
        Assert.Throws<InvalidDataException>(() => BbcFirmware.GetRomBytes(new byte[42], FirmwareCategory.Basic));
        Assert.Equal(EmulationFirmwareCompatibility.Unknown, Assert.Single(BbcFirmware.Inspect(new byte[42], "bad.rom", MachineModel.BbcMicroModelA, NativeFirmware.Field)).Compatibility);
    }

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

    [Theory]
    [InlineData(16384, true)]
    [InlineData(32768, false)]
    public void ModelARamExpansionChangesCpuAndVideoAddressing(int bytes, bool mirrored)
    {
        var machine = new ModelAMachine(new byte[16384], new byte[16384], bytes);
        machine.Write(0x0001, 0x12);
        machine.Write(0x4001, 0x34);
        Assert.Equal(mirrored ? 0x34 : 0x12, machine.Read(0x0001));
        Assert.Equal(0x34, machine.Read(0x4001));
        void Register(byte index, byte value) { machine.Write(0xfe00, index); machine.Write(0xfe01, value); }
        Register(0, 63); Register(1, 40); Register(4, 38); Register(6, 32); Register(9, 7); Register(10, 32); Register(12, 0x0b); Register(13, 0);
        machine.Write(0xfe20, 0x88); machine.Write(0xfe21, 0xf0);
        machine.Write(0x1800, 0); machine.Write(0x5800, 0x80);
        for (var tick = 0; tick < 39936 * 2; tick++) machine.Video.Tick();
        var image = machine.Video.Render();
        Assert.Equal(255, image[2]);
        Assert.Equal(mirrored ? 0x80 : 0, machine.Read(0x1800));
        machine.Reset(true);
        Assert.Equal(0, machine.Read(0x4001));
    }

    [Fact]
    public void ModelAMemorySettingIsAppliedValidatedAndSummarized()
    {
        const string fieldId = "acorn_main_memory";
        var configuration = new MachineConfiguration(MachineModel.BbcMicroModelA, core: Emulator.AcornEmu);
        var engine = new Engine();
        var fields = GWGUI.Emulation.Acorn.Common.Machines.Common.Functions.SettingsDescriptionFunctions.Create(configuration).SelectMany(block => block.Fields);
        var field = Assert.Single(fields, field => field.Id == fieldId);
        Assert.Equal("16", field.Value);
        Assert.Equal(new[] { "16", "32" }, field.Choices!.Select(choice => choice.Id));
        Assert.True(field.RequiresRestart);
        var expanded = Assert.IsType<MachineConfiguration>(engine.ApplySettings(configuration, new Dictionary<string, string?> { [fieldId] = "32" }));
        var adapter = (GWGUI.Emulation.Acorn.Common.Interfaces.IEmulatorSettings)new GWGUI.Emulation.Acorn.Emulators.AcornEmu.Factories.AcornEmuMachineFactory();
        Assert.Equal(32m, adapter.GetMemoryKib(expanded));
        Assert.Contains("RAM 32 KiB", adapter.Summarize(expanded).Details);
        Assert.Throws<ArgumentException>(() => engine.ApplySettings(configuration, new Dictionary<string, string?> { [fieldId] = "64" }));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ModelAMachine(new byte[16384], new byte[16384], 65536));
    }
    [Fact]
    public void ModeFourExpandsEachMemoryBitToTwoDisplayPixels()
    {
        var machine = new ModelAMachine(new byte[16384], new byte[16384]);
        void Register(byte index, byte value) { machine.Write(0xfe00, index); machine.Write(0xfe01, value); }
        Register(0, 63); Register(1, 40); Register(4, 38); Register(6, 32); Register(9, 7); Register(10, 32); Register(12, 0x0b); Register(13, 0);
        machine.Write(0xfe20, 0x88); machine.Write(0xfe21, 0xf0); machine.Write(0x5800, 0x80);
        for (var tick = 0; tick < 39936 * 2; tick++) machine.Video.Tick();
        var image = machine.Video.Render();
        Assert.Equal(255, image[2]);
        Assert.Equal(255, image[6]);
        Assert.Equal(0, image[10]);
    }
    private sealed class MemoryBus : ICpuBus
    {
        internal byte[] Bytes { get; } = new byte[65536];
        public bool Irq { get; set; }
        public bool Nmi { get; set; }
        public byte Read(ushort address) => Bytes[address];
        public void Write(ushort address, byte value) => Bytes[address] = value;
        public void Tick() { }
    }
}
