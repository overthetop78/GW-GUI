using GWGUI.Emulation;
using System.IO;
using System.Text.Json;
using GWGUI.Emulation.Functions;

namespace GWGUI.Emulation.Commodore.Common.Services;

public sealed class ConfigurationStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = ConfigurationStoreConstants.WriteIndentedJson,
        UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow
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
        => EmulationSdk.ConfigurationFiles.ReadDocumentsAsync(_directory, ConfigurationStoreConstants.MachineFileName,
            static json => JsonSerializer.Deserialize<MachineConfiguration>(json, JsonOptions),
            static value => value.SchemaVersion == ConfigurationStoreConstants.CurrentSchemaVersion,
            value => ResolvePaths(value.EnsureId()), cancellationToken);

    public Task SaveAsync(MachineConfiguration configuration, CancellationToken cancellationToken = default)
        => EmulationSdk.ConfigurationFiles.WriteDocumentAsync(SaveGate, _directory, configuration,
            static value => value.EnsureId() with { SchemaVersion = ConfigurationStoreConstants.CurrentSchemaVersion },
            static value => value.Id, StorePaths, JsonOptions, cancellationToken);

    public void Delete(Guid id) => EmulationSdk.ConfigurationFiles.DeleteDirectoryIfExists(Path.Combine(_directory, id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)), ConfigurationStoreConstants.RecursiveDirectoryDelete);

    private MachineConfiguration StorePaths(MachineConfiguration configuration) => configuration with
    {
        FirmwarePaths = configuration.FirmwarePaths?.ToDictionary(pair => pair.Key, pair => StorePath(pair.Value)!, StringComparer.Ordinal),
        InitialDiskPath = StorePath(configuration.InitialDiskPath),
        Floppies = configuration.Floppies?.Select(floppy => floppy with { Path = StorePath(floppy.Path)! }).ToArray(),
        Media = configuration.Media?.Select(media => media with { Path = StorePath(media.Path)! }).ToArray()
    };

    private MachineConfiguration ResolvePaths(MachineConfiguration configuration) => configuration with
    {
        FirmwarePaths = configuration.FirmwarePaths?.ToDictionary(pair => pair.Key, pair => ResolvePath(pair.Value)!, StringComparer.Ordinal),
        InitialDiskPath = ResolvePath(configuration.InitialDiskPath),
        Floppies = configuration.Floppies?.Select(floppy => floppy with { Path = ResolvePath(floppy.Path)! }).ToArray(),
        Media = configuration.Media?.Select(media => media with { Path = ResolvePath(media.Path)! }).ToArray()
    };

    private string? StorePath(string? path)
        => EmulationSdk.ConfigurationFiles.StorePath(string.IsNullOrWhiteSpace(path) ? null : path, _pathBase);

    private string? ResolvePath(string? path)
        => EmulationSdk.ConfigurationFiles.ResolvePath(string.IsNullOrWhiteSpace(path) ? null : path, _pathBase);
}
