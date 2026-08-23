using System.Text.Json;

namespace GWGUI.Emulation.Amiga;

public sealed class AmigaConfigurationStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private readonly string _directory;
    private readonly string _pathBase;

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
            .Select(directory => Path.Combine(directory, "machine.json"))
            .Concat(Directory.EnumerateFiles(_directory, "*.json"))
            .Where(File.Exists)
            .Order(StringComparer.OrdinalIgnoreCase);
        foreach (var path in paths)
        {
            try
            {
                await using var stream = File.OpenRead(path);
                var configuration = await JsonSerializer.DeserializeAsync<AmigaMachineConfiguration>(stream, JsonOptions, cancellationToken);
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
        configuration = configuration.EnsureId() with { SchemaVersion = 3 };
        var machineDirectory = Path.Combine(_directory, configuration.Id.ToString("N"));
        Directory.CreateDirectory(machineDirectory);
        var target = Path.Combine(machineDirectory, "machine.json");
        var temporary = target + ".tmp";
        await using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
            await JsonSerializer.SerializeAsync(stream, StorePaths(configuration), JsonOptions, cancellationToken);
        File.Move(temporary, target, true);
    }

    public void Delete(Guid id)
    {
        var target = Path.Combine(_directory, id.ToString("N"));
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
        if (relative != ".." && !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
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
