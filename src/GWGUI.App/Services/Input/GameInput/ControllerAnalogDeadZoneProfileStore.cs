using GWGUI.App.Services.Storage;
using System.IO;
using System.Text.Json;

namespace GWGUI.App.Services.Input.GameInput;

internal static class ControllerAnalogDeadZoneProfileStore
{
    private static readonly object Sync = new();
    private static Dictionary<string, ControllerAnalogDeadZoneProfile>? _profiles;
    private static string FilePath => Path.Combine(
        StoragePaths.DataDirectory, "Controllers", "analog-dead-zones.json");

    internal static ControllerAnalogDeadZoneProfile Get(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId)) return ControllerAnalogDeadZoneProfile.Default;
        lock (Sync)
            return Profiles().GetValueOrDefault(deviceId, ControllerAnalogDeadZoneProfile.Default).Normalize();
    }

    internal static void Preview(string deviceId, ControllerAnalogDeadZoneProfile profile)
    {
        if (string.IsNullOrWhiteSpace(deviceId)) return;
        lock (Sync) Profiles()[deviceId] = profile.Normalize();
    }

    internal static void Save(string deviceId, ControllerAnalogDeadZoneProfile profile)
    {
        if (string.IsNullOrWhiteSpace(deviceId)) return;
        lock (Sync)
        {
            Profiles()[deviceId] = profile.Normalize();
            SaveProfiles();
        }
    }

    private static Dictionary<string, ControllerAnalogDeadZoneProfile> Profiles() =>
        _profiles ??= Load();

    private static Dictionary<string, ControllerAnalogDeadZoneProfile> Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return new(StringComparer.OrdinalIgnoreCase);
            var profiles = JsonSerializer.Deserialize<Dictionary<string, ControllerAnalogDeadZoneProfile>>(
                File.ReadAllText(FilePath));
            return profiles is null
                ? new(StringComparer.OrdinalIgnoreCase)
                : new(profiles.ToDictionary(item => item.Key, item => item.Value.Normalize()),
                    StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception exception) when (exception is IOException or JsonException or NotSupportedException)
        {
            return new(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static void SaveProfiles()
    {
        var directory = Path.GetDirectoryName(FilePath)!;
        Directory.CreateDirectory(directory);
        var temporary = FilePath + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(_profiles,
            new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporary, FilePath, true);
    }
}
