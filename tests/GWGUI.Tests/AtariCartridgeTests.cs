using System.IO;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;
using GWGUI.Emulation.Atari.Cores;

namespace GWGUI.Tests;

public sealed class AtariCartridgeTests
{
    private const int StellaTestCartridgeSize = 4096;
    private const int TestCartridgeType = 1;
    public static TheoryData<AtariMachineModel, AtariCoreKind, string, bool> CoreCases => new()
    {
        { AtariMachineModel.Atari2600, AtariCoreKind.Stella, "game.a26", false },
        { AtariMachineModel.Atari7800, AtariCoreKind.ProSystem, "game.a78", false },
        { AtariMachineModel.Lynx, AtariCoreKind.BeetleLynx, "game.lnx", false },
        { AtariMachineModel.Jaguar, AtariCoreKind.VirtualJaguar, "game.j64", false }
    };

    public static TheoryData<string, AtariCoreKind> OfficialCoreCases => new()
    {
        { "stella.dll", AtariCoreKind.Stella },
        { "prosystem.dll", AtariCoreKind.ProSystem },
        { "beetle-lynx.dll", AtariCoreKind.BeetleLynx },
        { "virtual-jaguar.dll", AtariCoreKind.VirtualJaguar }
    };

    [Theory]
    [MemberData(nameof(OfficialCoreCases))]
    [Trait("Category", "LocalAssets")]
    public void RulesMatchExtensionsReportedByOfficialCore(string fileName, AtariCoreKind core)
    {
        var info = AtariExternalCoreProbe.Inspect(
            Path.Combine(FindRepositoryRoot(), "tmp", "atari-cores", fileName), core);

        Assert.True(AtariCartridgeConstants.Extensions[core].IsSubsetOf(info.Extensions));
        Assert.False(info.NeedsFullPath);
    }

