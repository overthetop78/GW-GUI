using GWGUI.Emulation.Atari;
using GWGUI.Emulation.Atari.Cores;
using GWGUI.Emulation.Common;
using System.IO;

namespace GWGUI.Tests;

public sealed class AtariCoreCatalogTests
{
    private const int ExpectedCoreCount = 6;
    private const string TestVersion = "test-version";
    private const string ExpectedManifestName = "core.json";

    public static TheoryData<AtariEmulator, string, string, string> CoreFiles => new()
    {
        { AtariEmulator.Hatari, "hatari", "hatari_libretro.dll", "hatari_libretro.dll.zip" },
        { AtariEmulator.Atari800, "atari800", "atari800_libretro.dll", "atari800_libretro.dll.zip" },
        { AtariEmulator.Stella, "stella2023", "stella2023_libretro.dll", "stella2023_libretro.dll.zip" },
        { AtariEmulator.ProSystem, "prosystem", "prosystem_libretro.dll", "prosystem_libretro.dll.zip" },
        { AtariEmulator.BeetleLynx, "beetle-lynx", "mednafen_lynx_libretro.dll", "mednafen_lynx_libretro.dll.zip" },
        { AtariEmulator.VirtualJaguar, "virtual-jaguar", "virtualjaguar_libretro.dll", "virtualjaguar_libretro.dll.zip" }
    };

    [Fact]
    public void CatalogContainsExactlyTheSixDistinctCores()
    {
        Assert.Equal(ExpectedCoreCount, AtariCoreCatalog.All.Count);
        Assert.Equal(Enum.GetValues<AtariEmulator>().Order(), AtariCoreCatalog.All.Select(item => item.Emulator).Order());
        Assert.Equal(ExpectedCoreCount, AtariCoreCatalog.All.Select(item => item.Id)
            .Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Theory]
    [MemberData(nameof(CoreFiles))]
    public void CoreHasExactOfficialNamesAndSeparateVersionPaths(AtariEmulator kind, string id,
        string dllName, string archiveName)
    {
        var entry = AtariCoreCatalog.Get(kind);
        var root = Path.Combine(Path.GetTempPath(), nameof(AtariCoreCatalogTests));
        var paths = AtariCoreCatalog.GetInstallationPaths(kind, root, TestVersion);

        Assert.Equal(id, entry.Id);
        Assert.Equal(dllName, entry.DllName);
        Assert.Equal(archiveName, entry.ArchiveName);
        Assert.Equal(archiveName, Path.GetFileName(entry.ArchiveUri.LocalPath));
        Assert.Equal(Uri.UriSchemeHttps, entry.ArchiveUri.Scheme);
        Assert.Equal(Uri.UriSchemeHttps, entry.SourceUri.Scheme);
        Assert.False(string.IsNullOrWhiteSpace(entry.LibraryName));
        Assert.False(string.IsNullOrWhiteSpace(entry.InspectedRevision));
        Assert.Equal(Path.Combine(Path.GetFullPath(root), id, TestVersion), paths.VersionDirectory);
        Assert.Equal(Path.Combine(paths.VersionDirectory, dllName), paths.LibraryPath);
        Assert.Equal(Path.Combine(paths.VersionDirectory, ExpectedManifestName), paths.ManifestPath);
    }

    [Fact]
    public void EveryMachineModelResolvesToExactlyItsConfiguredCore()
    {
        foreach (var model in Enum.GetValues<AtariMachineModel>())
        {
            var configuration = new AtariMachineConfiguration(model);
            Assert.Equal(configuration.Core, AtariCoreCatalog.Get(model).Emulator);
        }
    }

    [Fact]
    public void AmbiguousMachineAssociationIsRejected()
    {
        var first = AtariCoreCatalog.Get(AtariEmulator.Hatari) with
        {
            Models = new HashSet<AtariMachineModel> { AtariMachineModel.St }
        };
        var second = AtariCoreCatalog.Get(AtariEmulator.Atari800) with
        {
            Models = new HashSet<AtariMachineModel> { AtariMachineModel.St }
        };

        var error = Assert.Throws<InvalidDataException>(() =>
            AtariCoreCatalogFunctions.CreateModelAssociations([first, second]));
        Assert.Equal(AtariCoreCatalogErrors.DuplicateModel, error.Message);
    }

    [Fact]
    public void DiagnosticManifestRetainsEveryRequiredField()
    {
        var downloaded = DateTimeOffset.UtcNow;
        var manifest = new AtariCoreDiagnosticManifest("release-id", TestVersion,
            "https://example.invalid/core.zip", downloaded,
            ArchiveSize: 123, LibrarySize: 456, LibrarySha256: "ABC", Architecture: "x64",
            DeclaredVersion: TestVersion, Exports: ["example"]);

        Assert.Equal(downloaded, manifest.DownloadedUtc);
        Assert.Equal("release-id", manifest.ReleaseId);
        Assert.Equal(TestVersion, manifest.ReleaseVersion);
        Assert.Equal(123, manifest.ArchiveSize);
        Assert.Equal(456, manifest.LibrarySize);
        Assert.Equal("ABC", manifest.LibrarySha256);
        Assert.Equal("x64", manifest.Architecture);
        Assert.Equal(TestVersion, manifest.DeclaredVersion);
        Assert.Equal("example", Assert.Single(manifest.Exports));
        Assert.EndsWith("core.zip", manifest.DownloadUrl, StringComparison.Ordinal);
    }
}
