using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using GWGUI.App.Views.Windows.Shell;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Commodore.Modules;
using GWGUI.Emulation.Atari.Modules;

namespace GWGUI.Tests.Architecture;

public sealed class EmulationArchitectureTests
{
    [Fact]
    public void AtariHostUsesTheSharedCoreContractWithoutNativeApiTypes()
    {
        var assembly = typeof(AtariEmulationModule).Assembly;
        var contract = assembly.GetType("GWGUI.Emulation.Atari.Emulators.Common.Interfaces.IEmulatorCore");
        Assert.NotNull(contract);
        var host = assembly.GetType("GWGUI.Emulation.Atari.Common.Services.CoreHost");
        var factory = host!.GetMethod("Run", BindingFlags.Static | BindingFlags.NonPublic)!
            .GetParameters().Last().ParameterType;
        Assert.Equal(contract, factory.GetGenericArguments().Last());
        Assert.All(contract!.GetMethods(), method => Assert.DoesNotContain("ExternalCoreApi",
            method.ToString()!, StringComparison.Ordinal));
    }

    [Fact]
    public void AppAndEmulationDoNotReferenceFamilyModules()
    {
        var forbidden = new[] { "gwgui.emulation.commodore", "gwgui.emulation.atari", "gwgui.emulation.amstrad" };
        Assert.DoesNotContain(References(typeof(MainWindow).Assembly), forbidden.Contains);
        Assert.DoesNotContain(References(typeof(IEmulationModule).Assembly), forbidden.Contains);
    }

    [Fact]
    public void FamilyModulesExposeTheSameInternalAdapterNames()
    {
        var atari = CommonAdapter(typeof(AtariEmulationModule).Assembly, "GWGUI.Emulation.Atari");
        var amiga = CommonAdapter(typeof(CommodoreEmulationModule).Assembly, "GWGUI.Emulation.Commodore");
        Assert.Equal(MemberNames(atari.Adapter).Except(new[] { "GetCartridgeExtensions", "GetRuntimeOptions" }),
            MemberNames(amiga.Adapter));
        Assert.Equal(new[]
        {
            "Create", "Definition", "EmulatorId", "EmulatorKey", "FindInstalledCorePathAsync",
            "FindReleasesAsync", "GetInstallationAsync", "InstallAsync",
            "ResolveConfiguredMedia", "TryHandleHostCommand", "get_Definition", "get_EmulatorId",
            "get_EmulatorKey"
        }, MemberNames(atari.Adapter).Except(new[] { "GetCartridgeExtensions", "GetRuntimeOptions" }));
        Assert.Equal(MemberNames(atari.Context), MemberNames(amiga.Context));
        Assert.Equal(MemberNames(atari.Management), MemberNames(amiga.Management));
        foreach (var adapter in new[] { atari.Adapter, amiga.Adapter })
            Assert.DoesNotContain(MemberNames(adapter), member => member is "CatalogEntry" or "Emulator"
                or "NormalizeConfiguration" or "PrepareConfiguration" or "get_CatalogEntry"
                or "get_Emulator");
        var atariMediaAdapter = typeof(AtariEmulationModule).Assembly.GetType(
            "GWGUI.Emulation.Atari.Emulators.Common.Interop.Interfaces.IEmulatorMediaAdapter");
        Assert.NotNull(atariMediaAdapter);
        Assert.DoesNotContain("PrepareOptions", MemberNames(atariMediaAdapter!));

        var root = RepositoryRoot();
        var atariSource = NormalizedFamilySource(Path.Combine(root, "src",
            "GWGUI.Emulation.Atari", "Common", "Interfaces", "IEmulatorAdapter.cs"), "Atari");
        var amigaSource = NormalizedFamilySource(Path.Combine(root, "src",
            "GWGUI.Emulation.Commodore", "Common", "Interfaces", "IEmulatorAdapter.cs"), "Commodore");
        Assert.Equal(atariSource, amigaSource);
    }

