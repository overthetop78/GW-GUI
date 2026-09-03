using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace GWGUI.Emulation.Functions;

public static partial class JsonConfigurationRecoveryFunctions
{
    public static T DeserializeRemovingInvalidProperties<T>(string json,
        Func<JsonElement, T> deserialize, out string repairedJson)
    {
        var root = JsonNode.Parse(json) ?? throw new JsonException("The JSON document is empty.");
        var changed = false;
        for (var attempt = 0; attempt < 32; attempt++)
        {
            var candidate = changed
                ? root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) : json;
            try
            {
                using var document = JsonDocument.Parse(candidate);
                var value = deserialize(document.RootElement);
                repairedJson = candidate;
                return value;
            }
            catch (JsonException exception) when (RemoveInvalidProperty(root, exception.Path))
            {
                changed = true;
            }
        }
        repairedJson = json;
        throw new JsonException("Too many incompatible JSON properties.");
    }

    public static async Task WriteAtomicallyAsync(string path, string json,
        CancellationToken cancellationToken)
    {
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await File.WriteAllTextAsync(temporary, json, cancellationToken).ConfigureAwait(false);
            ConfigurationFileAccessFunctions.ReplaceFile(temporary, path);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    private static bool RemoveInvalidProperty(JsonNode root, string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || path == "$") return false;
        var matches = PathPart().Matches(path);
        if (matches.Count == 0) return false;
        JsonNode? parent = root;
        for (var index = 0; index < matches.Count - 1 && parent is not null; index++)
            parent = Child(parent, matches[index]);
        if (parent is null) return false;
        var last = matches[^1];
        if (last.Groups[1].Success && parent is JsonObject objectParent)
        {
            var actualName = objectParent.Select(property => property.Key).FirstOrDefault(name =>
                string.Equals(name, last.Groups[1].Value, StringComparison.OrdinalIgnoreCase));
            return actualName is not null && objectParent.Remove(actualName);
        }
        if (last.Groups[2].Success && parent is JsonArray arrayParent
            && int.TryParse(last.Groups[2].Value, out var arrayIndex)
            && arrayIndex >= 0 && arrayIndex < arrayParent.Count)
        {
            arrayParent.RemoveAt(arrayIndex);
            return true;
        }
        return false;
    }

    private static JsonNode? Child(JsonNode parent, Match match)
    {
        if (match.Groups[1].Success && parent is JsonObject objectParent)
        {
            var name = objectParent.Select(property => property.Key).FirstOrDefault(candidate =>
                string.Equals(candidate, match.Groups[1].Value, StringComparison.OrdinalIgnoreCase));
            return name is null ? null : objectParent[name];
        }
        return match.Groups[2].Success && parent is JsonArray arrayParent
            && int.TryParse(match.Groups[2].Value, out var index)
            && index >= 0 && index < arrayParent.Count ? arrayParent[index] : null;
    }

    [GeneratedRegex(@"\.([^\.\[\]]+)|\[(\d+)\]")]
    private static partial Regex PathPart();
}
