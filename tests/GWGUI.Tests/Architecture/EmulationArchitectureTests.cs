using System.Reflection;
using System.IO;
using GWGUI.App.Views.Windows.Shell;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Amiga.Modules;
using GWGUI.Emulation.Atari.Modules;

namespace GWGUI.Tests.Architecture;

public sealed class EmulationArchitectureTests
{
    [Fact]
    public void AppAndEmulationDoNotReferenceFamilyModules()
    {
        var forbidden = new[] { "gwgui.emulation.amiga", "gwgui.emulation.atari", "gwgui.emulation.amstrad" };
        Assert.DoesNotContain(References(typeof(MainWindow).Assembly), forbidden.Contains);
        Assert.DoesNotContain(References(typeof(IEmulationModule).Assembly), forbidden.Contains);
    }

    [Fact]
    public void FamilyModulesExposeTheSameInternalAdapterNames()
    {
        var atari = CommonAdapter(typeof(AtariEmulationModule).Assembly, "GWGUI.Emulation.Atari");
        var amiga = CommonAdapter(typeof(AmigaEmulationModule).Assembly, "GWGUI.Emulation.Amiga");
        Assert.Equal(MemberNames(atari.Adapter), MemberNames(amiga.Adapter));
        Assert.Equal(MemberNames(atari.Context), MemberNames(amiga.Context));
        Assert.Equal(MemberNames(atari.Management), MemberNames(amiga.Management));
    }

    [Fact]
    public void FamilyNamespacesFollowFoldersAndCommonDoesNotReferenceEmulators()
    {
        var root = RepositoryRoot();
        VerifyFamily(root, "Atari");
        VerifyFamily(root, "Amiga");
    }

    [Fact]
    public void FamilyCommonNamesAreGenericAndAudioConstantsHaveTheSamePath()
    {
        var root = RepositoryRoot();
        var constantFiles = new[]
        {
            "AudioConstants.cs", "ConfigurationConstants.cs", "ControllerConstants.cs",
            "CoreConstants.cs", "CoreManagementConstants.cs", "EmulationModuleConstants.cs",
            "FirmwareConstants.cs", "InputConstants.cs", "MachineConstants.cs",
            "MediaConstants.cs", "ModelConstants.cs", "RuntimeConstants.cs",
            "SettingsConstants.cs", "SettingsTextConstants.cs", "SettingsChoiceConstants.cs",
            "StateConstants.cs", "StorageConstants.cs", "VideoConstants.cs"
        };
        foreach (var family in new[] { "Atari", "Amiga", "Amstrad" })
        {
            var common = Path.Combine(root, "src", $"GWGUI.Emulation.{family}", "Common");
            foreach (var file in constantFiles)
                Assert.True(File.Exists(Path.Combine(common, "Constants", file)),
                    $"{family} Common/Constants/{file} is missing.");

            Assert.DoesNotContain(Directory.EnumerateDirectories(
                    Path.Combine(root, "src", $"GWGUI.Emulation.{family}"), "*", SearchOption.AllDirectories),
                directory => !Directory.EnumerateFileSystemEntries(directory).Any());

            if (family == "Amstrad") continue;
            Assert.Empty(Directory.EnumerateFiles(common, $"{family}*.cs", SearchOption.AllDirectories));
            Assert.Empty(Directory.EnumerateFiles(common, $"I{family}*.cs", SearchOption.AllDirectories));
            Assert.DoesNotContain(Directory.EnumerateFiles(common, "*.cs", SearchOption.AllDirectories),
                file => File.ReadLines(file).Count() > 200);
        }

        AssertFamilyFolders(root, "Atari", "Atari8Bit", "AtariClassic", "AtariST");
        AssertFamilyFolders(root, "Amiga", "AmigaComputers", "AmigaCDTV", "AmigaCD32");
    }

    private static void AssertFamilyFolders(string root, string module, params string[] families)
    {
        var machines = Path.Combine(root, "src", $"GWGUI.Emulation.{module}", "Common", "Machines");
        Assert.Equal(families.Order(StringComparer.Ordinal),
            Directory.EnumerateDirectories(machines).Select(Path.GetFileName).Order(StringComparer.Ordinal));
    }

    private static (Type Adapter, Type Context, Type Management) CommonAdapter(
        Assembly assembly, string rootNamespace)
    {
        var adapter = assembly.GetType($"{rootNamespace}.Common.Interfaces.IEmulatorAdapter");
        var context = assembly.GetType($"{rootNamespace}.Common.Contracts.EmulatorCreationContext");
        var management = assembly.GetType($"{rootNamespace}.Common.Contracts.EmulatorManagementContext");
        Assert.NotNull(adapter);
        Assert.NotNull(context);
        Assert.NotNull(management);
        return (adapter!, context!, management!);
    }

    private static void VerifyFamily(string repositoryRoot, string family)
    {
        var project = Path.Combine(repositoryRoot, "src", $"GWGUI.Emulation.{family}");
        foreach (var area in new[] { "Common", "Emulators" })
        foreach (var file in Directory.EnumerateFiles(Path.Combine(project, area), "*.cs",
                     SearchOption.AllDirectories))
        {
            var relativeDirectory = Path.GetRelativePath(project, Path.GetDirectoryName(file)!)
                .Replace(Path.DirectorySeparatorChar, '.');
            var expected = $"GWGUI.Emulation.{family}.{relativeDirectory}";
            var source = File.ReadAllText(file);
            Assert.Contains($"namespace {expected};", source, StringComparison.Ordinal);
            if (area == "Common")
                Assert.DoesNotContain($"GWGUI.Emulation.{family}.Emulators", source,
                    StringComparison.Ordinal);
        }

        foreach (var file in Directory.EnumerateFiles(Path.Combine(project, "Modules"), "*.cs"))
            Assert.DoesNotContain($"GWGUI.Emulation.{family}.Emulators", File.ReadAllText(file),
                StringComparison.Ordinal);
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "src")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }

    private static string[] MemberNames(Type type) => type.GetMembers(BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
        .Select(member => member.Name).Order(StringComparer.Ordinal).ToArray();

    private static IReadOnlySet<string> References(Assembly assembly) => assembly.GetReferencedAssemblies()
        .Select(reference => reference.Name?.ToLowerInvariant() ?? string.Empty)
        .ToHashSet(StringComparer.Ordinal);
}
