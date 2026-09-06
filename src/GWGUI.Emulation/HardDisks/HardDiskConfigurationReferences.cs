using System.Text.Json;

namespace GWGUI.Emulation.HardDisks;

public static class HardDiskConfigurationReferences
{
    // Strict read: malformed or unreadable configurations must block deletion,
    // rather than being silently skipped by the normal recovery loader.
    public static bool ContainsPath(string json, string path, string pathBase)
    {
        using var document = JsonDocument.Parse(json);
        return Contains(document.RootElement, path, pathBase);
    }

    private static bool Contains(JsonElement element, string path, string pathBase)
    {
        if (element.ValueKind == JsonValueKind.Object)
            return element.EnumerateObject().Any(property => Contains(property.Value, path, pathBase));
        if (element.ValueKind == JsonValueKind.Array)
            return element.EnumerateArray().Any(item => Contains(item, path, pathBase));
        if (element.ValueKind != JsonValueKind.String) return false;
        var value = element.GetString();
        if (string.IsNullOrWhiteSpace(value)) return false;
        try { return HardDiskPath.Equals(Path.GetFullPath(value, pathBase), path); }
        catch (ArgumentException) { return false; }
    }
}
