using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Contracts;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Services;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.ModelA.Services;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Enums;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Constants;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.SidewaysExpansionConstants;

namespace GWGUI.Tests.Emulation;

/// <summary>Bus des banques RAM et protections des modules, sans fichier ni firmware externe.</summary>
public sealed class AcornEmuSidewaysExpansionTests
{
    [Theory]
    [InlineData(C.None, 0, 0)]
    [InlineData("boobip-ramrom-1", 32, 1)]
    [InlineData("boobip-ramrom-2", 64, 2)]
    [InlineData("boobip-ramrom-4", 128, 4)]
    [InlineData("boobip-eprom-4", 0, 4)]
    [InlineData(C.AriesWithoutRam, 0, 12)]
    [InlineData(C.Aries8, 8, 12)]
    [InlineData(C.Aries16, 16, 12)]
    public void HardwareChoiceRamCapacityAndRomSocketsBelongToTheirOwnTabs(string card, int ramKib, int sockets)
    {
        var configuration = new MachineConfiguration(MachineModel.BbcMicroModelA, core: Emulator.AcornEmu,
            options: new Dictionary<string, string> { [SettingsConstants.MainMemory] = "32", [C.Option] = card });
        var blocks = new GWGUI.Emulation.Acorn.Emulators.AcornEmu.Factories.AcornEmuMachineFactory().DescribeSettings(configuration);
        var hardware = Assert.Single(blocks.SelectMany(block => block.Fields), field => field.Id == C.Option);
        Assert.Equal(GWGUI.Emulation.Enums.EmulationMachineTab.Extensions, hardware.Tab);
        var ram = Assert.Single(blocks.SelectMany(block => block.Fields), field => field.Id == C.RamCapacity);
        Assert.Equal(GWGUI.Emulation.Enums.EmulationMachineTab.Ram, ram.Tab);
        Assert.Equal(GWGUI.Emulation.Enums.EmulationSettingsEditor.Information, ram.Editor);
        Assert.Equal(ramKib * 1024L, ram.NumericValue);
        Assert.Equal(card != C.None, Assert.Single(blocks, block => block.Id == C.Group).IsVisible);
        var roms = Assert.Single(blocks, block => block.Id == C.RomGroup);
        Assert.Equal(GWGUI.Emulation.Enums.EmulationMachineTab.Rom, roms.Tab);
        Assert.Equal(sockets, roms.Fields.Count);
        Assert.Equal((32 + ramKib) * 1024L, MemoryExtensionFunctions.TotalBytes(configuration));
        Assert.Equal(card, configuration.Options[C.Option]);
    }

