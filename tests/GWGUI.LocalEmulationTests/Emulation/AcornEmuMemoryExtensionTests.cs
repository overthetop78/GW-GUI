using GWGUI.Emulation.Acorn.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Enums;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Constants;
using GWGUI.Emulation.Acorn.Common.Services;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Contracts;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Factories;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.ModelA.Services;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.MemoryExtensionConstants;

namespace GWGUI.Tests.Emulation;

/// <summary>Bus des cartes mémoire et configuration sans fichier ni ROM propriétaire.</summary>
public sealed class AcornEmuMemoryExtensionTests
{
    [Theory]
    [InlineData(C.AriesB20)]
    [InlineData(C.Watford32)]
    public void CardRamAndScreenRamRemainIndependentAndLowRamStaysShared(string card)
    {
        var machine = Machine(card);
        machine.Write(0x3000, 0x11);
        machine.Write(0x7fff, 0x12);
        machine.Write(C.ShadowRamSelect, 0x55);
        machine.Write(0x3000, 0x21);
        machine.Write(0x7fff, 0x22);
        machine.Write(0x0200, 0x33);
        machine.Write(C.MainRamSelect, 0xaa);
        Assert.Equal(0x11, machine.Read(0x3000));
        Assert.Equal(0x12, machine.Read(0x7fff));
        Assert.Equal(0x33, machine.Read(0x0200));
        machine.Write(C.ShadowRamSelect, 0);
        Assert.Equal(0x21, machine.Read(0x3000));
        Assert.Equal(0x22, machine.Read(0x7fff));
        Assert.Equal(0x33, machine.Read(0x0200));
    }

    [Theory]
    [InlineData(C.AriesB20)]
    [InlineData(C.Watford32)]
    public void CpuProgramSwitchesTheBoardThroughItsRealControlAddresses(string card)
    {
        var mos = new byte[16384]; mos[0x3ffc] = 0; mos[0x3ffd] = 2;
        var machine = new ModelAMachine(mos, new byte[16384], memoryBytes: 32768, memoryExtension: new(card));
        byte[] code = [0xa9,0x11, 0x8d,0x00,0x30, 0x8d,0xff,0xff, 0xa9,0x22, 0x8d,0x00,0x30, 0x8d,0xfe,0xff, 0xad,0x00,0x30];
        for (var i = 0; i < code.Length; i++) machine.Write((ushort)(0x0200 + i), code[i]);
        for (var i = 0; i < 7; i++) machine.Cpu.Step();
        Assert.Equal(0x11, machine.Cpu.A);
        machine.Write(C.ShadowRamSelect, 0);
        Assert.Equal(0x22, machine.Read(0x3000));
        Assert.Equal(0, machine.Read(0xfffc));
        Assert.Equal(2, machine.Read(0xfffd));
    }

    [Fact]
    public void WatfordFullRamSelectionIncludesZeroPageAndAnUnmappedSelectionDoesNotWrite()
    {
        var machine = Machine(C.Watford32);
        machine.Write(0x0002, 0x41);
        machine.Write(C.FullRamSelect, 0);
        Assert.Equal(0, machine.Read(0x0002));
        machine.Write(0x0002, 0x51);
        machine.Write(0x3000, 0x52);
        machine.Write(C.MainRamSelect, 0);
        Assert.Equal(0x41, machine.Read(0x0002));
        Assert.Equal(0, machine.Read(0x3000));
        machine.Write(0xfffd, 0);
        Assert.Equal(255, machine.Read(0x0002));
        machine.Write(0x0002, 0x61);
        machine.Write(C.FullRamSelect, 0);
        Assert.Equal(0x51, machine.Read(0x0002));
        Assert.Equal(0x52, machine.Read(0x3000));
    }

    [Theory]
    [InlineData(C.AriesB20)]
    [InlineData(C.Watford32)]
    public void BreakPreservesBoardStateAndColdResetClearsIt(string card)
    {
        var machine = Machine(card);
        machine.Write(C.ShadowRamSelect, 0);
        machine.Write(0x3000, 0x71);
        machine.Reset(false);
        Assert.Equal(0x71, machine.Read(0x3000));
        machine.Reset(true);
        Assert.Equal(0, machine.Read(0x3000));
        machine.Write(C.ShadowRamSelect, 0);
        Assert.Equal(0, machine.Read(0x3000));
    }

