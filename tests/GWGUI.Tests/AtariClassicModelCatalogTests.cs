using System.Globalization;
using GWGUI.App.Localization;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.Tests;

public sealed class AtariClassicModelCatalogTests
{
    public static TheoryData<AtariMachineModel, string, AtariEmulator, long, AtariClassicCpu> Models => new()
    {
        { AtariMachineModel.Atari400, AtariClassicModelConstants.Atari400And800ModelId, AtariEmulator.Atari800,
            AtariClassicModelConstants.FortyEightKibibytes, AtariClassicCpu.Mos6502B },
        { AtariMachineModel.Atari800, AtariClassicModelConstants.Atari400And800ModelId, AtariEmulator.Atari800,
            AtariClassicModelConstants.FortyEightKibibytes, AtariClassicCpu.Mos6502B },
        { AtariMachineModel.Atari800Xl, AtariClassicModelConstants.Atari800XlModelId, AtariEmulator.Atari800,
            AtariClassicModelConstants.SixtyFourKibibytes, AtariClassicCpu.Mos6502C },
        { AtariMachineModel.Atari130Xe, AtariClassicModelConstants.Atari130XeModelId, AtariEmulator.Atari800,
            AtariClassicModelConstants.OneHundredTwentyEightKibibytes, AtariClassicCpu.Mos6502C },
        { AtariMachineModel.XlXe, AtariClassicModelConstants.XlXeModelId, AtariEmulator.Atari800,
            AtariClassicModelConstants.ThreeHundredTwentyKibibytes, AtariClassicCpu.Mos6502C },
        { AtariMachineModel.Xegs, AtariClassicModelConstants.XegsModelId, AtariEmulator.Atari800,
            AtariClassicModelConstants.SixtyFourKibibytes, AtariClassicCpu.Mos6502C },
        { AtariMachineModel.Atari5200, AtariClassicModelConstants.Atari5200ModelId, AtariEmulator.Atari800,
            AtariClassicModelConstants.SixteenKibibytes, AtariClassicCpu.Mos6502C },
        { AtariMachineModel.Atari2600, AtariClassicModelConstants.Atari2600ModelId, AtariEmulator.Stella,
            AtariClassicModelConstants.OneHundredTwentyEightBytes, AtariClassicCpu.Mos6507 },
        { AtariMachineModel.Atari7800, AtariClassicModelConstants.Atari7800ModelId, AtariEmulator.ProSystem,
            AtariClassicModelConstants.FourKibibytes, AtariClassicCpu.Sally6502C },
        { AtariMachineModel.Lynx, AtariClassicModelConstants.LynxModelId, AtariEmulator.BeetleLynx,
            AtariClassicModelConstants.SixtyFourKibibytes, AtariClassicCpu.Wdc65Sc02 },
        { AtariMachineModel.Jaguar, AtariClassicModelConstants.JaguarModelId, AtariEmulator.VirtualJaguar,
            AtariClassicModelConstants.TwoMibibytes, AtariClassicCpu.Motorola68000 },
        { AtariMachineModel.JaguarCd, AtariClassicModelConstants.JaguarCdModelId, AtariEmulator.VirtualJaguar,
            AtariClassicModelConstants.TwoMibibytes, AtariClassicCpu.Motorola68000 }
    };

    [Theory]
    [MemberData(nameof(Models))]
    public void EveryModelAndVariantHasACompleteDefinition(AtariMachineModel model, string stableModelId,
        AtariEmulator core, long memoryBytes, AtariClassicCpu defaultCpu)
    {
        var definition = AtariClassicModelCatalog.Get(model);

        Assert.Equal(model, definition.Model);
        Assert.Equal(stableModelId, definition.StableModelId);
        Assert.Equal(core, definition.Core);
        Assert.Equal(core, AtariConfigurationFunctions.GetCore(model));
        Assert.Equal(memoryBytes, definition.MainMemoryBytes);
        Assert.Equal(defaultCpu, definition.DefaultCpu);
        Assert.True(definition.DefaultCpuFrequencyHz > 0);
        Assert.NotEmpty(definition.Cpus);
        Assert.NotEmpty(definition.Regions);
        Assert.NotEmpty(definition.Video);
        Assert.NotEmpty(definition.Audio);
        Assert.NotEmpty(definition.Storage);
        Assert.NotEmpty(definition.Ports);
        Assert.NotEmpty(definition.Media);
        Assert.All(definition.Ports, port => Assert.True(port.Count > 0));
    }

    [Theory]
    [MemberData(nameof(Models))]
    public void EveryModelAcceptsOnlyItsDeclaredFirmwareAndMedia(AtariMachineModel model, string ignoredModelId,
        AtariEmulator ignoredCore, long ignoredMemory, AtariClassicCpu ignoredCpu)
    {
        var definition = AtariClassicModelCatalog.Get(model);
        Assert.Equal(ignoredModelId, definition.StableModelId);
        Assert.Equal(ignoredCore, definition.Core);
        Assert.Equal(ignoredMemory, definition.MainMemoryBytes);
        Assert.Equal(ignoredCpu, definition.DefaultCpu);

        foreach (var firmware in definition.Firmware)
            _ = new AtariMachineConfiguration(model,
                firmwares: [new AtariFirmwareConfiguration(firmware, "firmware.rom", false)]);

        foreach (var media in definition.Media)
            _ = new AtariMachineConfiguration(model,
                media: [new AtariMediaConfiguration("content.bin", media, GetSlot(media))]);

        Assert.Throws<ArgumentException>(() => new AtariMachineConfiguration(model,
            firmwares: [new AtariFirmwareConfiguration(AtariFirmwareCategory.Tos, "tos.img", false)]));
        Assert.Throws<ArgumentException>(() => new AtariMachineConfiguration(model,
            media: [new AtariMediaConfiguration("disk.hd", AtariMediaCategory.HardDisk,
                EmulationMediaSlot.HardDisk0)]));
    }

    [Fact]
    public void CatalogContainsEveryNonStModelExactlyOnce()
    {
        var expected = Enum.GetValues<AtariMachineModel>()
            .Where(model => AtariConfigurationFunctions.GetFamily(model) != AtariMachineFamily.St)
            .Order();

        Assert.Equal(expected, AtariClassicModelCatalog.All.Select(definition => definition.Model).Order());
    }

    [Fact]
    public void EveryDisplayNameIsAvailableInEverySupportedLanguage()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            foreach (var language in UiLanguageCatalog.Available)
            {
                var culture = UiLanguageResolver.GetUiCulture(language.Code);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
                foreach (var definition in AtariClassicModelCatalog.All)
                    Assert.DoesNotContain('[', LocExtension.Get(definition.DisplayNameResourceKey));
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    [Fact]
    public void StModelsAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AtariClassicModelCatalog.Get(AtariMachineModel.St));
    }

    private static EmulationMediaSlot GetSlot(AtariMediaCategory media) => media switch
    {
        AtariMediaCategory.Floppy => EmulationMediaSlot.Floppy0,
        AtariMediaCategory.Cassette => EmulationMediaSlot.Cassette0,
        AtariMediaCategory.Cartridge => EmulationMediaSlot.Cartridge0,
        AtariMediaCategory.CompactDisc => EmulationMediaSlot.Cd0,
        _ => throw new ArgumentOutOfRangeException(nameof(media), media, null)
    };
}
