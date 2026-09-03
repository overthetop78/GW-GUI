using System.Text.Json;
using GWGUI.Emulation.Functions;

namespace GWGUI.Emulation.Atari.Services;

public sealed class AtariConfigurationStore
{
    private readonly string _directory;
    private readonly string _pathBase;
    private static readonly SemaphoreSlim SaveGate = new(1, 1);
    private int _activeLoads;

    public AtariConfigurationStore(string directory, string? pathBase = null)
    {
        _directory = Path.GetFullPath(
            string.IsNullOrWhiteSpace(directory) ? throw new ArgumentException(nameof(directory)) : directory);
        _pathBase = Path.GetFullPath(pathBase ?? directory);
    }

    public bool IsLoading => Volatile.Read(ref _activeLoads) > AtariConfigurationStoreConstants.NoActiveLoads;

    public async Task<IReadOnlyList<AtariMachineConfiguration>> LoadAllAsync(
        CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _activeLoads);
        try
        {
            Directory.CreateDirectory(_directory);
            var configurations = new List<AtariMachineConfiguration>();
            foreach (var path in ConfigurationPaths())
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var json = ConfigurationFileAccessFunctions.ReadAllText(path);
                    var document = JsonConfigurationRecoveryFunctions
                        .DeserializeRemovingInvalidProperties(json,
                            AtariConfigurationMigrationFunctions.MigrateToCurrent,
                            out var repairedJson);
                    if (!string.Equals(json, repairedJson, StringComparison.Ordinal))
                        await JsonConfigurationRecoveryFunctions.WriteAtomicallyAsync(path, repairedJson,
                            cancellationToken).ConfigureAwait(false);
                    configurations.Add(AtariConfigurationStoreFunctions.FromDocument(document, _pathBase));
                }
                catch (JsonException) { }
                catch (IOException) { }
                catch (InvalidDataException) { }
                catch (ArgumentException) { }
            }
            return configurations;
        }
        finally
        {
            Interlocked.Decrement(ref _activeLoads);
        }
    }

    public async Task SaveAsync(AtariMachineConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        await SaveGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var document = AtariConfigurationStoreFunctions.ToDocument(configuration, _pathBase);
            var machineDirectory = Path.Combine(_directory,
                configuration.Id.ToString(AtariConfigurationStoreConstants.MachineIdentifierFormat));
            await AtariConfigurationStoreFunctions.WriteDocumentAtomicallyAsync(
                Path.Combine(machineDirectory, AtariConfigurationStoreConstants.MachineFileName), document,
                cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            SaveGate.Release();
        }
    }

    public void Delete(Guid id)
    {
        if (id == Guid.Empty) throw new ArgumentException(nameof(id));
        var machineDirectory = Path.Combine(_directory,
            id.ToString(AtariConfigurationStoreConstants.MachineIdentifierFormat));
        if (Directory.Exists(machineDirectory)) Directory.Delete(machineDirectory, recursive: true);
        var legacyPath = Path.Combine(_directory,
            id.ToString(AtariConfigurationStoreConstants.MachineIdentifierFormat)
            + AtariConfigurationStoreConstants.LegacyFileExtension);
        if (File.Exists(legacyPath)) File.Delete(legacyPath);
    }

    private IEnumerable<string> ConfigurationPaths() => Directory.EnumerateDirectories(_directory)
        .Select(directory => Path.Combine(directory, AtariConfigurationStoreConstants.MachineFileName))
        .Concat(Directory.EnumerateFiles(_directory, AtariConfigurationStoreConstants.JsonSearchPattern))
        .Where(File.Exists)
        .Order(StringComparer.OrdinalIgnoreCase);
}
