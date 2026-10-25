using CommonCoreHostFunctions = GWGUI.Emulation.Nintendo.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation;
using System.IO;
using System.Text.Json;
using GWGUI.Emulation.Functions;

namespace GWGUI.Emulation.Nintendo.Common.Services;

internal sealed class ConfigurationStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = ConfigurationStoreConstants.WriteIndentedJson
    };
    private readonly string _directory;
    private readonly string _pathBase;
    private static readonly SemaphoreSlim SaveGate = new(ConfigurationStoreConstants.InitialWriterCount, ConfigurationStoreConstants.MaximumWriterCount);

    public ConfigurationStore(string directory, string? pathBase = null)
    {
        _directory = Path.GetFullPath(directory);
        _pathBase = Path.GetFullPath(pathBase ?? directory);
    }

    public Task<IReadOnlyList<MachineConfiguration>> LoadAllAsync(CancellationToken cancellationToken = default)
        => CommonCoreHostFunctions.ConfigurationFiles.ReadDocumentsAsync(_directory, ConfigurationStoreConstants.MachineFileName,
            static json => JsonSerializer.Deserialize<MachineConfiguration>(json, JsonOptions),
            static value => value.SchemaVersion == ConfigurationStoreConstants.CurrentSchemaVersion,
            value => ResolvePaths(value.EnsureId()), cancellationToken);

    public Task SaveAsync(MachineConfiguration configuration, CancellationToken cancellationToken = default)
        => CommonCoreHostFunctions.ConfigurationFiles.WriteDocumentAsync(SaveGate, _directory, configuration,
            static value => value.EnsureId() with { SchemaVersion = ConfigurationStoreConstants.CurrentSchemaVersion },
            static value => value.Id, StorePaths, JsonOptions, cancellationToken);

    public void Delete(Guid id) => CommonCoreHostFunctions.ConfigurationFiles.DeleteDirectoryIfExists(Path.Combine(_directory, id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)), ConfigurationStoreConstants.RecursiveDirectoryDelete);

    private MachineConfiguration StorePaths(MachineConfiguration configuration) => configuration with
    {
        Media = configuration.Media?.Select(media => media with { Path = StorePath(media.Path)! }).ToArray(),
        FirmwarePaths = configuration.FirmwarePaths?.ToDictionary(item => item.Key, item => StorePath(item.Value)!, StringComparer.Ordinal)
    };

    private MachineConfiguration ResolvePaths(MachineConfiguration configuration) => configuration with
    {
        Media = configuration.Media?.Select(media => media with { Path = ResolvePath(media.Path)! }).ToArray(),
        FirmwarePaths = configuration.FirmwarePaths?.ToDictionary(item => item.Key, item => ResolvePath(item.Value)!, StringComparer.Ordinal)
    };

    private string? StorePath(string? path)
        => CommonCoreHostFunctions.ConfigurationFiles.StorePath(string.IsNullOrWhiteSpace(path) ? null : path, _pathBase);

    private string? ResolvePath(string? path)
        => CommonCoreHostFunctions.ConfigurationFiles.ResolvePath(string.IsNullOrWhiteSpace(path) ? null : path, _pathBase);
}
