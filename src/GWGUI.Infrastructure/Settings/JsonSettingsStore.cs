using GWGUI.Domain.Settings;
using System.Text.Json;

namespace GWGUI.Infrastructure.Settings;

public sealed class JsonSettingsStore(string filePath) : ISettingsStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath)) return new AppSettings();
        try
        {
            return SettingsMigrator.Migrate(await DeserializeAsync(filePath, cancellationToken).ConfigureAwait(false));
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException)
        {
            PreserveInvalid(filePath);
            var backup = filePath + ".bak";
            if (File.Exists(backup))
            {
                try
                {
                    var recovered = SettingsMigrator.Migrate(await DeserializeAsync(backup, cancellationToken).ConfigureAwait(false));
                    File.Copy(backup, filePath, overwrite: true);
                    return recovered;
                }
                catch (Exception backupException) when (backupException is JsonException or NotSupportedException) { PreserveInvalid(backup); }
            }
            return new AppSettings();
        }
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        settings = SettingsMigrator.Migrate(settings);
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        var temporary = filePath + ".tmp";
        await using (var stream = File.Create(temporary))
            await JsonSerializer.SerializeAsync(stream, settings, Options, cancellationToken).ConfigureAwait(false);
        if (File.Exists(filePath)) File.Copy(filePath, filePath + ".bak", overwrite: true);
        File.Move(temporary, filePath, overwrite: true);
    }

    private static async Task<AppSettings> DeserializeAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<AppSettings>(stream, Options, cancellationToken).ConfigureAwait(false) ?? new AppSettings();
    }

    private static void PreserveInvalid(string path)
    {
        var destination = path + ".invalid-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff");
        File.Copy(path, destination, overwrite: false);
    }
}