    [Fact]
    public void NoBoardKeepsOriginalRamAliasingAndDoesNotInterpretControlWrites()
    {
        var machine = new ModelAMachine(new byte[16384], new byte[16384]);
        machine.Write(0x0123, 0x81);
        machine.Write(C.ShadowRamSelect, 0);
        Assert.Equal(0x81, machine.Read(0x4123));
        machine.Write(C.FullRamSelect, 0);
        Assert.Equal(0x81, machine.Read(0x0123));
    }

    [Fact]
    public void ExpansionRequiresMainRamAndDoesNotReplaceOrEraseConfiguredRomPaths()
    {
        var factory = new AcornEmuMachineFactory();
        var engine = new Engine();
        var plain = new MachineConfiguration(MachineModel.BbcMicroModelA, core: Emulator.AcornEmu);
        var block = Assert.Single(factory.DescribeSettings(plain), b => b.Id == C.Group);
        Assert.False(Assert.Single(block.Fields, f => f.Id == C.CardOption).IsEnabled);
        Assert.Throws<ArgumentException>(() => MemoryExtensionFunctions.Read(plain with { Options = new Dictionary<string,string> { [C.CardOption] = C.AriesB20 } }));
        var configured = Configuration(C.Watford32);
        configured = (MachineConfiguration)engine.ApplySettings(configured, new Dictionary<string,string?> { [C.Firmware] = "memory.rom" });
        Assert.False(Assert.Single(factory.DescribeSettings(configured).SelectMany(b => b.Fields), f => f.Id == SettingsConstants.MainMemory).IsEnabled);
        Assert.True(Assert.Single(factory.GetFirmwareSlots(configured), slot => slot.FieldId == C.Firmware).IsRequired);
        configured = (MachineConfiguration)engine.ApplySettings(configured, new Dictionary<string,string?> { [C.CardOption] = C.None });
        Assert.Equal("memory.rom", configured.Options[C.Firmware]);
        Assert.True(Assert.Single(factory.DescribeSettings(configured).SelectMany(b => b.Fields), f => f.Id == SettingsConstants.MainMemory).IsEnabled);
        Assert.DoesNotContain(factory.DescribeSettings(configured).Where(b => b.Tab == EmulationMachineTab.Rom).SelectMany(b => b.Fields), f => f.Id == C.Firmware);
        Assert.Equal(0, new MemoryExtensionSettings(C.None).Bytes);
        Assert.Equal(20 * 1024, new MemoryExtensionSettings(C.AriesB20).Bytes);
        Assert.Equal(32 * 1024, new MemoryExtensionSettings(C.Watford32).Bytes);
    }

    [Fact]
    public void KnownMemoryFirmwareIsRejectedForTheOtherBoard()
    {
        var candidate = new EmulationFirmwareCandidate("2590596e0864b97b30050c33b21aa89bb8956204:MemoryExtension", "aries.rom", C.AriesName, "2.4", EmulationFirmwareCompatibility.PartiallyCompatible, C.Firmware);
        Assert.Equal(EmulationFirmwareCompatibility.Incompatible, MemoryExtensionFunctions.Destination(Configuration(C.Watford32), candidate).Compatibility);
        Assert.Equal(EmulationFirmwareCompatibility.PartiallyCompatible, MemoryExtensionFunctions.Destination(Configuration(C.AriesB20), candidate).Compatibility);
    }

    [Theory]
    [InlineData(C.None, 32, 0)]
    [InlineData(C.AriesB20, 52, 20)]
    [InlineData(C.Watford32, 64, 32)]
    public void TotalIncludesTheBoardWhileMainRamRemainsUnchanged(string card, int totalKib, int boardKib)
    {
        var configuration = Configuration(card);
        Assert.Equal(totalKib * 1024L, MemoryExtensionFunctions.TotalBytes(configuration));
        Assert.Equal("32", configuration.Options[SettingsConstants.MainMemory]);
        var field = Assert.Single(MemoryExtensionFunctions.Describe(configuration).Fields, item => item.Id == C.CardOption);
        Assert.Equal(boardKib * 1024L, Assert.Single(field.Choices!, choice => choice.Id == card).NumericValue);
    }

    private static MachineConfiguration Configuration(string card) => new(MachineModel.BbcMicroModelA, core: Emulator.AcornEmu, options: new Dictionary<string,string>
    {
        [SettingsConstants.MainMemory] = "32", [C.CardOption] = card
    });
    private static ModelAMachine Machine(string card) => new(new byte[16384], new byte[16384], memoryBytes: 32768, memoryExtension: new(card));
}
