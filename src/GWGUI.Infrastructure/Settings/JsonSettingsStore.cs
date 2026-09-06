using GWGUI.Domain.Settings;
using System.Text.Json;

namespace GWGUI.Infrastructure.Settings;

public sealed class JsonSettingsStore : ISettingsStore
{
    private readonly string filePath;
    private readonly ISettingsFileSystem files;
    private readonly Func<DateTime> utcNow;

    public JsonSettingsStore(string filePath) : this(filePath, new PhysicalSettingsFileSystem(), () => DateTime.UtcNow) { }

    public JsonSettingsStore(string filePath, ISettingsFileSystem files, Func<DateTime> utcNow)
    {
        this.filePath = filePath;
        this.files = files ?? throw new ArgumentNullException(nameof(files));
        this.utcNow = utcNow ?? throw new ArgumentNullException(nameof(utcNow));
    }
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!files.Exists(filePath)) return new AppSettings();
        try
        {
            return SettingsMigrator.Migrate(await DeserializeAsync(filePath, cancellationToken).ConfigureAwait(false));
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException)
        {
            PreserveInvalid(filePath);
            var backup = filePath + ".bak";
            if (files.Exists(backup))
            {
                try
                {
                    var recovered = SettingsMigrator.Migrate(await DeserializeAsync(backup, cancellationToken).ConfigureAwait(false));
                    files.Copy(backup, filePath, overwrite: true);
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
        if (!string.IsNullOrWhiteSpace(directory)) files.CreateDirectory(directory);
        var temporary = filePath + ".tmp";
        await using (var stream = files.Create(temporary))
            await JsonSerializer.SerializeAsync(stream, settings, Options, cancellationToken).ConfigureAwait(false);
        if (files.Exists(filePath)) files.Copy(filePath, filePath + ".bak", overwrite: true);
        files.Move(temporary, filePath, overwrite: true);
    }

    private async Task<AppSettings> DeserializeAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = files.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<AppSettings>(stream, Options, cancellationToken).ConfigureAwait(false) ?? new AppSettings();
    }

    private void PreserveInvalid(string path)
    {
        var destination = path + ".invalid-" + utcNow().ToString("yyyyMMdd-HHmmssfff");
        files.Copy(path, destination, overwrite: false);
    }
}
