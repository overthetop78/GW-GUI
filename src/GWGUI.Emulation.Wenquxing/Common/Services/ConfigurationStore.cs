using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Wenquxing.Common.Constants;
using GWGUI.Emulation.Wenquxing.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Wenquxing.Common.Machines.Common.Functions;
using System.IO;

namespace GWGUI.Emulation.Wenquxing.Common.Services;

public sealed class ConfigurationStore
{
    private readonly string _directory;
    private readonly string _pathBase;
    private static readonly SemaphoreSlim SaveGate = new(ConfigurationStoreConstants.InitialWriterCount, ConfigurationStoreConstants.MaximumWriterCount);

    public ConfigurationStore(string directory, string? pathBase = null)
    {
        _directory = Path.GetFullPath(string.IsNullOrWhiteSpace(directory) ? throw new ArgumentException(nameof(directory)) : directory);
        _pathBase = Path.GetFullPath(pathBase ?? directory);
    }

    public Task<IReadOnlyList<MachineConfiguration>> LoadAllAsync(CancellationToken cancellationToken = default)
        => ConfigurationFileAccessFunctions.ReadDocumentsAsync(_directory, ConfigurationPaths,
            static root => ConfigurationStoreFunctions.Deserialize(root),
            document => ConfigurationStoreFunctions.FromDocument(document, _pathBase), cancellationToken);

    public async Task SaveAsync(MachineConfiguration configuration, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ConfigurationFunctions.Validate(configuration);
        await ConfigurationFileAccessFunctions.WithSaveGateAsync(SaveGate, () =>
        {
            var document = ConfigurationStoreFunctions.ToDocument(configuration, _pathBase);
            var machineDirectory = Path.Combine(_directory, configuration.Id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat));
            return ConfigurationStoreFunctions.WriteDocumentAtomicallyAsync(Path.Combine(machineDirectory, ConfigurationStoreConstants.MachineFileName), document, cancellationToken);
        }, cancellationToken).ConfigureAwait(false);
    }

    public void Delete(Guid id)
    {
        if (id == Guid.Empty) throw new ArgumentException(nameof(id));
        var machineDirectory = Path.Combine(_directory, id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat));
        if (Directory.Exists(machineDirectory)) Directory.Delete(machineDirectory, ConfigurationStoreConstants.RecursiveDirectoryDelete);
    }

    private IEnumerable<string> ConfigurationPaths() => Directory.EnumerateDirectories(_directory).Select(directory => Path.Combine(directory, ConfigurationStoreConstants.MachineFileName)).Where(File.Exists).Order(StringComparer.OrdinalIgnoreCase);
}
