using System.IO;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.Tests;

public sealed class Atari800MediaTests
{
    private static readonly IReadOnlySet<string> SupportedExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "atr", "cas", "car", "bin", "rom", "a52", "m3u"
        };

    [Theory]
    [InlineData("disk.atr", AtariMediaKind.Floppy, "Floppy")]
    [InlineData("tape.cas", AtariMediaKind.Cassette, "Cassette")]
    [InlineData("game.bin", AtariMediaKind.Cartridge, "ComputerCartridge")]
    public void ComputerContent_IsClassifiedByKindAndModel(
        string fileName, AtariMediaKind kind, string expected)
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
        var media = new AtariMediaConfiguration(fileName, AtariMediaKind.Cartridge,
            EmulationMediaSlot.Cartridge0, CartridgePlatform: AtariCartridgePlatform.Atari5200);

        Assert.Equal(Atari800ContentType.ConsoleCartridge,
            Atari800MediaFunctions.Classify(AtariMachineModel.Atari5200, media));
    }

    [Fact]
    public void AmbiguousRawCartridge_RejectsPlatformThatConflictsWithMachine()
    {
        var media = new AtariMediaConfiguration("game.rom", AtariMediaKind.Cartridge,
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
    [InlineData(AtariMediaKind.Floppy, "disk.cas")]
    [InlineData(AtariMediaKind.Cassette, "tape.atr")]
    [InlineData(AtariMediaKind.Cartridge, "game.atr")]
    public void KindAndExtensionMismatch_IsRejected(AtariMediaKind kind, string fileName)
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
    [InlineData(AtariMediaKind.Floppy, "disk.atr")]
    [InlineData(AtariMediaKind.Cassette, "tape.cas")]
    public void WritableDiskAndCassette_UseExplicitSessionSave(AtariMediaKind kind, string fileName)
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
        var media = new AtariMediaConfiguration("game.a52", AtariMediaKind.Cartridge,
            EmulationMediaSlot.Cartridge0, CartridgePlatform: AtariCartridgePlatform.Atari5200);
        var prepared = new Atari800PreparedMedia(media, Atari800ContentType.ConsoleCartridge, media.Path, null);

        var options = Atari800MediaFunctions.ApplyOptions(configuration, prepared);

        Assert.Equal(Atari800MediaConstants.Atari5200SystemValue,
            options[Atari800MediaConstants.SystemOptionKey]);
        Assert.Equal(Atari800MediaConstants.DisabledOptionValue,
            options[Atari800MediaConstants.CassetteBootOptionKey]);
    }

    private static EmulationMediaSlot Slot(AtariMediaKind kind) => kind switch
    {
        AtariMediaKind.Floppy => EmulationMediaSlot.Floppy0,
        AtariMediaKind.Cassette => EmulationMediaSlot.Cassette0,
        AtariMediaKind.Cartridge => EmulationMediaSlot.Cartridge0,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    };

    private static string CreateRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "GWGUI-Atari800-Media", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }
}
