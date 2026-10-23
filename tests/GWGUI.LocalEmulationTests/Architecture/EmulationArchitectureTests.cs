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
    public void ModuleMediaDependenciesStayWithinTheSharedLibraries()
    {
        foreach (var assembly in new[] { typeof(CommodoreEmulationModule).Assembly, typeof(AtariEmulationModule).Assembly })
            Assert.Equal(new[] { "gwgui.emulation", "gwgui.mediaengine" }, References(assembly).Where(name => name.StartsWith("gwgui.", StringComparison.Ordinal)).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void AtariHostUsesTheSharedCoreContractWithoutNativeApiTypes()
    {
        var assembly = typeof(AtariEmulationModule).Assembly;
        var contract = assembly.GetType("GWGUI.Emulation.Atari.Common.Interfaces.IEmulatorCore");
        Assert.NotNull(contract);
        var host = assembly.GetType("GWGUI.Emulation.Atari.Common.Services.CoreHost");
        var factory = host!.GetMethod("Run", BindingFlags.Static | BindingFlags.NonPublic)!.GetParameters().Last().ParameterType;
        Assert.Equal(contract, factory.GetGenericArguments().Last());
        Assert.All(contract!.GetMethods(), method => Assert.DoesNotContain("ExternalCoreApi", method.ToString()!, StringComparison.Ordinal));
    }

    [Fact]
    public void AppAndEmulationDoNotReferenceFamilyModules()
    {
        var forbidden = FamilyModules().Select(row => "gwgui.emulation." + ((string)row[0]).ToLowerInvariant()).ToHashSet(StringComparer.Ordinal);
        Assert.DoesNotContain(References(typeof(MainWindow).Assembly), forbidden.Contains);
        Assert.DoesNotContain(References(typeof(IEmulationModule).Assembly), forbidden.Contains);
    }

    [Fact]
    public void FamilyModulesExposeSharedAdapterOperations()
    {
        var atari = CommonAdapter(typeof(AtariEmulationModule).Assembly, "GWGUI.Emulation.Atari");
        var amiga = CommonAdapter(typeof(CommodoreEmulationModule).Assembly, "GWGUI.Emulation.Commodore");
        Assert.All(MemberNames(atari.Adapter).Where(member => !member.Contains('.')).Except(new[] { "GetCartridgeExtensions", "GetRuntimeOptions" }), member => Assert.Contains(member, MemberNames(amiga.Adapter)));
        Assert.Equal(new[]
        {
            "ApplyDeviceSettings", "ApplySettings", "ApplyStorage", "ApplyStorageConfiguration", "Create", "Definition", "DescribeDeviceSettings", "DescribeInput", "DescribeMediaDevices", "DescribeStorage", "EmulatorId", "EmulatorKey", "EnsurePersistentStorage", "FindInstalledCorePathAsync",
            "FindReleasesAsync", "GetControllerBindingDefinitions", "GetControllerOutputs", "GetControllerPortCount", "GetControllerPortDisplayName", "GetControllerPortDisplayNumber", "GetDefaultOptions", "GetInstallationAsync", "GetMediaDependencies", "GetMemoryCardCount", "GetMemoryCardDirectory", "GetMemoryCardDisplayLabel", "GetMemoryCardIndexes", "GetOutputResolution", "GetPeripherals", "GetUsbPeripherals", "HasFixedMemoryCardDirectories", "InitializeOptions", "InstallAsync", "IsCpuClock", "IsDefaultFor", "IsDeferredOption", "MapInput", "MemoryCardInsertedOptionKey", "NormalizeOptions", "RequiresMediaRestart", "RequiresOptionRestart",
            "ResolveConfiguredMedia", "ResolveContentExtensions", "ResolveInitialContentPaths", "ResolveModel", "ScanFirmwareFileAsync", "SelectConfiguration", "SupportedContentExtensions", "SupportsInitialContent", "SupportsNoContent", "SupportsPointer", "SupportsPointerCapture", "SupportsVideoSettings", "TryHandleHostCommand", "ValidateConfiguration", "ValidateOptions", "get_Definition", "get_EmulatorId",
            "get_EmulatorKey", "get_HasFixedMemoryCardDirectories", "get_MemoryCardInsertedOptionKey", "get_SupportedContentExtensions", "get_SupportsInitialContent", "get_SupportsNoContent", "get_SupportsVideoSettings"
        }, MemberNames(atari.Adapter).Where(member => !member.Contains('.')).Except(new[] { "GetCartridgeExtensions", "GetRuntimeOptions", "DescribeSettings" }));
        Assert.Equal(MemberNames(atari.Context), MemberNames(amiga.Context));
        foreach (var row in FamilyModules())
        {
            var family = (string)row[0];
            var assembly = Assembly.Load($"gwgui.emulation.{family.ToLowerInvariant()}");
            var module = CommonAdapter(assembly, $"GWGUI.Emulation.{family}");
            Assert.Equal(typeof(GWGUI.Emulation.Contracts.EmulatorManagementContext), module.Management);
        }
        foreach (var adapter in new[] { atari.Adapter, amiga.Adapter })
            Assert.DoesNotContain(MemberNames(adapter), member => member is "CatalogEntry" or "Emulator" or "NormalizeConfiguration" or "PrepareConfiguration" or "get_CatalogEntry" or "get_Emulator");
        var atariMediaAdapter = typeof(AtariEmulationModule).Assembly.GetType("GWGUI.Emulation.Atari.Emulators.Common.Interop.Interfaces.IEmulatorMediaAdapter");
        Assert.NotNull(atariMediaAdapter);
        Assert.DoesNotContain("PrepareOptions", MemberNames(atariMediaAdapter!));

        var root = RepositoryRoot();
        var atariSource = NormalizedFamilySource(Path.Combine(root, "src", "GWGUI.Emulation.Atari", "Common", "Interfaces", "IEmulatorAdapter.cs"), "Atari");
        var amigaSource = NormalizedFamilySource(Path.Combine(root, "src", "GWGUI.Emulation.Commodore", "Common", "Interfaces", "IEmulatorAdapter.cs"), "Commodore");
        Assert.Contains("interface IEmulatorAdapter", atariSource, StringComparison.Ordinal);
        Assert.Contains("interface IEmulatorAdapter", amigaSource, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(FamilyModules))]
    public void FamilyNamespacesFollowFoldersAndCommonDoesNotReferenceEmulators(string family)
    {
        var root = RepositoryRoot();
        VerifyFamily(root, family);
    }

    [Fact]
    public void AtariMachineFamiliesUseTheSharedHardwareContractAndCommonAdapter()
    {
        var assembly = typeof(AtariEmulationModule).Assembly;
        var hardwareType = assembly.GetType("GWGUI.Emulation.Atari.Common.Machines.Common.Contracts.HardwareModelDefinition");
        var adapterType = assembly.GetType("GWGUI.Emulation.Atari.Common.Interfaces.IEmulatorAdapter");
        var coreCatalog = assembly.GetType("GWGUI.Emulation.Atari.Emulators.Common.Interop.Dictionaries.CoreCatalog");
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
            var catalogName = family == "Atari8Bit" ? "Atari8BitModelCatalog" : $"{family}ModelCatalog";
            var catalog = assembly.GetType($"GWGUI.Emulation.Atari.Common.Machines.{family}.Dictionaries.{catalogName}");
            Assert.NotNull(catalog);
            var familyDefinitions = ((System.Collections.IEnumerable)catalog!.GetProperty("All", BindingFlags.Static | BindingFlags.Public)!.GetValue(null)!).Cast<object>().ToArray();
            Assert.All(familyDefinitions, definition => Assert.Equal(hardwareType, definition.GetType()));
            Assert.Equal(models, familyDefinitions.Select(definition => definition.GetType().GetProperty("Model")!.GetValue(definition)!.ToString()).ToArray());
            definitions.AddRange(familyDefinitions);
        }

        var createAdapter = coreCatalog!.GetMethod("CreateAdapter", BindingFlags.Static | BindingFlags.NonPublic);
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
    public void FamilyCommonConstantsDoNotOwnCoreReleaseMetadata()
    {
        var root = RepositoryRoot();
        foreach (var family in new[] { "Atari", "Commodore", "Amstrad", "Nec", "Nintendo", "Sega", "Sony", "Microsoft" })
        {
            var common = Path.Combine(root, "src", $"GWGUI.Emulation.{family}", "Common");
            Assert.DoesNotContain(Directory.EnumerateFiles(common, "*.cs", SearchOption.AllDirectories), file => Path.GetFileName(file) is "CoreReleaseConstants.cs" or "CoreDefinitionConstants.cs");
        }
    }

    [Fact]
    public void FamilyCommonContractFilesAndContentsAreIdentical()
    {
        var atariContext = CommonAdapter(typeof(AtariEmulationModule).Assembly, "GWGUI.Emulation.Atari").Context;
        var amigaContext = CommonAdapter(typeof(CommodoreEmulationModule).Assembly, "GWGUI.Emulation.Commodore").Context;
        Assert.Equal(typeof(GWGUI.Emulation.Contracts.EmulatorCreationContext<>), atariContext.GetGenericTypeDefinition());
        Assert.Equal(atariContext.GetGenericTypeDefinition(), amigaContext.GetGenericTypeDefinition());
        var root = RepositoryRoot();
        var atari = ContractFileNames(root, "Atari");
        var amiga = ContractFileNames(root, "Commodore");
        Assert.Equal(atari, amiga);

        foreach (var fileName in atari)
        {
            var atariSource = NormalizedFamilySource(Path.Combine(root, "src", "GWGUI.Emulation.Atari", "Common", "Contracts", fileName), "Atari");
            var amigaSource = NormalizedFamilySource(Path.Combine(root, "src", "GWGUI.Emulation.Commodore", "Common", "Contracts", fileName), "Commodore");
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
                     "A5200", "Atari800", "BeetleLynx", "GearLynx", "Handy", "Holani", "Hatari", "Hatari2014", "HatariB", "ProSystem", "Stella", "Stella2014", "Stella2023", "VirtualJaguar"
                 })
            Assert.True(File.Exists(Path.Combine(atari, "Emulators", emulator, "Constants", "EmulatorConstants.cs")), $"{emulator} does not own its metadata.");

        var commonSource = string.Join('\n', Directory.EnumerateFiles(Path.Combine(atari, "Common"), "*.cs", SearchOption.AllDirectories).Select(File.ReadAllText));
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

        var atariGenericSource = string.Join('\n', new[] { "Common", "Modules" }.SelectMany(area => Directory.EnumerateFiles(Path.Combine(atari, area), "*.cs", SearchOption.AllDirectories)).Select(File.ReadAllText));
        Assert.DoesNotMatch(new Regex("\\\"(?:hatari|atari800|atarixegs|stella|virtualjaguar)_[^\\\"]*\\\"", RegexOptions.CultureInvariant), atariGenericSource);
        foreach (var nativeOption in new[]
                 {
                     "color_hue", "color_saturation", "color_contrast", "color_brightness",
                     "color_gamma", "color_delay", "external_palette", "pot_digital_sensitivity",
                     "pot_analog_sensitivity", "pot_analog_deadzone", "paddle_active",
                     "paddle_movement_speed"
                 })
            Assert.DoesNotContain($"\"{nativeOption}\"", atariGenericSource, StringComparison.Ordinal);

        var amiga = Path.Combine(root, "src", "GWGUI.Emulation.Commodore");
        var amigaGenericSource = string.Join('\n', new[] { "Common", "Modules" }.SelectMany(area => Directory.EnumerateFiles(Path.Combine(amiga, area), "*.cs", SearchOption.AllDirectories)).Select(File.ReadAllText));
        Assert.DoesNotContain("puae_", amigaGenericSource, StringComparison.Ordinal);
        var genericOptionDeclarations = Regex.Matches(amigaGenericSource, "const string Option\\w+ = \\\"(gwgui_amiga_[^\\\"]+)\\\"", RegexOptions.CultureInvariant).Select(match => match.Groups[1].Value).ToArray();
        Assert.Equal(genericOptionDeclarations.Distinct(StringComparer.Ordinal), genericOptionDeclarations);

        var prefixedDeclaration = new Regex(@"\b(?:class|record|enum|interface|struct)\s+(?:Amiga|Atari(?!800))", RegexOptions.CultureInvariant);
        foreach (var family in new[] { "Atari", "Commodore" })
        foreach (var file in Directory.EnumerateFiles(Path.Combine(root, "src", $"GWGUI.Emulation.{family}", "Emulators"), "*.cs", SearchOption.AllDirectories))
            if (!file.Contains(Path.Combine("UAE", "Common"), StringComparison.Ordinal))
                Assert.DoesNotMatch(prefixedDeclaration, File.ReadAllText(file));
    }

    private static void AssertFamilyFolders(string root, string module, params string[] families)
    {
        var machines = Path.Combine(root, "src", $"GWGUI.Emulation.{module}", "Common", "Machines");
        Assert.Equal(families.Order(StringComparer.Ordinal), Directory.EnumerateDirectories(machines).Select(Path.GetFileName).Order(StringComparer.Ordinal));
    }

    private static void AssertMachineCommonFolders(string root, string module, params string[] folders)
    {
        var common = Path.Combine(root, "src", $"GWGUI.Emulation.{module}", "Common", "Machines", "Common");
        Assert.Equal(folders.Order(StringComparer.Ordinal), Directory.EnumerateDirectories(common).Select(Path.GetFileName).Order(StringComparer.Ordinal));
    }

    [Theory]
    [MemberData(nameof(FamilyModules))]
    public void FamilyEmulatorCatalogsDeclareAdaptersWithoutNamespaceReflection(string family)
    {
        var root = RepositoryRoot();
        var source = File.ReadAllText(Path.Combine(root, "src", $"GWGUI.Emulation.{family}", "Common", "Dictionaries", "EmulatorCatalog.cs"));
        Assert.DoesNotContain("GetTypes()", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Namespace?.Contains", source, StringComparison.Ordinal);
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
    public void SharedCatalogAndReleaseErrorsAreOwnedByCommon()
    {
        var root = RepositoryRoot();
        foreach (var family in FamilyModules().Select(row => (string)row[0]).Append("Example"))
        {
            var assembly = Assembly.Load($"gwgui.emulation.{family.ToLowerInvariant()}");
            var commonNamespace = $"GWGUI.Emulation.{family}.Common";
            var functions = Path.Combine(root, "src", $"GWGUI.Emulation.{family}", "Common", "Functions");
            var source = string.Join('\n', Directory.EnumerateFiles(functions, "*.cs").Select(File.ReadAllText));
            foreach (var errorName in new[] { "CoreReleaseErrors", "EmulatorCatalogErrors" })
            {
                if (!Regex.IsMatch(source, @"\b" + errorName + @"\.")) continue;
                var owner = assembly.GetType(commonNamespace + ".Exceptions." + errorName);
                Assert.NotNull(owner);
                Assert.False(owner!.IsVisible);
                Assert.DoesNotContain(assembly.GetTypes(), type => type.Name == errorName && type.Namespace!.Contains(".Emulators.", StringComparison.Ordinal));
            }
        }
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
    public void FamilyCommonFunctionFoldersExposeSharedInterfaceGateways()
    {
        var root = RepositoryRoot();
        var services = new Dictionary<string, Type>
        {
            ["Protocol"] = typeof(IEmulationHostProtocol), ["NativeResources"] = typeof(IEmulationNativeResources),
            ["Adapters"] = typeof(IEmulationAdapters), ["MachineExecution"] = typeof(IEmulationMachineExecution),
            ["Messages"] = typeof(IEmulationMessages), ["VideoProcessing"] = typeof(IEmulationVideoProcessing),
            ["HardwareSettings"] = typeof(IEmulationHardwareSettings), ["OptionProcessing"] = typeof(IEmulationCoreOptionProcessing),
            ["EnvironmentProcessing"] = typeof(IEmulationCoreEnvironmentProcessing),
            ["FirmwareProcessing"] = typeof(IEmulationFirmwareProcessing), ["MediaRules"] = typeof(IEmulationMediaRules)
        };
        foreach (var family in FamilyModules().Select(row => (string)row[0]).Append("Example"))
        {
            var files = CommonFileNames(root, family, "Functions");
            foreach (var expected in new[] { "AudioFunctions.cs", "ControllerFunctions.cs", "CoreFunctions.Catalog.cs", "CoreFunctions.Host.cs", "CoreFunctions.Release.cs" })
                Assert.Contains(expected, files);
            var assembly = Assembly.Load($"gwgui.emulation.{family.ToLowerInvariant()}");
            var owner = assembly.GetType($"GWGUI.Emulation.{family}.Common.Functions.CoreHostFunctions", throwOnError: true)!;
            Assert.False(owner.IsVisible);
            foreach (var service in services)
            {
                var entry = owner.GetProperty(service.Key, BindingFlags.Static | BindingFlags.NonPublic);
                Assert.NotNull(entry);
                Assert.Equal(service.Value, entry!.PropertyType);
                Assert.True(entry.PropertyType.IsInterface && entry.PropertyType.IsPublic);
                Assert.True(entry.GetMethod!.IsAssembly);
            }
        }
    }

    [Theory]
    [MemberData(nameof(FamilyModules))]
    public void FamilyAdapterContractsDoNotRequireNativeTransportTypes(string family)
    {
        var root = RepositoryRoot();
            var source = File.ReadAllText(Path.Combine(root, "src", $"GWGUI.Emulation.{family}", "Common", "Interfaces", "IEmulatorAdapter.cs"));
            Assert.DoesNotContain("Libretro", source, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("ExternalCoreApi", source, StringComparison.Ordinal);
            Assert.DoesNotContain("DllImport", source, StringComparison.Ordinal);
            Assert.DoesNotContain("IntPtr", source, StringComparison.Ordinal);
    }

    public static IEnumerable<object[]> FamilyModules() => Directory.EnumerateDirectories(Path.Combine(RepositoryRoot(), "src"), "GWGUI.Emulation.*")
        .Where(project => File.Exists(Path.Combine(project, "module.json")))
        .Order(StringComparer.Ordinal)
        .Select(project => new object[] { Path.GetFileName(project)["GWGUI.Emulation.".Length..] });

    private static string[] ConstantFileNames(string root, string family) =>
        Directory.EnumerateFiles(Path.Combine(root, "src", $"GWGUI.Emulation.{family}", "Common", "Constants"), "*.cs", SearchOption.TopDirectoryOnly).Select(Path.GetFileName).OfType<string>()
            .Where(name => name != "ExternalCoreInteropConstants.cs")
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static string[] ContractFileNames(string root, string family) => CommonFileNames(root, family, "Contracts");

    private static string[] CommonFileNames(string root, string family, string folder) =>
        Directory.Exists(Path.Combine(root, "src", $"GWGUI.Emulation.{family}", "Common", folder))
            ? Directory.EnumerateFiles(Path.Combine(root, "src", $"GWGUI.Emulation.{family}", "Common", folder), "*.cs", SearchOption.TopDirectoryOnly).Select(Path.GetFileName).OfType<string>().Order(StringComparer.Ordinal).ToArray()
            : [];

    private static IReadOnlyDictionary<string, Type> ConstantTypes(Assembly assembly, string constantsNamespace) => assembly.GetTypes().Where(type => type.Namespace == constantsNamespace).ToDictionary(type => type.Name, StringComparer.Ordinal);

    private static string[] ConstantMemberNames(Type type) => type.GetMembers(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
        .Where(member => member.MemberType is MemberTypes.Field or MemberTypes.Property)
        .Select(member => member.Name)
        .Order(StringComparer.Ordinal)
        .ToArray();

    internal static (Type Adapter, Type Context, Type Management) CommonAdapter(Assembly assembly, string rootNamespace)
    {
        var adapter = assembly.GetType($"{rootNamespace}.Common.Interfaces.IEmulatorAdapter");
        Assert.NotNull(adapter);
        var sharedAdapter = Assert.Single(adapter!.GetInterfaces(), type => type.IsGenericType
            && type.GetGenericTypeDefinition() == typeof(GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<,>));
        Assert.True(sharedAdapter.GetGenericTypeDefinition().IsPublic);
        var create = sharedAdapter.GetMethod("Create");
        Assert.NotNull(create);
        var parameters = create!.GetParameters();
        Assert.Equal(2, parameters.Length);
        var context = parameters[1].ParameterType;
        Assert.True(context.IsGenericType);
        Assert.Equal(typeof(GWGUI.Emulation.Contracts.EmulatorCreationContext<>), context.GetGenericTypeDefinition());
        Assert.True(context.GetGenericTypeDefinition().IsPublic);
        Assert.Equal(parameters[0].ParameterType, Assert.Single(context.GetGenericArguments()));
        Assert.Null(assembly.GetType($"{rootNamespace}.Common.Contracts.EmulatorCreationContext"));
        if (rootNamespace == "GWGUI.Emulation.Nec")
        {
            var firmware = context.GetConstructors().Single(constructor => constructor.GetParameters().Length == 6)
                .GetParameters().Last();
            Assert.Equal("FirmwareDirectory", firmware.Name);
            Assert.False(firmware.HasDefaultValue);
        }
        Assert.Null(assembly.GetType($"{rootNamespace}.Common.Contracts.EmulatorManagementContext"));
        var installation = sharedAdapter.GetMethod("GetInstallationAsync");
        Assert.NotNull(installation);
        var management = installation!.GetParameters()[0].ParameterType;
        Assert.Equal(typeof(GWGUI.Emulation.Contracts.EmulatorManagementContext), management);
        return (adapter!, context!, management!);
    }

    private static void VerifyFamily(string repositoryRoot, string family)
    {
        var project = Path.Combine(repositoryRoot, "src", $"GWGUI.Emulation.{family}");
        Assert.True(Directory.Exists(Path.Combine(project, "Common")), family + " must expose Common.");
        foreach (var area in new[] { "Common", "Emulators" }.Where(area => Directory.Exists(Path.Combine(project, area))))
        foreach (var file in Directory.EnumerateFiles(Path.Combine(project, area), "*.cs", SearchOption.AllDirectories))
        {
            var relativeDirectory = Path.GetRelativePath(project, Path.GetDirectoryName(file)!).Replace(Path.DirectorySeparatorChar, '.');
            var expected = $"GWGUI.Emulation.{family}.{relativeDirectory}";
            var source = File.ReadAllText(file);
            Assert.Contains($"namespace {expected};", source, StringComparison.Ordinal);
            if (area == "Common")
            {
                Assert.DoesNotContain($"GWGUI.Emulation.{family}.Emulators", source, StringComparison.Ordinal);
                Assert.False(Regex.IsMatch(source, @"\bEmulators\s*\."), file + " references an emulator through a relative namespace.");
            }
        }

        foreach (var file in Directory.EnumerateFiles(Path.Combine(project, "Modules"), "*.cs"))
            Assert.DoesNotContain($"GWGUI.Emulation.{family}.Emulators", File.ReadAllText(file), StringComparison.Ordinal);
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "src")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }

    private static string[] MemberNames(Type type) => (type.IsInterface ? type.GetInterfaces().Append(type) : [type])
        .SelectMany(contract => contract.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
        .Where(member => member.MemberType != MemberTypes.NestedType || !member.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), inherit: false))
        .Select(member => member.Name).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();

    private static string NormalizedFamilySource(string path, string family)
    {
        var source = File.ReadAllText(path).Replace($"GWGUI.Emulation.{family}", "GWGUI.Emulation.Family", StringComparison.Ordinal).Replace("\r\n", "\n", StringComparison.Ordinal);
        if (Path.GetFileName(path) == "IEmulatorAdapter.cs")
            source = string.Join('\n', source.Split('\n').Where(line => !line.Contains(" GetCartridgeExtensions(", StringComparison.Ordinal) && !line.Contains(" GetRuntimeOptions(", StringComparison.Ordinal)));
        if (Path.GetFileName(path) == "EmulatorCatalog.cs")
            source = source.Replace("\n                && type.Namespace?.Contains(\".Emulators.\", StringComparison.Ordinal) == true", string.Empty, StringComparison.Ordinal);
        return source;
    }

    private static IReadOnlySet<string> References(Assembly assembly) => assembly.GetReferencedAssemblies().Select(reference => reference.Name?.ToLowerInvariant() ?? string.Empty).ToHashSet(StringComparer.Ordinal);
}