    [Fact]
    public void FamilyNamespacesFollowFoldersAndCommonDoesNotReferenceEmulators()
    {
        var root = RepositoryRoot();
        VerifyFamily(root, "Atari");
        VerifyFamily(root, "Commodore");
    }

    [Fact]
    public void AtariMachineFamiliesUseTheSharedHardwareContractAndCommonAdapter()
    {
        var assembly = typeof(AtariEmulationModule).Assembly;
        var hardwareType = assembly.GetType(
            "GWGUI.Emulation.Atari.Common.Machines.Common.Contracts.HardwareModelDefinition");
        var adapterType = assembly.GetType(
            "GWGUI.Emulation.Atari.Common.Interfaces.IEmulatorAdapter");
        var coreCatalog = assembly.GetType(
            "GWGUI.Emulation.Atari.Emulators.Common.Interop.Dictionaries.CoreCatalog");
        Assert.NotNull(hardwareType);
        Assert.NotNull(adapterType);
        Assert.NotNull(coreCatalog);

        var expectedModels = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["Atari8Bit"] = ["Atari400", "Atari800", "Atari800Xl", "Atari130Xe", "XlXe", "Xegs"],
            ["Atari2600"] = ["Atari2600"],
            ["Atari5200"] = ["Atari5200"],
            ["Atari7800"] = ["Atari7800"],
            ["AtariLynx"] = ["Lynx"],
            ["AtariJaguar"] = ["Jaguar", "JaguarCd"]
        };
        var definitions = new List<object>();
        foreach (var (family, models) in expectedModels)
        {
            var catalogName = family == "Atari8Bit" ? "Atari8BitModelCatalog"
                : $"{family}ModelCatalog";
            var catalog = assembly.GetType(
                $"GWGUI.Emulation.Atari.Common.Machines.{family}.Dictionaries.{catalogName}");
            Assert.NotNull(catalog);
            var familyDefinitions = ((System.Collections.IEnumerable)catalog!
                    .GetProperty("All", BindingFlags.Static | BindingFlags.Public)!.GetValue(null)!)
                .Cast<object>().ToArray();
            Assert.All(familyDefinitions, definition => Assert.Equal(hardwareType, definition.GetType()));
            Assert.Equal(models, familyDefinitions.Select(definition => definition.GetType()
                .GetProperty("Model")!.GetValue(definition)!.ToString()).ToArray());
            definitions.AddRange(familyDefinitions);
        }

        var createAdapter = coreCatalog!.GetMethod("CreateAdapter",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(createAdapter);
        foreach (var definition in definitions)
        {
            var emulator = hardwareType!.GetProperty("Emulator")!.GetValue(definition);
            var adapter = createAdapter!.Invoke(null, [emulator]);
            Assert.NotNull(adapter);
            Assert.True(adapterType!.IsInstanceOfType(adapter));
        }
    }

    [Fact]
    public void FamilyCommonConstantFilesAndMembersAreIdentical()
    {
        var root = RepositoryRoot();
        var atariFiles = ConstantFileNames(root, "Atari");
        var amigaFiles = ConstantFileNames(root, "Commodore");
        Assert.Equal(atariFiles, amigaFiles);

        var atariTypes = ConstantTypes(typeof(AtariEmulationModule).Assembly,
            "GWGUI.Emulation.Atari.Common.Constants");
        var amigaTypes = ConstantTypes(typeof(CommodoreEmulationModule).Assembly,
            "GWGUI.Emulation.Commodore.Common.Constants");
        amigaTypes = amigaTypes.Where(pair => pair.Key != "ExternalCoreInteropConstants")
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        Assert.Equal(atariTypes.Keys, amigaTypes.Keys);
        foreach (var typeName in atariTypes.Keys)
            Assert.Equal(ConstantMemberNames(atariTypes[typeName]),
                ConstantMemberNames(amigaTypes[typeName]));

        foreach (var family in new[] { "Atari", "Commodore", "Amstrad" })
        {
            Assert.DoesNotContain(Directory.EnumerateDirectories(
                    Path.Combine(root, "src", $"GWGUI.Emulation.{family}"), "*", SearchOption.AllDirectories),
                directory => !Directory.EnumerateFileSystemEntries(directory).Any());

            if (family == "Amstrad") continue;
            var common = Path.Combine(root, "src", $"GWGUI.Emulation.{family}", "Common");
            Assert.Empty(Directory.EnumerateFiles(common, $"{family}*.cs", SearchOption.AllDirectories));
            Assert.Empty(Directory.EnumerateFiles(common, $"I{family}*.cs", SearchOption.AllDirectories));
        }

        AssertFamilyFolders(root, "Atari", "Atari8Bit", "Atari2600", "Atari5200", "Atari7800",
            "AtariJaguar", "AtariLynx", "AtariST", "Common");
        AssertMachineCommonFolders(root, "Atari", "Constants", "Contracts", "Dictionaries",
            "Enums", "Functions");
        AssertFamilyFolders(root, "Commodore", "AmigaComputers", "CommodoreCDTV", "AmigaCD32", "Common");
        AssertMachineCommonFolders(root, "Commodore", "Constants", "Contracts", "Dictionaries",
            "Enums", "Exceptions", "Functions");
    }

