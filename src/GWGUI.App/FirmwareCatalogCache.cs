using System.IO;
using System.Text.Json;

namespace GWGUI.App;

internal static class FirmwareCatalogCache
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    internal static void Write<T>(string platform, IReadOnlyList<T> entries)
    {
        Directory.CreateDirectory(StoragePaths.FirmwareCatalogDirectory);
        var path = StoragePaths.FirmwareCatalogPath(platform);
        var temporaryPath = path + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(entries, JsonOptions));
        File.Move(temporaryPath, path, true);
    }

    internal static IReadOnlyList<T> Read<T>(string platform)
    {
        var path = StoragePaths.FirmwareCatalogPath(platform);
        if (!File.Exists(path)) return [];
        try
        {
            return JsonSerializer.Deserialize<T[]>(File.ReadAllText(path), JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