    [Fact]
    public void CapacityIsNotPersistedAndDisablingTheCardPreservesRomPaths()
    {
        var configuration = new MachineConfiguration(MachineModel.BbcMicroModelA, core: Emulator.AcornEmu,
            options: new Dictionary<string, string>
            {
                [SettingsConstants.MainMemory] = "32", [C.Option] = "boobip-ramrom-2",
                [C.ChipPrefix + "0"] = "saved-chip.rom"
            });
        var engine = new GWGUI.Emulation.Acorn.Common.Services.Engine();
        var changed = (MachineConfiguration)engine.ApplySettings(configuration,
            new Dictionary<string, string?> { [C.Option] = C.None, [C.RamCapacity] = "64 KiB" });
        Assert.Equal(C.None, changed.Options[C.Option]);
        Assert.Equal("saved-chip.rom", changed.Options[C.ChipPrefix + "0"]);
        Assert.DoesNotContain(C.RamCapacity, changed.Options.Keys);
        Assert.Equal("32", changed.Options[SettingsConstants.MainMemory]);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void EveryInstalledRamBankIsIndependentAndMissingBanksRemainUnmapped(int count)
    {
        var settings = new SidewaysExpansionSettings(count, true);
        var bus = new SidewaysRoms(new byte[C.BankBytes], expansion: settings);
        for (var bank = 0; bank < C.RamBanksPerModule * C.SocketCount; bank++)
        {
            bus.Select((byte)bank);
            bus.Write(0x8000, (byte)(bank + 1));
            bus.Write(0xbfff, (byte)(bank + 17));
        }
        for (var bank = 0; bank < C.RamBanksPerModule * C.SocketCount; bank++)
        {
            bus.Select((byte)bank);
            Assert.Equal(settings.IsRamBank(bank) ? bank + 1 : 255, bus.Read(0x8000));
            Assert.Equal(settings.IsRamBank(bank) ? bank + 17 : 255, bus.Read(0xbfff));
        }
    }

    [Fact]
    public void ProtectionCommandsSelectAndProtectOnlyTheirRamBank()
    {
        var bus = new SidewaysRoms(new byte[C.BankBytes], expansion: new(4, true));
        bus.Select(0); bus.Write(0x8123, 0x41);
        bus.Select(C.ProtectCommand); bus.Write(0x8123, 0x51);
        Assert.Equal(0x41, bus.Read(0x8123));
        bus.Select(1); bus.Write(0x8123, 0x61);
        Assert.Equal(0x61, bus.Read(0x8123));
        bus.Select(C.UnprotectCommand); bus.Write(0x8123, 0x71);
        Assert.Equal(0x71, bus.Read(0x8123));
        bus.Select(1); Assert.Equal(0x61, bus.Read(0x8123));
    }

    [Fact]
    public void RomCannotBeWrittenAndColdResetRestoresRamAndProtection()
    {
        var basic = Enumerable.Repeat((byte)0x81, C.BankBytes).ToArray();
        var bus = new SidewaysRoms(basic, expansion: new(4, true));
        bus.Write(0x8000, 0x91); Assert.Equal(0x81, bus.Read(0x8000));
        bus.Select(0); bus.Write(0x8000, 0x31); bus.Select(C.ProtectCommand);
        bus.Reset(); Assert.Equal(0x81, bus.Read(0x8000));
        bus.Select(0); Assert.Equal(0, bus.Read(0x8000));
        bus.Write(0x8000, 0x51); Assert.Equal(0x51, bus.Read(0x8000));
    }

    [Fact]
    public void ModelABusRoutesRomWindowWritesAndBreakPreservesTheBanks()
    {
        var machine = new ModelAMachine(new byte[C.BankBytes], new byte[C.BankBytes], sidewaysExpansion: new(4, true));
        try
        {
            machine.Write(0xfe30, 3); machine.Write(0x8123, 0xa1);
            machine.Reset(false); Assert.Equal(0xa1, machine.Read(0x8123));
            machine.Write(0xfe30, 0x23); machine.Write(0x8123, 0xb1);
            Assert.Equal(0xa1, machine.Read(0x8123));
            machine.Reset(true); machine.Write(0xfe30, 3); Assert.Equal(0, machine.Read(0x8123));
        }
        finally { machine.Reset(true); }
    }

    [Theory]
    [InlineData("boobip-ramrom-1", 48)]
    [InlineData("boobip-ramrom-4", 144)]
    [InlineData("boobip-eprom-4", 16)]
    public void PhysicalCapacityIsAddedWithoutChangingTheMainRam(string card, int totalKib)
    {
        var configuration = new MachineConfiguration(MachineModel.BbcMicroModelA, core: Emulator.AcornEmu, options: new Dictionary<string,string> { [C.Option] = card });
        Assert.Equal(totalKib * 1024L, MemoryExtensionFunctions.TotalBytes(configuration));
        Assert.Equal("16", configuration.Options.GetValueOrDefault(SettingsConstants.MainMemory, "16"));
        var field = Assert.Single(SidewaysExpansionFunctions.Describe(configuration).Fields);
        var selected = Assert.Single(field.Choices!, choice => choice.Id == card);
        Assert.Equal(SidewaysExpansionFunctions.Read(configuration).RamBytes, selected.NumericValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(C.AriesHalfRamBytes)]
    [InlineData(C.BankBytes)]
    public void AriesB12WritesBankZeroIndependentlyAndDoesNotInventTheMissingHalf(int bytes)
    {
        var settings = new SidewaysExpansionSettings(AriesB12: true, AriesRamBytes: bytes);
        var basic = Enumerable.Repeat((byte)0x81, C.BankBytes).ToArray();
        var bus = new SidewaysRoms(basic, expansion: settings);
        Assert.Equal(0x81, bus.Read(0x8000));
        bus.Write(0x8000, 0x41);
        bus.Write(0xa000, 0x51);
        Assert.Equal(0x81, bus.Read(0x8000));
        bus.Select(C.ProtectCommand);
        Assert.Equal(bytes > 0 ? 0x41 : 255, bus.Read(0x8000));
        Assert.Equal(bytes == C.BankBytes ? 0x51 : 255, bus.Read(0xa000));
        bus.Write(0x8000, 0x61);
        Assert.Equal(bytes > 0 ? 0x61 : 255, bus.Read(0x8000));
        bus.Reset();
        Assert.Equal(0x81, bus.Read(0x8000));
        bus.Select(0); Assert.Equal(bytes > 0 ? 0 : 255, bus.Read(0x8000));
    }

    [Theory]
    [InlineData(C.AriesWithoutRam, 32)]
    [InlineData(C.Aries8, 40)]
    [InlineData(C.Aries16, 48)]
    public void AriesB12AccountsForInstalledMemoryAndReassignsTheNativeRomSockets(string card, int totalKib)
    {
        var configuration = new MachineConfiguration(MachineModel.BbcMicroModelA, core: Emulator.AcornEmu, options: new Dictionary<string,string>
        {
            [SettingsConstants.MainMemory] = "32", [C.Option] = card
        });
        Assert.Equal(totalKib * 1024L, MemoryExtensionFunctions.TotalBytes(configuration));
        Assert.Equal(12, SidewaysExpansionFunctions.Read(configuration).RomSocketCount);
        Assert.Equal(12, SidewaysExpansionFunctions.NativeBank(configuration, 15));
        Assert.Equal(11, SidewaysExpansionFunctions.NativeBank(configuration, 14));
        Assert.Equal(10, SidewaysExpansionFunctions.NativeBank(configuration, 13));
        Assert.Equal(9, SidewaysExpansionFunctions.NativeBank(configuration, 12));
        Assert.Equal(12, SidewaysExpansionFunctions.DescribeRoms(configuration).Fields.Count);
    }

    [Fact]
    public void AriesB12RequiresTheDocumentedMainRamWithoutReplacingRomPaths()
    {
        var plain = new MachineConfiguration(MachineModel.BbcMicroModelA, core: Emulator.AcornEmu);
        Assert.Throws<ArgumentException>(() => SidewaysExpansionFunctions.Read(plain with { Options = new Dictionary<string,string> { [C.Option] = C.Aries16 } }));
        var configuration = new MachineConfiguration(MachineModel.BbcMicroModelA, core: Emulator.AcornEmu, options: new Dictionary<string,string>
        {
            [SettingsConstants.MainMemory] = "32", [C.Option] = C.Aries16,
            [GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.EmulatorConstants.BasicFirmware] = "basic.rom"
        });
        _ = SidewaysExpansionFunctions.Describe(configuration);
        Assert.Equal("basic.rom", configuration.Options[GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.EmulatorConstants.BasicFirmware]);
    }

    [Fact]
    public void OverlappingRomAndRamOrTooManyPhysicalModulesAreRejected()
    {
        Assert.Throws<ArgumentException>(() => new SidewaysRoms(new byte[C.BankBytes], new Dictionary<int,byte[]> { [0] = new byte[C.BankBytes] }, new(1, true)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SidewaysRoms(new byte[C.BankBytes], expansion: new(5, true)));
    }

    [Theory]
    [InlineData("boobip-ramrom-0")]
    [InlineData("boobip-ramrom-5")]
    [InlineData("boobip-ramrom-04")]
    [InlineData("boobip-eprom-256")]
    public void UndocumentedModuleCountsAreRejected(string card)
    {
        var configuration = new MachineConfiguration(MachineModel.BbcMicroModelA, core: Emulator.AcornEmu);
        Assert.Throws<ArgumentException>(() => SidewaysExpansionFunctions.Read(configuration with { Options = new Dictionary<string,string> { [C.Option] = card } }));
    }
}
