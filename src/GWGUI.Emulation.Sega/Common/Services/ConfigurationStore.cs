using System.IO;
using System.Text.Json;
using GWGUI.Emulation.Sega.Common.Machines.Common.Contracts;

namespace GWGUI.Emulation.Sega.Common.Services;

public sealed class ConfigurationStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
    private readonly string _directory;

    public ConfigurationStore(string directory)
    {
        _directory = Path.GetFullPath(directory);
    }

    public async Task<IReadOnlyList<MachineConfiguration>> LoadAllAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_directory);
        var result = new List<MachineConfiguration>();
        foreach (var path in Directory.EnumerateFiles(_directory, "*.json", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await using var stream = File.OpenRead(path);
                var configuration = await JsonSerializer.DeserializeAsync<MachineConfiguration>(stream, JsonOptions, cancellationToken).ConfigureAwait(false);
                if (configuration is not null) result.Add(configuration.EnsureId());
            }
            catch (JsonException) { }
            catch (IOException) { }
        }
        return result;
    }

    public async Task SaveAsync(MachineConfiguration configuration, CancellationToken cancellationToken = default)
    {
        configuration = configuration.EnsureId();
        var directory = Path.Combine(_directory, configuration.Id.ToString("N"));
        Directory.CreateDirectory(directory);
        var target = Path.Combine(directory, "machine.json");
        var temporary = target + ".tmp";
        try
        {
            await using (var stream = File.Create(temporary))
                await JsonSerializer.SerializeAsync(stream, configuration, JsonOptions, cancellationToken).ConfigureAwait(false);
            File.Move(temporary, target, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    public void Delete(Guid id)
    {
        var directory = Path.Combine(_directory, id.ToString("N"));
        if (Directory.Exists(directory)) Directory.Delete(directory, true);
    }
}
