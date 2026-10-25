using CommonCoreHostFunctions = GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation;
using System.IO;
using GWGUI.Emulation.Functions;

namespace GWGUI.Emulation.Acorn.Common.Services;

internal sealed class ConfigurationStore
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
        => CommonCoreHostFunctions.ConfigurationFiles.ReadDocumentsAsync(_directory, ConfigurationPaths,
            static root => ConfigurationStoreFunctions.Deserialize(root),
            document => ConfigurationStoreFunctions.FromDocument(document, _pathBase), cancellationToken);

    public async Task SaveAsync(MachineConfiguration configuration, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        await CommonCoreHostFunctions.ConfigurationFiles.WithSaveGateAsync(SaveGate, () =>
        {
            var document = ConfigurationStoreFunctions.ToDocument(configuration, _pathBase);
            var machineDirectory = Path.Combine(_directory, configuration.Id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat));
            return ConfigurationStoreFunctions.WriteDocumentAtomicallyAsync(Path.Combine(machineDirectory, ConfigurationStoreConstants.MachineFileName), document, cancellationToken);
        }, cancellationToken).ConfigureAwait(false);
    }

    public void Delete(Guid id)
    {
        if (id == Guid.Empty) throw new ArgumentException(nameof(id));
        CommonCoreHostFunctions.ConfigurationFiles.DeleteDirectoryIfExists(Path.Combine(_directory, id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)), ConfigurationStoreConstants.RecursiveDirectoryDelete);
    }

    private IEnumerable<string> ConfigurationPaths() => CommonCoreHostFunctions.ConfigurationFiles.EnumerateDocumentPaths(_directory, ConfigurationStoreConstants.MachineFileName);
}

