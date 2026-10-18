using GWGUI.App.Services.Storage;
using System.IO;
using System.Text.Json;

namespace GWGUI.App.Services.Emulation;

internal static class FirmwareCatalogCache
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    internal static void Write<T>(string platform, IReadOnlyList<T> entries)
    => GWGUI.Emulation.Functions.ConfigurationFileAccessFunctions.WriteDocumentAtomically(StoragePaths.FirmwareCatalogPath(platform), entries, JsonOptions, ".tmp");

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
