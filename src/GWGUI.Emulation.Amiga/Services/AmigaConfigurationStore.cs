using System.Text.Json;
using GWGUI.Emulation.Functions;

namespace GWGUI.Emulation.Amiga.Services;

public sealed class AmigaConfigurationStore
{
    private const string SaveMutexName = @"Local\GWGUI.AmigaConfigurationStore.Save";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private readonly string _directory;
    private readonly string _pathBase;
    private static readonly SemaphoreSlim SaveGate = new(1, 1);
    private static readonly Mutex SaveMutex = new(false, SaveMutexName);

    public AmigaConfigurationStore(string directory, string? pathBase = null)
    {
        _directory = Path.GetFullPath(directory);
        _pathBase = Path.GetFullPath(pathBase ?? directory);
    }

    public async Task<IReadOnlyList<AmigaMachineConfiguration>> LoadAllAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_directory);
        var configurations = new List<AmigaMachineConfiguration>();
        var paths = Directory.EnumerateDirectories(_directory)
            .Select(directory => Path.Combine(directory, AmigaConfigurationStoreConstants.MachineJson))
            .Concat(Directory.EnumerateFiles(_directory, AmigaConfigurationStoreConstants.Json))
            .Where(File.Exists)
            .Order(StringComparer.OrdinalIgnoreCase);
        foreach (var path in paths)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var json = ReadAllText(path);
                var configuration = JsonConfigurationRecoveryFunctions
                    .DeserializeRemovingInvalidProperties(json, root =>
                        root.Deserialize<AmigaMachineConfiguration>(JsonOptions)
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

    public async Task SaveAsync(AmigaMachineConfiguration configuration, CancellationToken cancellationToken = default)
    {
        await SaveGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        string? temporary = null;
        try
        {
            configuration = configuration.EnsureId() with { SchemaVersion = 3 };
            var machineDirectory = Path.Combine(_directory, configuration.Id.ToString(AmigaConfigurationStoreConstants.N));
            Directory.CreateDirectory(machineDirectory);
            var target = Path.Combine(machineDirectory, AmigaConfigurationStoreConstants.MachineJson);
            temporary = target + "." + Guid.NewGuid().ToString(AmigaConfigurationStoreConstants.N)
                + AmigaConfigurationStoreConstants.Tmp;
            await using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                await JsonSerializer.SerializeAsync(stream, StorePaths(configuration), JsonOptions, cancellationToken)
                    .ConfigureAwait(false);
            ReplaceFile(temporary, target);
        }
        finally
        {
            if (temporary is not null && File.Exists(temporary)) File.Delete(temporary);
            SaveGate.Release();
        }
    }

    private static string ReadAllText(string path)
    {
        var lockTaken = false;
        try
        {
            try
            {
                SaveMutex.WaitOne();
                lockTaken = true;
            }
            catch (AbandonedMutexException)
            {
                lockTaken = true;
            }
            return File.ReadAllText(path);
        }
        finally
        {
            if (lockTaken) SaveMutex.ReleaseMutex();
        }
    }
    private static void ReplaceFile(string source, string target)
    {
        var lockTaken = false;
        try
        {
            try
            {
                SaveMutex.WaitOne();
                lockTaken = true;
            }
            catch (AbandonedMutexException)
            {
                lockTaken = true;
            }
            for (var attempt = 0; attempt < AmigaConfigurationStoreConstants.ReplacementRetryCount; attempt++)
            {
                try
                {
                    File.Move(source, target, true);
                    return;
                }
                catch (UnauthorizedAccessException)
                {
                    if (attempt + 1 >= AmigaConfigurationStoreConstants.ReplacementRetryCount) break;
                    Thread.Sleep(AmigaConfigurationStoreConstants.ReplacementRetryDelayMilliseconds * (attempt + 1));
                }
                catch (IOException)
                {
                    if (attempt + 1 >= AmigaConfigurationStoreConstants.ReplacementRetryCount) break;
                    Thread.Sleep(AmigaConfigurationStoreConstants.ReplacementRetryDelayMilliseconds * (attempt + 1));
                }
            }

            using (var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var output = new FileStream(target, FileMode.Create, FileAccess.Write,
                       FileShare.ReadWrite, 4096, FileOptions.WriteThrough))
            {
                input.CopyTo(output);
                output.Flush(flushToDisk: true);
            }
            File.Delete(source);
        }
        finally
        {
            if (lockTaken) SaveMutex.ReleaseMutex();
        }
    }

    public void Delete(Guid id)
    {
        var target = Path.Combine(_directory, id.ToString(AmigaConfigurationStoreConstants.N));
        if (Directory.Exists(target)) Directory.Delete(target, true);
        var legacy = Path.Combine(_directory, $"{id:N}.json");
        if (File.Exists(legacy)) File.Delete(legacy);
    }

    private AmigaMachineConfiguration StorePaths(AmigaMachineConfiguration configuration) => configuration with
    {
        KickstartPath = StorePath(configuration.KickstartPath)!,
        InitialDiskPath = StorePath(configuration.InitialDiskPath),
        ExtendedRomPath = StorePath(configuration.ExtendedRomPath),
        RomKeyPath = StorePath(configuration.RomKeyPath),
        Floppies = configuration.Floppies?.Select(floppy => floppy with { Path = StorePath(floppy.Path)! }).ToArray(),
        Media = configuration.Media?.Select(media => media with { Path = StorePath(media.Path)! }).ToArray()
    };

    private AmigaMachineConfiguration ResolvePaths(AmigaMachineConfiguration configuration) => configuration with
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
        if (relative != AmigaConfigurationStoreConstants.Value && !relative.StartsWith(AmigaConfigurationStoreConstants.Value + Path.DirectorySeparatorChar, StringComparison.Ordinal))
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
