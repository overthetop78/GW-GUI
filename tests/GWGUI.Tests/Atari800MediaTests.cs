using System.IO;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;
using GWGUI.App.Controls;

namespace GWGUI.Tests;

public sealed class Atari800MediaTests
{
    private static readonly IReadOnlySet<string> SupportedExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "atr", "cas", "car", "bin", "rom", "a52", "m3u"
        };

    [Theory]
    [InlineData("disk.atr", AtariMediaCategory.Floppy, "Floppy")]
    [InlineData("tape.cas", AtariMediaCategory.Cassette, "Cassette")]
    [InlineData("game.bin", AtariMediaCategory.Cartridge, "ComputerCartridge")]
    public void ComputerContent_IsClassifiedByKindAndModel(
        string fileName, AtariMediaCategory kind, string expected)
    {
        var media = new AtariMediaConfiguration(fileName, kind, Slot(kind));

        Assert.Equal(expected, Atari800MediaFunctions.Classify(AtariMachineModel.Atari800Xl, media).ToString());
    }

    [Theory]
    [InlineData("game.a52")]
    [InlineData("game.bin")]
    [InlineData("game.rom")]
    public void Atari5200Cartridge_IsSelectedFromModelAndMetadata(string fileName)
    {
        var media = new AtariMediaConfiguration(fileName, AtariMediaCategory.Cartridge,
            EmulationMediaSlot.Cartridge0, CartridgePlatform: AtariCartridgePlatform.Atari5200);

        Assert.Equal(Atari800ContentType.ConsoleCartridge,
            Atari800MediaFunctions.Classify(AtariMachineModel.Atari5200, media));
    }

    [Fact]
    public void AmbiguousRawCartridge_RejectsPlatformThatConflictsWithMachine()
    {
        var media = new AtariMediaConfiguration("game.rom", AtariMediaCategory.Cartridge,
            EmulationMediaSlot.Cartridge0, CartridgePlatform: AtariCartridgePlatform.Atari5200);

        Assert.Throws<ArgumentException>(() =>
            Atari800MediaFunctions.Classify(AtariMachineModel.Atari130Xe, media));
    }

    [Fact]
    public void CartHeader_IsDetectedFromFileMetadata()
    {
        var root = CreateRoot();
        var path = Path.Combine(root, "game.car");
        File.WriteAllBytes(path, [.. "CART"u8, 0, 0, 0, 1]);
        try
        {
            Assert.True(Atari800MediaFunctions.HasCartridgeHeader(path));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Theory]
    [InlineData(AtariMediaCategory.Floppy, "disk.cas")]
    [InlineData(AtariMediaCategory.Cassette, "tape.atr")]
    [InlineData(AtariMediaCategory.Cartridge, "game.atr")]
    public void KindAndExtensionMismatch_IsRejected(AtariMediaCategory kind, string fileName)
    {
        var root = CreateRoot();
        var path = Path.Combine(root, fileName);
        File.WriteAllBytes(path, [0]);
        var media = new AtariMediaConfiguration(path, kind, Slot(kind));
        try
        {
            Assert.Throws<AtariEmulationException>(() => Atari800MediaFunctions.Prepare(
                new AtariMachineConfiguration(AtariMachineModel.Atari800Xl), media,
                Path.Combine(root, "session"), SupportedExtensions));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Theory]
    [InlineData(AtariMediaCategory.Floppy, "disk.atr")]
    [InlineData(AtariMediaCategory.Cassette, "tape.cas")]
    public void WritableDiskAndCassette_UseExplicitSessionSave(AtariMediaCategory kind, string fileName)
    {
        var root = CreateRoot();
        var path = Path.Combine(root, fileName);
        File.WriteAllBytes(path, [1]);
        var media = new AtariMediaConfiguration(path, kind, Slot(kind));
        try
        {
            var prepared = Atari800MediaFunctions.Prepare(
                new AtariMachineConfiguration(AtariMachineModel.Atari800Xl), media,
                Path.Combine(root, "session"), SupportedExtensions);
            Assert.NotNull(prepared.SessionMedia);
            File.WriteAllBytes(prepared.RuntimePath, [2]);
            Assert.Equal(new byte[] { 1 }, File.ReadAllBytes(path));
            AtariSessionMediaFunctions.Save(prepared.SessionMedia!);
            Assert.Equal(new byte[] { 2 }, File.ReadAllBytes(path));
            using var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void CassetteBootAndMachineOptions_AreExplicit()
    {
        var configuration = new AtariMachineConfiguration(AtariMachineModel.Atari5200);
        var media = new AtariMediaConfiguration("game.a52", AtariMediaCategory.Cartridge,
            EmulationMediaSlot.Cartridge0, CartridgePlatform: AtariCartridgePlatform.Atari5200);
        var prepared = new Atari800PreparedMedia(media, Atari800ContentType.ConsoleCartridge, media.Path, null);

        var options = Atari800MediaFunctions.ApplyOptions(configuration, prepared);

        Assert.Equal(Atari800MediaConstants.Atari5200SystemValue,
            options[Atari800MediaConstants.SystemOptionKey]);
        Assert.Equal(AtariEightBitSettingsConstants.Disabled,
            options[AtariEightBitSettingsConstants.CassetteBootOptionKey]);
    }

    [Fact]
    public void Atari400SettingsAreTranslatedToNativeOptions()
    {
        var configuration = new AtariMachineConfiguration(AtariMachineModel.Atari400, options:
            new Dictionary<string, string>
            {
                [AtariConfigurationOptionConstants.VideoStandard] = AtariClassicRegion.Pal.ToString(),
                [AtariConfigurationOptionConstants.VideoResolution] = "384x288",
                [AtariEightBitSettingsConstants.AxlonMemoryOptionKey] = "256 KB",
                [AtariEightBitSettingsConstants.PokeyStereoOptionKey] = AtariEightBitSettingsConstants.Enabled
            });

        var options = Atari800MediaFunctions.ApplyOptions(configuration, null);

        Assert.Equal(AtariEightBitSettingsConstants.Pal,
            options[AtariEightBitSettingsConstants.VideoStandardOptionKey]);
        Assert.Equal("384x288", options[AtariEightBitSettingsConstants.ResolutionOptionKey]);
        Assert.Equal(AtariEightBitSettingsConstants.Disabled,
            options[AtariEightBitSettingsConstants.MosaicMemoryOptionKey]);
        Assert.Equal("256 KB", options[AtariEightBitSettingsConstants.AxlonMemoryOptionKey]);
        Assert.Equal(AtariEightBitSettingsConstants.Enabled,
            options[AtariEightBitSettingsConstants.PokeyStereoOptionKey]);
        Assert.Equal(AtariEightBitSettingsConstants.NeutralAnalogDeadZone,
            options[AtariEightBitSettingsConstants.AnalogDeadZoneOptionKey]);
        Assert.DoesNotContain(AtariConfigurationOptionConstants.VideoStandard, options);
        Assert.DoesNotContain(AtariConfigurationOptionConstants.VideoResolution, options);
    }

    [Fact]
    public void Atari400MutuallyExclusiveMemoryExtensionsAreNormalized()
    {
        var configuration = new AtariMachineConfiguration(AtariMachineModel.Atari400, options:
            new Dictionary<string, string>
            {
                [AtariEightBitSettingsConstants.MosaicMemoryOptionKey] = "80 KB",
                [AtariEightBitSettingsConstants.AxlonMemoryOptionKey] = "256 KB",
                [AtariEightBitSettingsConstants.AxlonShadowOptionKey] = AtariEightBitSettingsConstants.Enabled
            });

        var options = Atari800MediaFunctions.ApplyOptions(configuration, null);

        Assert.Equal("80 KB", options[AtariEightBitSettingsConstants.MosaicMemoryOptionKey]);
        Assert.Equal(AtariEightBitSettingsConstants.Disabled,
            options[AtariEightBitSettingsConstants.AxlonMemoryOptionKey]);
        Assert.Equal(AtariEightBitSettingsConstants.Disabled,
            options[AtariEightBitSettingsConstants.AxlonShadowOptionKey]);
    }

    private static EmulationMediaSlot Slot(AtariMediaCategory kind) => kind switch
    {
        AtariMediaCategory.Floppy => EmulationMediaSlot.Floppy0,
        AtariMediaCategory.Cassette => EmulationMediaSlot.Cassette0,
        AtariMediaCategory.Cartridge => EmulationMediaSlot.Cartridge0,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    };

    private static string CreateRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "GWGUI-Atari800-Media", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }
}
