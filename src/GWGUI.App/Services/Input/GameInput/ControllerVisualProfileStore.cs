using GWGUI.App.Services.Storage;
using System.IO;
using System.Text.Json;

namespace GWGUI.App.Services.Input.GameInput;

internal static class ControllerVisualProfileStore
{
    private static readonly object Sync = new();
    private static Dictionary<string, ControllerVisualProfile>? _profiles;
    private static string FilePath => Path.Combine(StoragePaths.DataDirectory,
        "Controllers", "visual-models.json");

    internal static string DisplayName(ControllerVisualModel model, string fallback) => model switch
    {
        ControllerVisualModel.NintendoEntertainmentSystem => "Manette NES",
        ControllerVisualModel.Nintendo64 => "Manette Nintendo 64",
        ControllerVisualModel.SuperNintendo => "Manette Super Nintendo",
        ControllerVisualModel.MegaDrive3 => "SEGA Mega Drive 3 boutons",
        ControllerVisualModel.MegaDrive6 => "SEGA Mega Drive 6 boutons",
        _ => fallback
    };

    internal static IReadOnlyDictionary<string, ControllerVisualModel> GetModels()
    {
        lock (Sync)
            return Profiles().ToDictionary(item => item.Key, item => item.Value.Model,
                StringComparer.OrdinalIgnoreCase);
    }

    internal static bool TryGet(string deviceId, out ControllerVisualProfile profile)
    {
        lock (Sync) return Profiles().TryGetValue(deviceId, out profile!);
    }

    internal static void Set(string deviceId, ControllerVisualModel model, string displayName)
    {
        lock (Sync)
        {
            Profiles()[deviceId] = new ControllerVisualProfile(model, displayName);
            Save();
        }
    }

    internal static void Remove(string deviceId)
    {
        lock (Sync)
        {
            if (!Profiles().Remove(deviceId)) return;
            Save();
        }
    }

    private static Dictionary<string, ControllerVisualProfile> Profiles() =>
        _profiles ??= Load();

    private static Dictionary<string, ControllerVisualProfile> Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return new(StringComparer.OrdinalIgnoreCase);
            return JsonSerializer.Deserialize<Dictionary<string, ControllerVisualProfile>>(
                    File.ReadAllText(FilePath))
                is { } profiles
                ? new Dictionary<string, ControllerVisualProfile>(profiles,
                    StringComparer.OrdinalIgnoreCase)
                : new(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static void Save()
    {
        var directory = Path.GetDirectoryName(FilePath)!;
        Directory.CreateDirectory(directory);
        var temporary = FilePath + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(_profiles,
            new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporary, FilePath, true);
    }
}
