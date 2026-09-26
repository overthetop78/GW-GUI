using System.Text.Json;
using GWGUI.Emulation.Functions;

namespace GWGUI.Emulation.Atari.Common.Services;

public sealed class ConfigurationStore
{
    private readonly string _directory;
    private readonly string _pathBase;
    private static readonly SemaphoreSlim SaveGate = new(
        ConfigurationStoreConstants.InitialWriterCount,
        ConfigurationStoreConstants.MaximumWriterCount);

    public ConfigurationStore(string directory, string? pathBase = null)
    {
        _directory = Path.GetFullPath(
            string.IsNullOrWhiteSpace(directory) ? throw new ArgumentException(nameof(directory)) : directory);
        _pathBase = Path.GetFullPath(pathBase ?? directory);
    }

    public async Task<IReadOnlyList<MachineConfiguration>> LoadAllAsync(
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_directory);
        var configurations = new List<MachineConfiguration>();
        foreach (var path in ConfigurationPaths())
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var json = ConfigurationFileAccessFunctions.ReadAllText(path);
                var document = JsonConfigurationRecoveryFunctions
                    .DeserializeRemovingInvalidProperties(json,
                        ConfigurationStoreFunctions.Deserialize,
                        out var repairedJson);
                if (!string.Equals(json, repairedJson, StringComparison.Ordinal))
                    await JsonConfigurationRecoveryFunctions.WriteAtomicallyAsync(path, repairedJson,
                        cancellationToken).ConfigureAwait(false);
                configurations.Add(ConfigurationStoreFunctions.FromDocument(document, _pathBase));
            }
            catch (JsonException) { }
            catch (IOException) { }
            catch (InvalidDataException) { }
            catch (ArgumentException) { }
        }
        return configurations;
    }

    public async Task SaveAsync(MachineConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        await SaveGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var document = ConfigurationStoreFunctions.ToDocument(configuration, _pathBase);
            var machineDirectory = Path.Combine(_directory,
                configuration.Id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat));
            await ConfigurationStoreFunctions.WriteDocumentAtomicallyAsync(
                Path.Combine(machineDirectory, ConfigurationStoreConstants.MachineFileName), document,
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
            id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat));
        if (Directory.Exists(machineDirectory))
            Directory.Delete(machineDirectory, ConfigurationStoreConstants.RecursiveDirectoryDelete);
        var legacyPath = Path.Combine(_directory,
            id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)
            + ConfigurationStoreConstants.LegacyFileExtension);
        if (File.Exists(legacyPath)) File.Delete(legacyPath);
    }

    private IEnumerable<string> ConfigurationPaths() => Directory.EnumerateDirectories(_directory)
        .Select(directory => Path.Combine(directory, ConfigurationStoreConstants.MachineFileName))
        .Concat(Directory.EnumerateFiles(_directory, ConfigurationStoreConstants.JsonSearchPattern))
        .Where(File.Exists)
        .Order(StringComparer.OrdinalIgnoreCase);
}
