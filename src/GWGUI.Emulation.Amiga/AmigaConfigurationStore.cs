using System.Text.Json;

namespace GWGUI.Emulation.Amiga;

public sealed class AmigaConfigurationStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private readonly string _directory;

    public AmigaConfigurationStore(string directory) => _directory = Path.GetFullPath(directory);

    public async Task<IReadOnlyList<AmigaMachineConfiguration>> LoadAllAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_directory);
        var configurations = new List<AmigaMachineConfiguration>();
        foreach (var path in Directory.EnumerateFiles(_directory, "*.json").Order(StringComparer.OrdinalIgnoreCase))
        {
            await using var stream = File.OpenRead(path);
            var configuration = await JsonSerializer.DeserializeAsync<AmigaMachineConfiguration>(stream, JsonOptions, cancellationToken);
            if (configuration is not null) configurations.Add(configuration.EnsureId());
        }
        return configurations;
    }

    public async Task SaveAsync(AmigaMachineConfiguration configuration, CancellationToken cancellationToken = default)
    {
        configuration = configuration.EnsureId();
        Directory.CreateDirectory(_directory);
        var target = Path.Combine(_directory, $"{configuration.Id:N}.json");
        var temporary = target + ".tmp";
        await using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
            await JsonSerializer.SerializeAsync(stream, configuration, JsonOptions, cancellationToken);
        File.Move(temporary, target, true);
    }

    public void Delete(Guid id)
    {
        var target = Path.Combine(_directory, $"{id:N}.json");
        if (File.Exists(target)) File.Delete(target);
    }
}
