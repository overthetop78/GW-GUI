using System.IO;
using GWGUI.Updates.Contracts;

namespace GWGUI.Updates.Services;

public sealed class ModuleDirectoryCatalogValidator
{
    public const int SupportedSchemaVersion = 1;

    public IReadOnlyList<ModuleDirectoryEntry> Validate(ModuleDirectoryCatalog directory)
    {
        if (directory.SchemaVersion != SupportedSchemaVersion)
            throw new InvalidDataException($"Unsupported module directory schema '{directory.SchemaVersion}'.");
        if (directory.Modules is null)
            throw new InvalidDataException("The module directory does not contain a modules collection.");

        var identifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var catalogUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var module in directory.Modules)
        {
            if (string.IsNullOrWhiteSpace(module.Id)
                || module.Id != module.Id.Trim()
                || !module.Id.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_'))
                throw new InvalidDataException("A module directory entry has an invalid id.");
            if (!identifiers.Add(module.Id))
                throw new InvalidDataException($"Duplicate module directory id '{module.Id}'.");
            if (string.IsNullOrWhiteSpace(module.DisplayName) || module.DisplayName != module.DisplayName.Trim())
                throw new InvalidDataException($"Module directory entry '{module.Id}' has an invalid display name.");
            if (!Uri.TryCreate(module.CatalogUrl, UriKind.Absolute, out var catalogUri)
                || catalogUri.Scheme != Uri.UriSchemeHttps
                || !catalogUri.AbsolutePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(
                    $"Module directory entry '{module.Id}' must contain a direct HTTPS catalog URL ending in .json.");
            if (!catalogUrls.Add(catalogUri.AbsoluteUri))
                throw new InvalidDataException($"Duplicate module directory catalog URL '{module.CatalogUrl}'.");
        }

        return directory.Modules.OrderBy(module => module.DisplayName, StringComparer.CurrentCultureIgnoreCase).ToArray();
    }
}