    [Fact]
    public void FamilyCommonContractFilesAndContentsAreIdentical()
    {
        var root = RepositoryRoot();
        var atari = ContractFileNames(root, "Atari");
        var amiga = ContractFileNames(root, "Commodore");
        Assert.Equal(atari, amiga);

        foreach (var fileName in atari)
        {
            var atariSource = NormalizedFamilySource(Path.Combine(root, "src",
                "GWGUI.Emulation.Atari", "Common", "Contracts", fileName), "Atari");
            var amigaSource = NormalizedFamilySource(Path.Combine(root, "src",
                "GWGUI.Emulation.Commodore", "Common", "Contracts", fileName), "Commodore");
            Assert.Equal(atariSource, amigaSource);
        }
    }

    [Fact]
    public void ConcreteEmulatorMetadataAndNamesStayWithAdapters()
    {
        var root = RepositoryRoot();
        var atari = Path.Combine(root, "src", "GWGUI.Emulation.Atari");
        foreach (var emulator in new[]
                 {
                     "A5200", "Atari800", "BeetleLynx", "Hatari", "HatariB", "ProSystem", "Stella", "Stella2014", "Stella2023", "VirtualJaguar"
                 })
            Assert.True(File.Exists(Path.Combine(atari, "Emulators", emulator,
                "Constants", "EmulatorConstants.cs")), $"{emulator} does not own its metadata.");

        var commonSource = string.Join('\n', Directory.EnumerateFiles(
                Path.Combine(atari, "Common"), "*.cs", SearchOption.AllDirectories)
            .Select(File.ReadAllText));
        foreach (var concreteValue in new[]
                 {
                     "hatari_libretro.dll", "atari800_libretro.dll", "stella2023_libretro.dll",
                     "prosystem_libretro.dll", "mednafen_lynx_libretro.dll",
                     "virtualjaguar_libretro.dll", "github.com/libretro/hatari",
                     "github.com/libretro/libretro-atari800", "github.com/libretro/stella",
                     "github.com/libretro/prosystem-libretro",
                     "github.com/libretro/beetle-lynx-libretro",
                     "github.com/libretro/virtualjaguar-libretro"
                 })
            Assert.DoesNotContain(concreteValue, commonSource, StringComparison.OrdinalIgnoreCase);

        var atariGenericSource = string.Join('\n', new[] { "Common", "Modules" }
            .SelectMany(area => Directory.EnumerateFiles(Path.Combine(atari, area), "*.cs",
                SearchOption.AllDirectories)).Select(File.ReadAllText));
        Assert.DoesNotMatch(new Regex(
            "\\\"(?:hatari|atari800|atarixegs|stella|virtualjaguar)_[^\\\"]*\\\"",
            RegexOptions.CultureInvariant), atariGenericSource);
        foreach (var nativeOption in new[]
                 {
                     "color_hue", "color_saturation", "color_contrast", "color_brightness",
                     "color_gamma", "color_delay", "external_palette", "pot_digital_sensitivity",
                     "pot_analog_sensitivity", "pot_analog_deadzone", "paddle_active",
                     "paddle_movement_speed"
                 })
            Assert.DoesNotContain($"\"{nativeOption}\"", atariGenericSource,
                StringComparison.Ordinal);

        var amiga = Path.Combine(root, "src", "GWGUI.Emulation.Commodore");
        var amigaGenericSource = string.Join('\n', new[] { "Common", "Modules" }
            .SelectMany(area => Directory.EnumerateFiles(Path.Combine(amiga, area), "*.cs",
                SearchOption.AllDirectories)).Select(File.ReadAllText));
        Assert.DoesNotContain("puae_", amigaGenericSource, StringComparison.Ordinal);
        var genericOptionDeclarations = Regex.Matches(amigaGenericSource,
            "const string Option\\w+ = \\\"(gwgui_amiga_[^\\\"]+)\\\"",
            RegexOptions.CultureInvariant).Select(match => match.Groups[1].Value).ToArray();
        Assert.Equal(genericOptionDeclarations.Distinct(StringComparer.Ordinal),
            genericOptionDeclarations);

        var prefixedDeclaration = new Regex(
            @"\b(?:class|record|enum|interface|struct)\s+(?:Amiga|Atari(?!800))",
            RegexOptions.CultureInvariant);
        foreach (var family in new[] { "Atari", "Commodore" })
        foreach (var file in Directory.EnumerateFiles(Path.Combine(root, "src",
                     $"GWGUI.Emulation.{family}", "Emulators"), "*.cs", SearchOption.AllDirectories))
            Assert.DoesNotMatch(prefixedDeclaration, File.ReadAllText(file));
    }

