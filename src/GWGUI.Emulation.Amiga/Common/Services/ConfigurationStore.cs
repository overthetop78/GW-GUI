using System.Text.Json;
using GWGUI.Emulation.Functions;

namespace GWGUI.Emulation.Amiga.Common.Services;

public sealed class ConfigurationStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private readonly string _directory;
    private readonly string _pathBase;
    private static readonly SemaphoreSlim SaveGate = new(1, 1);

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
            .Select(directory => Path.Combine(directory, ConfigurationStoreConstants.MachineJson))
            .Concat(Directory.EnumerateFiles(_directory, ConfigurationStoreConstants.Json))
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
                        ?? throw new JsonException("The Amiga configuration is empty."),
                        out var repairedJson);
                if (!string.Equals(json, repairedJson, StringComparison.Ordinal))
                    await JsonConfigurationRecoveryFunctions.WriteAtomicallyAsync(path, repairedJson,
                        cancellationToken).ConfigureAwait(false);
                if (configuration is not null && configuration.SchemaVersion is > 0 and <= 3)
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
            configuration = configuration.EnsureId() with { SchemaVersion = 3 };
            var machineDirectory = Path.Combine(_directory, configuration.Id.ToString(ConfigurationStoreConstants.N));
            Directory.CreateDirectory(machineDirectory);
            var target = Path.Combine(machineDirectory, ConfigurationStoreConstants.MachineJson);
            temporary = target + "." + Guid.NewGuid().ToString(ConfigurationStoreConstants.N)
                + ConfigurationStoreConstants.Tmp;
            await using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                await JsonSerializer.SerializeAsync(stream, StorePaths(configuration), JsonOptions, cancellationToken)
                    .ConfigureAwait(false);
            ConfigurationFileAccessFunctions.ReplaceFile(temporary, target,
                ConfigurationStoreConstants.ReplacementRetryCount,
                ConfigurationStoreConstants.ReplacementRetryDelayMilliseconds);
        }
        finally
        {
            if (temporary is not null && File.Exists(temporary)) File.Delete(temporary);
            SaveGate.Release();
        }
    }

    public void Delete(Guid id)
    {
        var target = Path.Combine(_directory, id.ToString(ConfigurationStoreConstants.N));
        if (Directory.Exists(target)) Directory.Delete(target, true);
        var legacy = Path.Combine(_directory, $"{id:N}.json");
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
        if (relative != ConfigurationStoreConstants.Value && !relative.StartsWith(ConfigurationStoreConstants.Value + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            return relative.Replace(Path.DirectorySeparatorChar, '/');
        return fullPath;
    }

    private string? ResolvePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        if (Path.IsPathFullyQualified(path)) return path;
        return Path.GetFullPath(Path.Combine(_pathBase, path.Replace('/', Path.DirectorySeparatorChar)));
    }
}