    [Theory]
    [MemberData(nameof(CoreCases))]
    public void EachCoreAcceptsOnlyItsConfiguredCartridge(
        AtariMachineModel model, AtariCoreKind core, string fileName, bool needsFullPath)
    {
        var root = CreateRoot();
        var path = Path.Combine(root, fileName);
        File.WriteAllBytes(path, [1, 2, 3]);
        try
        {
            var media = Cartridge(path);
            var prepared = AtariCartridgeFunctions.Prepare(
                new AtariMachineConfiguration(model), media, core, needsFullPath,
                AtariCartridgeConstants.Extensions[core]);

            Assert.Equal(core, prepared.Core);
            Assert.Equal(Path.GetFullPath(path), prepared.RuntimePath);
            Assert.Equal(needsFullPath, prepared.NeedsFullPath);
            Assert.Same(media, prepared.Configuration);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Theory]
    [MemberData(nameof(CoreCases))]
    public void WrongExtensionIsRejectedForEveryCore(
        AtariMachineModel model, AtariCoreKind core, string _, bool needsFullPath)
    {
        var root = CreateRoot();
        var path = Path.Combine(root, "wrong.txt");
        File.WriteAllBytes(path, [1]);
        try
        {
            Assert.Throws<AtariEmulationException>(() => AtariCartridgeFunctions.Prepare(
                new AtariMachineConfiguration(model), Cartridge(path), core, needsFullPath,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "txt" }));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void CartridgeCannotBePreparedForAnotherMachineCore()
    {
        var root = CreateRoot();
        var path = Path.Combine(root, "game.a26");
        File.WriteAllBytes(path, [1]);
        try
        {
            Assert.Throws<ArgumentException>(() => AtariCartridgeFunctions.Prepare(
                new AtariMachineConfiguration(AtariMachineModel.Atari2600), Cartridge(path),
                AtariCoreKind.ProSystem, false, AtariCartridgeConstants.Extensions[AtariCoreKind.ProSystem]));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void MapperMetadataIsRejectedWhenCoreHasNoDedicatedInterface()
    {
        var media = Cartridge("game.a26") with { CartridgeType = TestCartridgeType };

        Assert.Throws<ArgumentException>(() => AtariCartridgeFunctions.ValidateNoUnsupportedMetadata(media));
    }

    [Theory]
    [InlineData(AtariCartridgeRegion.Automatic, "auto")]
    [InlineData(AtariCartridgeRegion.Ntsc, "ntsc")]
    [InlineData(AtariCartridgeRegion.Pal, "pal")]
    [InlineData(AtariCartridgeRegion.Secam, "secam")]
    public void StellaReceivesSupportedRegionMetadata(AtariCartridgeRegion region, string expected)
    {
        var media = Cartridge("game.a26") with { CartridgeRegion = region };

        var options = AtariCartridgeFunctions.ApplyOptions(
            new Dictionary<string, string>(), media, AtariCoreKind.Stella);

        Assert.Equal(expected, options[AtariCartridgeConstants.StellaRegionOptionKey]);
    }

    [Theory]
    [InlineData(AtariCartridgeRegion.Automatic, "disabled")]
    [InlineData(AtariCartridgeRegion.Ntsc, "disabled")]
    [InlineData(AtariCartridgeRegion.Pal, "enabled")]
    public void JaguarReceivesOnlySupportedRegionMetadata(AtariCartridgeRegion region, string expected)
    {
        var media = Cartridge("game.j64") with { CartridgeRegion = region };

        var options = AtariCartridgeFunctions.ApplyOptions(
            new Dictionary<string, string>(), media, AtariCoreKind.VirtualJaguar);

        Assert.Equal(expected, options[AtariCartridgeConstants.JaguarRegionOptionKey]);
    }

    [Theory]
    [InlineData(AtariCoreKind.ProSystem)]
    [InlineData(AtariCoreKind.BeetleLynx)]
    public void RegionMetadataIsRejectedWhenCoreDoesNotExposeIt(AtariCoreKind core)
    {
        var media = Cartridge("game.bin") with { CartridgeRegion = AtariCartridgeRegion.Pal };

        Assert.Throws<ArgumentException>(() => AtariCartridgeFunctions.ApplyOptions(
            new Dictionary<string, string>(), media, core));
    }

    [Fact]
    [Trait("Category", "LocalAssets")]
    public void PoweredMachineCanReplaceCartridgeAndRejectsUnsupportedEjection()
    {
        var root = CreateRoot();
        var first = Path.Combine(root, "first.a26");
        var second = Path.Combine(root, "second.a26");
        File.WriteAllBytes(first, new byte[StellaTestCartridgeSize]);
        var replacementBytes = new byte[StellaTestCartridgeSize];
        replacementBytes[^TestCartridgeType] = TestCartridgeType;
        File.WriteAllBytes(second, replacementBytes);
        try
        {
            using var core = new AtariExternalCore(
                Path.Combine(FindRepositoryRoot(), "tmp", "atari-cores", "stella.dll"), AtariCoreKind.Stella);
            core.Initialize(new AtariMachineConfiguration(AtariMachineModel.Atari2600,
                media: [Cartridge(first)]), Path.Combine(root, "session"));

            core.InsertMedia(Cartridge(second));

            Assert.Equal(Path.GetFullPath(second), Assert.Single(core.MountedMedia).Path);
            Assert.Throws<NotSupportedException>(() => core.EjectMedia(EmulationMediaSlot.Cartridge0));
            core.Stop();
            using var firstLock = new FileStream(first, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            using var secondLock = new FileStream(second, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    [Trait("Category", "LocalAssets")]
    public void InvalidReplacementLeavesPreviousCartridgeRunning()
    {
        var root = CreateRoot();
        var first = Path.Combine(root, "first.a26");
        var invalid = Path.Combine(root, "invalid.txt");
        File.WriteAllBytes(first, new byte[StellaTestCartridgeSize]);
        File.WriteAllBytes(invalid, [TestCartridgeType]);
        try
        {
            using var core = new AtariExternalCore(
                Path.Combine(FindRepositoryRoot(), "tmp", "atari-cores", "stella.dll"), AtariCoreKind.Stella);
            core.Initialize(new AtariMachineConfiguration(AtariMachineModel.Atari2600,
                media: [Cartridge(first)]), Path.Combine(root, "session"));

            Assert.Throws<AtariEmulationException>(() => core.InsertMedia(Cartridge(invalid)));

            Assert.Equal(Path.GetFullPath(first), Assert.Single(core.MountedMedia).Path);
            core.RunFrame();
            core.Stop();
            using var sourceLock = new FileStream(first, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void ExclusivelyLockedCartridgeIsRejectedAndReleasedAfterUnlock()
    {
        var root = CreateRoot();
        var path = Path.Combine(root, "game.a26");
        File.WriteAllBytes(path, [1]);
        try
        {
            using (var locked = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                Assert.Throws<AtariEmulationException>(() => AtariCartridgeFunctions.Prepare(
                    new AtariMachineConfiguration(AtariMachineModel.Atari2600), Cartridge(path),
                    AtariCoreKind.Stella, false, AtariCartridgeConstants.Extensions[AtariCoreKind.Stella]));
            }

            var prepared = AtariCartridgeFunctions.Prepare(
                new AtariMachineConfiguration(AtariMachineModel.Atari2600), Cartridge(path),
                AtariCoreKind.Stella, false, AtariCartridgeConstants.Extensions[AtariCoreKind.Stella]);
            Assert.Equal(Path.GetFullPath(path), prepared.RuntimePath);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static AtariMediaConfiguration Cartridge(string path) =>
        new(path, AtariMediaKind.Cartridge, EmulationMediaSlot.Cartridge0, IsReadOnly: true);

    private static string CreateRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "GWGUI-Atari-Cartridge", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "GWGUI.sln")))
            current = current.Parent;
        return current?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }
}
