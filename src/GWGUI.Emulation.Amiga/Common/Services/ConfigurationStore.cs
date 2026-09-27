using System.Text.Json;
using GWGUI.Emulation.Functions;

namespace GWGUI.Emulation.Amiga.Common.Services;

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

    public async Task<IReadOnlyList<MachineConfiguration>> LoadAllAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_directory);
        var configurations = new List<MachineConfiguration>();
        var paths = Directory.EnumerateDirectories(_directory)
            .Select(directory => Path.Combine(directory, ConfigurationStoreConstants.MachineFileName))
            .Concat(Directory.EnumerateFiles(_directory, ConfigurationStoreConstants.JsonSearchPattern))
            .Where(File.Exists)
            .Order(StringComparer.OrdinalIgnoreCase);
        foreach (var path in paths)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var json = ConfigurationFileAccessFunctions.ReadAllText(path);
                var configuration = JsonConfigurationRecoveryFunctions
                    .DeserializeRemovingInvalidProperties(json, root =>
                        root.Deserialize<MachineConfiguration>(JsonOptions)
                        ?? throw new JsonException(),
                        out var repairedJson);
                if (!string.Equals(json, repairedJson, StringComparison.Ordinal))
                    await JsonConfigurationRecoveryFunctions.WriteAtomicallyAsync(path, repairedJson,
                        cancellationToken).ConfigureAwait(false);
                if (configuration is not null
                    && configuration.SchemaVersion is >= ConfigurationStoreConstants.MinimumSchemaVersion
                    and <= ConfigurationStoreConstants.CurrentSchemaVersion)
                    configurations.Add(ResolvePaths(configuration.EnsureId()));
            }
            catch (JsonException) { }
            catch (IOException) { }
        }
        return configurations;
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
        var legacy = Path.Combine(_directory,
            id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)
            + ConfigurationStoreConstants.LegacyFileExtension);
        if (File.Exists(legacy)) File.Delete(legacy);
    }

    private MachineConfiguration StorePaths(MachineConfiguration configuration) => configuration with
    {
        KickstartPath = StorePath(configuration.KickstartPath)!,
        InitialDiskPath = StorePath(configuration.InitialDiskPath),
        ExtendedRomPath = StorePath(configuration.ExtendedRomPath),
        RomKeyPath = StorePath(configuration.RomKeyPath),
        Floppies = configuration.Floppies?.Select(floppy => floppy with { Path = StorePath(floppy.Path)! }).ToArray(),
        Media = configuration.Media?.Select(media => media with { Path = StorePath(media.Path)! }).ToArray()
    };

    private MachineConfiguration ResolvePaths(MachineConfiguration configuration) => configuration with
    {
        KickstartPath = ResolvePath(configuration.KickstartPath)!,
        InitialDiskPath = ResolvePath(configuration.InitialDiskPath),
        ExtendedRomPath = ResolvePath(configuration.ExtendedRomPath),
        RomKeyPath = ResolvePath(configuration.RomKeyPath),
        Floppies = configuration.Floppies?.Select(floppy => floppy with { Path = ResolvePath(floppy.Path)! }).ToArray(),
        Media = configuration.Media?.Select(media => media with { Path = ResolvePath(media.Path)! }).ToArray()
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
