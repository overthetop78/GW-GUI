using System.IO;
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

    public Task<IReadOnlyList<MachineConfiguration>> LoadAllAsync(
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
                using var parsed = JsonDocument.Parse(json);
                var document = ConfigurationStoreFunctions.Deserialize(parsed.RootElement);
                configurations.Add(ConfigurationStoreFunctions.FromDocument(document, _pathBase));
            }
            catch (JsonException) { }
            catch (IOException) { }
            catch (InvalidDataException) { }
            catch (ArgumentException) { }
        }
        return Task.FromResult<IReadOnlyList<MachineConfiguration>>(configurations);
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
    }

    private IEnumerable<string> ConfigurationPaths() => Directory.EnumerateDirectories(_directory)
        .Select(directory => Path.Combine(directory, ConfigurationStoreConstants.MachineFileName))
        .Where(File.Exists)
        .Order(StringComparer.OrdinalIgnoreCase);
}