    private static void AssertFamilyFolders(string root, string module, params string[] families)
    {
        var machines = Path.Combine(root, "src", $"GWGUI.Emulation.{module}", "Common", "Machines");
        Assert.Equal(families.Order(StringComparer.Ordinal),
            Directory.EnumerateDirectories(machines).Select(Path.GetFileName).Order(StringComparer.Ordinal));
    }

    private static void AssertMachineCommonFolders(string root, string module, params string[] folders)
    {
        var common = Path.Combine(root, "src", $"GWGUI.Emulation.{module}", "Common", "Machines",
            "Common");
        Assert.Equal(folders.Order(StringComparer.Ordinal),
            Directory.EnumerateDirectories(common).Select(Path.GetFileName).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void FamilyCommonDictionaryFilesAndContentsAreIdentical()
    {
        var root = RepositoryRoot();
        var atari = CommonFileNames(root, "Atari", "Dictionaries");
        var amiga = CommonFileNames(root, "Commodore", "Dictionaries");
        Assert.Equal(atari, amiga);

        foreach (var fileName in atari)
        {
            var atariSource = NormalizedFamilySource(Path.Combine(root, "src",
                "GWGUI.Emulation.Atari", "Common", "Dictionaries", fileName), "Atari");
            var amigaSource = NormalizedFamilySource(Path.Combine(root, "src",
                "GWGUI.Emulation.Commodore", "Common", "Dictionaries", fileName), "Commodore");
            Assert.Equal(atariSource, amigaSource);
        }
    }

    [Fact]
    public void FamilyCommonEnumFoldersAreBothEmpty()
    {
        var root = RepositoryRoot();
        var atari = CommonFileNames(root, "Atari", "Enums");
        var amiga = CommonFileNames(root, "Commodore", "Enums");
        Assert.Equal(atari, amiga);
        Assert.Empty(atari);
    }

    [Fact]
    public void FamilyCommonExceptionFoldersAreBothEmpty()
    {
        var root = RepositoryRoot();
        var atari = CommonFileNames(root, "Atari", "Exceptions");
        var amiga = CommonFileNames(root, "Commodore", "Exceptions");
        Assert.Equal(atari, amiga);
        Assert.Empty(atari);
    }

    [Fact]
    public void FamilyCommonFactoryFoldersAreBothEmpty()
    {
        var root = RepositoryRoot();
        var atari = CommonFileNames(root, "Atari", "Factories");
        var amiga = CommonFileNames(root, "Commodore", "Factories");
        Assert.Equal(atari, amiga);
        Assert.Empty(atari);
    }

    [Fact]
    public void FamilyCommonFunctionFoldersAreBothEmpty()
    {
        var root = RepositoryRoot();
        var atari = CommonFileNames(root, "Atari", "Functions");
        var amiga = CommonFileNames(root, "Commodore", "Functions");
        Assert.Equal(atari, amiga);
        Assert.Empty(atari);
    }

    [Fact]
    public void FamilyCommonInterfaceFilesAndContentsAreIdentical()
    {
        var root = RepositoryRoot();
        var atari = CommonFileNames(root, "Atari", "Interfaces");
        var amiga = CommonFileNames(root, "Commodore", "Interfaces");
        Assert.Equal(atari, amiga);
        Assert.Equal(["IEmulatorAdapter.cs"], atari);

        var atariSource = NormalizedFamilySource(Path.Combine(root, "src",
            "GWGUI.Emulation.Atari", "Common", "Interfaces", atari[0]), "Atari");
        var amigaSource = NormalizedFamilySource(Path.Combine(root, "src",
            "GWGUI.Emulation.Commodore", "Common", "Interfaces", amiga[0]), "Commodore");
        Assert.Equal(atariSource, amigaSource);
    }

    private static string[] ConstantFileNames(string root, string family) =>
        Directory.EnumerateFiles(Path.Combine(root, "src", $"GWGUI.Emulation.{family}",
                "Common", "Constants"), "*.cs", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .OfType<string>()
            .Where(name => name != "ExternalCoreInteropConstants.cs")
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static string[] ContractFileNames(string root, string family) =>
        CommonFileNames(root, family, "Contracts");

    private static string[] CommonFileNames(string root, string family, string folder) =>
        Directory.Exists(Path.Combine(root, "src", $"GWGUI.Emulation.{family}", "Common", folder))
            ? Directory.EnumerateFiles(Path.Combine(root, "src", $"GWGUI.Emulation.{family}",
                    "Common", folder), "*.cs", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .OfType<string>()
            .Order(StringComparer.Ordinal)
            .ToArray()
            : [];

    private static IReadOnlyDictionary<string, Type> ConstantTypes(Assembly assembly,
        string constantsNamespace) => assembly.GetTypes()
        .Where(type => type.Namespace == constantsNamespace)
        .ToDictionary(type => type.Name, StringComparer.Ordinal);

    private static string[] ConstantMemberNames(Type type) => type
        .GetMembers(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly)
        .Where(member => member.MemberType is MemberTypes.Field or MemberTypes.Property)
        .Select(member => member.Name)
        .Order(StringComparer.Ordinal)
        .ToArray();

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

    private static string NormalizedFamilySource(string path, string family)
    {
        var source = File.ReadAllText(path).Replace($"GWGUI.Emulation.{family}", "GWGUI.Emulation.Family",
            StringComparison.Ordinal).Replace("\r\n", "\n", StringComparison.Ordinal);
        if (Path.GetFileName(path) == "IEmulatorAdapter.cs")
            source = string.Join('\n', source.Split('\n').Where(line =>
                !line.Contains(" GetCartridgeExtensions(", StringComparison.Ordinal)
                && !line.Contains(" GetRuntimeOptions(", StringComparison.Ordinal)));
        if (Path.GetFileName(path) == "EmulatorCatalog.cs")
            source = source.Replace("\n                && type.Namespace?.Contains(\".Emulators.\", StringComparison.Ordinal) == true",
                string.Empty, StringComparison.Ordinal);
        return source;
    }

    private static IReadOnlySet<string> References(Assembly assembly) => assembly.GetReferencedAssemblies()
        .Select(reference => reference.Name?.ToLowerInvariant() ?? string.Empty)
        .ToHashSet(StringComparer.Ordinal);
}
