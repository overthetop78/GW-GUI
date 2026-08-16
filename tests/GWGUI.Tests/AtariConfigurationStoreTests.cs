using System.IO;
using System.Text.Json;
using GWGUI.App;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.Tests;

public sealed class AtariConfigurationStoreTests
{
    [Fact]
    public async Task RoundTripsTwoConfigurationsForEveryFamily()
    {
        var root = CreateRoot();
        var data = Path.Combine(root, AtariConfigurationStoreTestConstants.DataDirectoryName);
        var directory = Path.Combine(data, AtariConfigurationStoreTestConstants.ConfigurationsDirectoryName);
        var store = new AtariConfigurationStore(directory, data);
        var configurations = AtariConfigurationStoreTestConstants.FamilyModels
            .Select((model, index) => Configuration(model, data, index)).ToArray();
        try
        {
            foreach (var configuration in configurations) await store.SaveAsync(configuration);
            var loaded = await store.LoadAllAsync();

            Assert.Equal(AtariConfigurationStoreTestConstants.ExpectedConfigurationCount, loaded.Count);
            foreach (var family in Enum.GetValues<AtariMachineFamily>())
                Assert.Equal(AtariConfigurationStoreTestConstants.ConfigurationsPerFamily,
                    loaded.Count(configuration => configuration.Family == family));
            foreach (var expected in configurations)
            {
                var actual = Assert.Single(loaded, configuration => configuration.Id == expected.Id);
                Assert.Equal(expected.Model, actual.Model);
                Assert.Equal(expected.Core, actual.Core);
                Assert.Equal(expected.Options, actual.Options);
                Assert.Equal(expected.Input.KeyboardMappings, actual.Input.KeyboardMappings);
                Assert.Equal(expected.Folders.Shared, actual.Folders.Shared);
                Assert.Equal(expected.VideoRenderer, actual.VideoRenderer);
                Assert.Equal(AtariConstants.CurrentConfigurationSchemaVersion, actual.SchemaVersion);
            }
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public async Task PersistsRelativeDataPathsAndAbsoluteExternalPaths()
    {
        var root = CreateRoot();
        var data = Path.Combine(root, AtariConfigurationStoreTestConstants.DataDirectoryName);
        var directory = Path.Combine(data, AtariConfigurationStoreTestConstants.ConfigurationsDirectoryName);
        var firmware = Path.Combine(data, AtariConfigurationStoreTestConstants.FirmwareFileName);
        var internalMedia = Path.Combine(data, AtariConfigurationStoreTestConstants.InternalMediaFileName);
        var externalMedia = Path.Combine(root, AtariConfigurationStoreTestConstants.ExternalDirectoryName,
            AtariConfigurationStoreTestConstants.ExternalMediaFileName);
        var captureFolder = Path.Combine(data, AtariConfigurationStoreTestConstants.CaptureDirectoryName);
        CreateFile(firmware, AtariConfigurationStoreTestConstants.FirmwareBytes);
        CreateFile(internalMedia, AtariConfigurationStoreTestConstants.InternalMediaBytes);
        CreateFile(externalMedia, AtariConfigurationStoreTestConstants.ExternalMediaBytes);
        var configuration = new AtariMachineConfiguration(AtariMachineModel.St,
            [new AtariFirmwareConfiguration(AtariFirmwareKind.Tos, firmware, IsRequired: true)],
            [
                new AtariMediaConfiguration(internalMedia, AtariMediaKind.Floppy, EmulationMediaSlot.Floppy0),
                new AtariMediaConfiguration(externalMedia, AtariMediaKind.Floppy, EmulationMediaSlot.Floppy1)
            ],
            AtariConfigurationStoreTestConstants.Options,
            AtariConfigurationStoreTestConstants.Input,
            folders: new AtariFolderConfiguration(Shared: data, Floppies: externalMedia,
                Captures: captureFolder));
        var store = new AtariConfigurationStore(directory, data);
        try
        {
            await store.SaveAsync(configuration);
            var documentPath = DocumentPath(directory, configuration.Id);
            var json = await File.ReadAllTextAsync(documentPath);
            Assert.Contains(AtariConfigurationStoreTestConstants.StoredFirmwarePath, json, StringComparison.Ordinal);
            Assert.Contains(AtariConfigurationStoreTestConstants.StoredInternalMediaPath, json,
                StringComparison.Ordinal);
            Assert.Contains(JsonEncodedText.Encode(externalMedia).ToString(), json,
                StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(AtariConfigurationStoreTestConstants.SessionDirectoryName, json,
                StringComparison.OrdinalIgnoreCase);

            var loaded = Assert.Single(await store.LoadAllAsync());
            Assert.Equal(Path.GetFullPath(firmware), Assert.Single(loaded.Firmwares).Path);
            Assert.Equal(Path.GetFullPath(internalMedia), loaded.Media[AtariConfigurationStoreTestConstants.FirstIndex].Path);
            Assert.Equal(Path.GetFullPath(externalMedia), loaded.Media[AtariConfigurationStoreTestConstants.SecondIndex].Path);
            Assert.Equal(Path.GetFullPath(captureFolder), loaded.Folders.Captures);
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public async Task CorruptAndFutureDocumentsAreIsolatedFromValidDocuments()
    {
        var root = CreateRoot();
        var store = new AtariConfigurationStore(root, root);
        var valid = Configuration(AtariMachineModel.Atari2600, root,
            AtariConfigurationStoreTestConstants.FirstIndex);
        try
        {
            await store.SaveAsync(valid);
            WriteDocument(root, AtariConfigurationStoreTestConstants.CorruptDirectoryName,
                AtariConfigurationStoreTestConstants.CorruptJson);
            WriteDocument(root, AtariConfigurationStoreTestConstants.FutureDirectoryName,
                AtariConfigurationStoreTestConstants.FutureJson);

            var loaded = Assert.Single(await store.LoadAllAsync());
            Assert.Equal(valid.Id, loaded.Id);
            Assert.False(store.IsLoading);
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public async Task SavingAgainReplacesAtomicallyAndLeavesNoTemporaryFile()
    {
        var root = CreateRoot();
        var store = new AtariConfigurationStore(root, root);
        var first = Configuration(AtariMachineModel.Atari2600, root,
            AtariConfigurationStoreTestConstants.FirstIndex);
        var second = new AtariMachineConfiguration(first.Model, options:
            AtariConfigurationStoreTestConstants.ReplacementOptions, id: first.Id);
        try
        {
            await store.SaveAsync(first);
            await store.SaveAsync(second);
            var documentPath = DocumentPath(root, first.Id);
            Assert.False(File.Exists(documentPath + AtariConfigurationStoreConstants.TemporaryFileSuffix));
            Assert.Equal(AtariConfigurationStoreTestConstants.ReplacementOptionValue,
                Assert.Single(await store.LoadAllAsync()).Options[AtariConfigurationStoreTestConstants.OptionKey]);
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public async Task DeleteRemovesOnlyConfigurationDocumentAndKeepsSharedFiles()
    {
        var root = CreateRoot();
        var data = Path.Combine(root, AtariConfigurationStoreTestConstants.DataDirectoryName);
        var directory = Path.Combine(data, AtariConfigurationStoreTestConstants.ConfigurationsDirectoryName);
        var shared = Path.Combine(data, AtariConfigurationStoreTestConstants.SharedFileName);
        CreateFile(shared, AtariConfigurationStoreTestConstants.SharedBytes);
        var first = Configuration(AtariMachineModel.Atari2600, data,
            AtariConfigurationStoreTestConstants.FirstIndex);
        var second = Configuration(AtariMachineModel.Atari7800, data,
            AtariConfigurationStoreTestConstants.SecondIndex);
        var store = new AtariConfigurationStore(directory, data);
        try
        {
            await store.SaveAsync(first);
            await store.SaveAsync(second);
            store.Delete(first.Id);

            var remaining = Assert.Single(await store.LoadAllAsync());
            Assert.Equal(second.Id, remaining.Id);
            Assert.True(File.Exists(shared));
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public void MigrationDispatcherAcceptsCurrentAndRejectsFutureSchemas()
    {
        var configuration = Configuration(AtariMachineModel.Atari2600, Path.GetTempPath(),
            AtariConfigurationStoreTestConstants.FirstIndex);
        var document = AtariConfigurationStoreFunctions.ToDocument(configuration, Path.GetTempPath());
        using var current = JsonDocument.Parse(JsonSerializer.Serialize(document,
            AtariConfigurationStoreConstants.JsonOptions));
        Assert.Equal(configuration.Id,
            AtariConfigurationMigrationFunctions.MigrateToCurrent(current.RootElement).Id);
        using var future = JsonDocument.Parse(AtariConfigurationStoreTestConstants.FutureJson);
        Assert.Throws<InvalidDataException>(() =>
            AtariConfigurationMigrationFunctions.MigrateToCurrent(future.RootElement));
    }

    [Fact]
    public void StoragePathsExposeInstalledAndPortableAtariLocations()
    {
        var installed = StoragePaths.ResolveDataDirectory(
            AtariConfigurationStoreTestConstants.ApplicationDirectory,
            AtariConfigurationStoreTestConstants.RoamingDirectory);
        var portableRoot = CreateRoot();
        try
        {
            File.WriteAllText(Path.Combine(portableRoot, AtariConfigurationStoreTestConstants.PortableFlagName),
                string.Empty);
            var portable = StoragePaths.ResolveDataDirectory(portableRoot,
                AtariConfigurationStoreTestConstants.RoamingDirectory);
            Assert.Equal(Path.Combine(AtariConfigurationStoreTestConstants.RoamingDirectory,
                AtariConfigurationStoreTestConstants.ApplicationName), installed);
            Assert.Equal(Path.Combine(portableRoot, AtariConfigurationStoreTestConstants.DataDirectoryName), portable);
            Assert.EndsWith(Path.Combine(AtariConfigurationStoreTestConstants.MachinesDirectoryName,
                AtariConfigurationStoreTestConstants.AtariDirectoryName,
                AtariConfigurationStoreTestConstants.ConfigurationsDirectoryName),
                StoragePaths.AtariConfigurationsDirectory, StringComparison.OrdinalIgnoreCase);
            Assert.EndsWith(Path.Combine(AtariConfigurationStoreTestConstants.StatesDirectoryName,
                AtariConfigurationStoreTestConstants.AtariDirectoryName), StoragePaths.AtariStatesDirectory,
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteRoot(portableRoot);
        }
    }

    private static AtariMachineConfiguration Configuration(AtariMachineModel model, string data, int index) =>
        new(model,
            options: new Dictionary<string, string>
            {
                [AtariConfigurationStoreTestConstants.OptionKey] = index.ToString()
            },
            input: AtariConfigurationStoreTestConstants.Input,
            folders: new AtariFolderConfiguration(Shared: data),
            audioEnabled: index % AtariConfigurationStoreTestConstants.AlternatingDivisor
                          == AtariConfigurationStoreTestConstants.FirstIndex,
            videoRenderer: index % AtariConfigurationStoreTestConstants.AlternatingDivisor
                           == AtariConfigurationStoreTestConstants.FirstIndex
                ? EmulationVideoRenderer.Direct3D11
                : EmulationVideoRenderer.Wpf);

    private static string DocumentPath(string directory, Guid id) => Path.Combine(directory,
        id.ToString(AtariConfigurationStoreConstants.MachineIdentifierFormat),
        AtariConfigurationStoreConstants.MachineFileName);

    private static void WriteDocument(string root, string directoryName, string json)
    {
        var directory = Path.Combine(root, directoryName);
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, AtariConfigurationStoreConstants.MachineFileName), json);
    }

    private static void CreateFile(string path, byte[] contents)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, contents);
    }

    private static string CreateRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), AtariConfigurationStoreTestConstants.RootPrefix
            + Guid.NewGuid().ToString(AtariConfigurationStoreConstants.MachineIdentifierFormat));
        Directory.CreateDirectory(root);
        return root;
    }

    private static void DeleteRoot(string root)
    {
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }
}

internal static class AtariConfigurationStoreTestConstants
{
    internal const string RootPrefix = "gwgui-atari-config-";
    internal const string DataDirectoryName = "Data";
    internal const string ConfigurationsDirectoryName = "Configurations";
    internal const string ExternalDirectoryName = "External";
    internal const string FirmwareFileName = "Firmware/tos.img";
    internal const string InternalMediaFileName = "Media/system.st";
    internal const string ExternalMediaFileName = "external.st";
    internal const string CaptureDirectoryName = "Captures";
    internal const string StoredFirmwarePath = "Firmware/tos.img";
    internal const string StoredInternalMediaPath = "Media/system.st";
    internal const string SessionDirectoryName = "Sessions";
    internal const string CorruptDirectoryName = "corrupt";
    internal const string FutureDirectoryName = "future";
    internal const string CorruptJson = "{broken";
    internal const string FutureJson = "{\"schemaVersion\":999}";
    internal const string SharedFileName = "shared.bin";
    internal const string OptionKey = "test_option";
    internal const string ReplacementOptionValue = "replacement";
    internal const string ApplicationDirectory = "application";
    internal const string RoamingDirectory = "roaming";
    internal const string ApplicationName = "GW GUI";
    internal const string PortableFlagName = "portable.flag";
    internal const string MachinesDirectoryName = "Machines";
    internal const string AtariDirectoryName = "Atari";
    internal const string StatesDirectoryName = "States";
    internal const int FirstIndex = 0;
    internal const int SecondIndex = 1;
    internal const int AlternatingDivisor = 2;
    internal const int ConfigurationsPerFamily = 2;
    internal const int ExpectedConfigurationCount = 14;
    internal static readonly IReadOnlyDictionary<string, string> Options = new Dictionary<string, string>
    {
        [OptionKey] = "value"
    };
    internal static readonly IReadOnlyDictionary<string, string> ReplacementOptions =
        new Dictionary<string, string> { [OptionKey] = ReplacementOptionValue };
    internal static readonly AtariInputConfiguration Input = new(
        new Dictionary<string, EmulationKey> { ["START"] = EmulationKey.Return },
        MouseDeviceId: "mouse-test", CaptureMouse: true, ReleaseMouseKey: EmulationKey.F10);
    internal static readonly AtariMachineModel[] FamilyModels =
    [
        AtariMachineModel.St, AtariMachineModel.Falcon,
        AtariMachineModel.Atari400, AtariMachineModel.Atari800Xl,
        AtariMachineModel.Atari5200, AtariMachineModel.Atari5200,
        AtariMachineModel.Atari2600, AtariMachineModel.Atari2600,
        AtariMachineModel.Atari7800, AtariMachineModel.Atari7800,
        AtariMachineModel.Lynx, AtariMachineModel.Lynx,
        AtariMachineModel.Jaguar, AtariMachineModel.JaguarCd
    ];
    internal static readonly byte[] FirmwareBytes = [1];
    internal static readonly byte[] InternalMediaBytes = [2];
    internal static readonly byte[] ExternalMediaBytes = [3];
    internal static readonly byte[] SharedBytes = [4];
}
