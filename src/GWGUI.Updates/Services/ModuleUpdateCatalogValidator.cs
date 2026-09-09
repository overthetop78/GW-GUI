using System.IO;
using GWGUI.Updates.Contracts;

namespace GWGUI.Updates.Services;

public sealed class ModuleUpdateCatalogValidator
{
    public const int SupportedSchemaVersion = 2;

    public UpdateCatalogComponent Validate(UpdateCatalog catalog)
    {
        if (catalog.Components.Count != 1)
            throw new InvalidDataException("A module update catalog must contain exactly one component.");
        return Validate(catalog, catalog.Components[0].Id);
    }

    public UpdateCatalogComponent Validate(UpdateCatalog catalog, string expectedModuleId)
    {
        if (catalog.SchemaVersion != SupportedSchemaVersion)
            throw new InvalidDataException($"Unsupported module update catalog schema '{catalog.SchemaVersion}'.");
        if (catalog.Kind != UpdateCatalogKind.Module)
            throw new InvalidDataException("The update catalog is not a module catalog.");
        if (string.IsNullOrWhiteSpace(expectedModuleId))
            throw new ArgumentException("The expected module id is required.", nameof(expectedModuleId));
        if (catalog.Components.Count != 1)
            throw new InvalidDataException("A module update catalog must contain exactly one component.");

        var component = catalog.Components[0];
        if (component.Kind != UpdateComponentKind.Module)
            throw new InvalidDataException("A module update catalog can contain only a module component.");
        if (!component.Id.Equals(expectedModuleId, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"Module update catalog id '{component.Id}' does not match installed module '{expectedModuleId}'.");
        if (component.Releases.Count == 0)
            throw new InvalidDataException($"Module update catalog '{component.Id}' contains no release.");

        var versions = new HashSet<string>(StringComparer.Ordinal);
        foreach (var release in component.Releases)
        {
            _ = ParseVersion(release.Version, 3, "module version");
            if (!versions.Add(release.Version))
                throw new InvalidDataException($"Duplicate module version '{release.Version}'.");
            ValidateHttpsUri(release.PackageUrl, "packageUrl");
            ValidateHttpsUri(release.NotesUrl, "notesUrl");
            if (release.Sha256.Length != 64 || !release.Sha256.All(char.IsAsciiHexDigit))
                throw new InvalidDataException($"Invalid SHA-256 for module version '{release.Version}'.");
            var minimum = ParseVersion(release.HostApiMinimum, 2, "hostApiMinimum");
            var maximum = ParseVersion(release.HostApiMaximum, 2, "hostApiMaximum");
            if (minimum > maximum)
                throw new InvalidDataException($"Module version '{release.Version}' has reversed host API bounds.");
            if (release.HostApiVersion is not null)
                throw new InvalidDataException("A module release cannot declare hostApiVersion.");
        }

        return component;
    }

    private static Version ParseVersion(string? value, int components, string field)
    {
        if (string.IsNullOrEmpty(value) || value.Split('.').Length != components
            || value.Split('.').Any(part => part.Length == 0 || !part.All(char.IsAsciiDigit))
            || !Version.TryParse(value, out var version))
            throw new InvalidDataException($"Catalog field '{field}' must contain {components} numeric version components.");
        return version;
    }

    private static void ValidateHttpsUri(string value, string field)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            throw new InvalidDataException($"Catalog field '{field}' must contain an absolute HTTPS URL.");
    }
}
