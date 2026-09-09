using System.IO;
using System.Text.Json;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;

namespace GWGUI.App.Services.Emulation;

internal static class EmulationModuleManifestReader
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    internal static EmulationModuleManifest Read(string directory)
    {
        EnsureRegularPath(directory, isDirectory: true);
        var manifestPath = Path.Combine(directory, EmulationHostApi.ManifestFileName);
        EnsureRegularPath(manifestPath, isDirectory: false);
        var manifest = Parse(File.ReadAllText(manifestPath));
        EnsureRegularPath(Path.Combine(directory, manifest.EntryAssembly), isDirectory: false);
        return manifest;
    }

    internal static EmulationModuleManifest Parse(string json, Version? hostApiVersion = null)
    {
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("The emulation module manifest must be a JSON object.");
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in document.RootElement.EnumerateObject())
            if (!names.Add(property.Name))
                throw new InvalidDataException($"Duplicate manifest field '{property.Name}'.");
        var manifest = document.RootElement.Deserialize<EmulationModuleManifest>(JsonOptions)
            ?? throw new InvalidDataException("The emulation module manifest is empty.");
        if (manifest.SchemaVersion != EmulationHostApi.ManifestSchemaVersion)
            throw new InvalidDataException($"Unsupported module manifest schema '{manifest.SchemaVersion}'.");
        ValidateFileName(manifest.Id, "id");
        ValidateFileName(manifest.EntryAssembly, "entryAssembly");
        if (!string.Equals(Path.GetExtension(manifest.EntryAssembly), ".dll", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("The module entryAssembly must name a DLL.");
        _ = ParseVersion(manifest.ModuleVersion, 3, "moduleVersion");
        var minimum = ParseVersion(manifest.HostApiMinimum, 2, "hostApiMinimum");
        var maximum = ParseVersion(manifest.HostApiMaximum, 2, "hostApiMaximum");
        if (minimum > maximum)
            throw new InvalidDataException($"Module '{manifest.Id}' has reversed host API bounds: {minimum} to {maximum}.");
        if (string.IsNullOrWhiteSpace(manifest.UpdateCatalogUrl)
            || manifest.UpdateCatalogUrl != manifest.UpdateCatalogUrl.Trim()
            || !Uri.TryCreate(manifest.UpdateCatalogUrl, UriKind.Absolute, out var updateCatalogUri)
            || updateCatalogUri.Scheme != Uri.UriSchemeHttps)
            throw new InvalidDataException("Manifest field 'updateCatalogUrl' must contain an absolute HTTPS URL.");
        var current = hostApiVersion ?? EmulationHostApi.CurrentVersion;
        if (current < minimum || current > maximum)
            throw new InvalidDataException($"Module '{manifest.Id}' version {manifest.ModuleVersion} requires host API " +
                $"{minimum} to {maximum}; available API is {current}.");
        return manifest;
    }

    private static Version ParseVersion(string? value, int components, string field)
    {
        if (string.IsNullOrEmpty(value)
            || value.Split('.').Length != components
            || value.Split('.').Any(part => part.Length == 0 || !part.All(char.IsAsciiDigit))
            || !Version.TryParse(value, out var version))
            throw new InvalidDataException($"Manifest field '{field}' must contain {components} numeric version components; received '{value}'.");
        return version;
    }

    private static void ValidateFileName(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value) || value != value.Trim()
            || value.EndsWith('.') || value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || value.Contains('/') || value.Contains('\\') || Path.IsPathRooted(value))
            throw new InvalidDataException($"Invalid module manifest field '{field}': '{value}'.");
        var device = value.Split('.')[0].ToUpperInvariant();
        if (device is "CON" or "PRN" or "AUX" or "NUL"
            || device.Length == 4 && (device.StartsWith("COM", StringComparison.Ordinal)
                || device.StartsWith("LPT", StringComparison.Ordinal)) && device[3] is >= '1' and <= '9')
            throw new InvalidDataException($"Reserved Windows name in manifest field '{field}': '{value}'.");
    }

    private static void EnsureRegularPath(string path, bool isDirectory)
    {
        var attributes = File.GetAttributes(path);
        if ((attributes & FileAttributes.ReparsePoint) != 0
            || ((attributes & FileAttributes.Directory) != 0) != isDirectory)
            throw new InvalidDataException($"Module path must be a regular {(isDirectory ? "directory" : "file")}: '{path}'.");
    }
}
