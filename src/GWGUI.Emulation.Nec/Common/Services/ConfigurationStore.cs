using System.IO;
using System.Text.Json;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Constants;

namespace GWGUI.Emulation.Nec.Common.Services;

public sealed class ConfigurationStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = ConfigurationStoreConstants.WriteIndentedJson
    };
    private readonly string _directory;
    private readonly string _pathBase;
    private static readonly SemaphoreSlim SaveGate = new(
        ConfigurationStoreConstants.InitialWriterCount,
        ConfigurationStoreConstants.MaximumWriterCount);

    public ConfigurationStore(string directory, string? pathBase = null)
    {
        _directory = Path.GetFullPath(directory);
        _pathBase = Path.GetFullPath(pathBase ?? directory);
    }

    public string MemoryDirectory(Guid id) => Path.Combine(_directory,
        id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat),
        PcFxBackupMemoryConstants.MemoryDirectoryName);

    public Task<IReadOnlyList<MachineConfiguration>> LoadAllAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_directory);
        var configurations = new List<MachineConfiguration>();
        var paths = Directory.EnumerateDirectories(_directory)
            .Select(directory => Path.Combine(directory, ConfigurationStoreConstants.MachineFileName))
            .Where(File.Exists)
            .Order(StringComparer.OrdinalIgnoreCase);
        foreach (var path in paths)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var json = ConfigurationFileAccessFunctions.ReadAllText(path);
                var configuration = JsonSerializer.Deserialize<MachineConfiguration>(json, JsonOptions);
                if (configuration is not null
                    && configuration.SchemaVersion == ConfigurationStoreConstants.CurrentSchemaVersion)
                    configurations.Add(ResolvePaths(configuration.EnsureId()));
            }
            catch (JsonException) { }
            catch (IOException) { }
        }
        return Task.FromResult<IReadOnlyList<MachineConfiguration>>(configurations);
    }

    public async Task SaveAsync(MachineConfiguration configuration, CancellationToken cancellationToken = default)
    {
        await SaveGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        string? temporary = null;
        try
        {
            configuration = configuration.EnsureId() with
            {
                SchemaVersion = ConfigurationStoreConstants.CurrentSchemaVersion
            };
            var machineDirectory = Path.Combine(_directory,
                configuration.Id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat));
            Directory.CreateDirectory(machineDirectory);
            var target = Path.Combine(machineDirectory, ConfigurationStoreConstants.MachineFileName);
            temporary = target + ConfigurationStoreConstants.TemporaryNameSeparator
                + Guid.NewGuid().ToString(
                ConfigurationStoreConstants.MachineIdentifierFormat)
                + ConfigurationStoreConstants.TemporaryFileSuffix;
            await using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write,
                             FileShare.None, ConfigurationStoreConstants.WriteBufferSize,
                             ConfigurationStoreConstants.UseAsyncFileAccess))
                await JsonSerializer.SerializeAsync(stream, StorePaths(configuration), JsonOptions, cancellationToken)
                    .ConfigureAwait(false);
            ConfigurationFileAccessFunctions.ReplaceFile(temporary, target);
        }
        finally
        {
            if (temporary is not null && File.Exists(temporary)) File.Delete(temporary);
            SaveGate.Release();
        }
    }

    public void Delete(Guid id)
    {
        var target = Path.Combine(_directory,
            id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat));
        if (Directory.Exists(target))
            Directory.Delete(target, ConfigurationStoreConstants.RecursiveDirectoryDelete);
    }

    private MachineConfiguration StorePaths(MachineConfiguration configuration) => configuration with
    {
        Media = configuration.Media?.Select(media => media with { Path = StorePath(media.Path)! }).ToArray(),
        FirmwarePath = StorePath(configuration.FirmwarePath),
        FirmwarePaths = configuration.FirmwarePaths?.ToDictionary(item => item.Key,
            item => StorePath(item.Value)!, StringComparer.Ordinal)
    };

    private MachineConfiguration ResolvePaths(MachineConfiguration configuration) => configuration with
    {
        Media = configuration.Media?.Select(media => media with { Path = ResolvePath(media.Path)! }).ToArray(),
        FirmwarePath = ResolvePath(configuration.FirmwarePath),
        FirmwarePaths = configuration.FirmwarePaths?.ToDictionary(item => item.Key,
            item => ResolvePath(item.Value)!, StringComparer.Ordinal)
    };

    private string? StorePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        var fullPath = Path.GetFullPath(path);
        var relative = Path.GetRelativePath(_pathBase, fullPath);
        if (Path.IsPathFullyQualified(relative)) return fullPath;
        if (relative != ConfigurationStoreConstants.ParentDirectoryName
            && !relative.StartsWith(ConfigurationStoreConstants.ParentDirectoryName
                + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            return relative.Replace(Path.DirectorySeparatorChar,
                ConfigurationStoreConstants.StoredDirectorySeparator);
        return fullPath;
    }

    private string? ResolvePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        if (Path.IsPathFullyQualified(path)) return path;
        return Path.GetFullPath(Path.Combine(_pathBase,
            path.Replace(ConfigurationStoreConstants.StoredDirectorySeparator,
                Path.DirectorySeparatorChar)));
    }
}
