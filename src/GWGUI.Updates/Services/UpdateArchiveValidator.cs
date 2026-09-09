using System.IO.Compression;
using System.Text.Json;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Updates.Contracts;

namespace GWGUI.Updates.Services;

public sealed class UpdateArchiveValidator
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public ValidatedUpdateArchive ExtractValidated(string archivePath, string destinationDirectory,
        string componentId, UpdateComponentKind kind, string expectedVersion)
    {
        var archive = Path.GetFullPath(archivePath);
        var destination = Path.GetFullPath(destinationDirectory);
        if (!File.Exists(archive)) throw new FileNotFoundException("Update archive was not found.", archive);
        if (Directory.Exists(destination)) Directory.Delete(destination, recursive: true);
        Directory.CreateDirectory(destination);

        var expectedRoot = kind == UpdateComponentKind.Application
            ? "GW GUI" : $"Modules/{componentId}";
        var files = 0;
        var targets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            using var zip = ZipFile.OpenRead(archive);
            foreach (var entry in zip.Entries)
            {
                var name = entry.FullName.Replace('\\', '/').TrimEnd('/');
                if (string.IsNullOrEmpty(name)) continue;
                if (name.StartsWith('/') || name.Contains(':')
                    || name.Split('/').Any(part => part is "" or "." or ".."))
                    throw new InvalidDataException($"Unsafe archive path '{entry.FullName}'.");
                if (!name.Equals(expectedRoot, StringComparison.OrdinalIgnoreCase)
                    && !name.StartsWith(expectedRoot + "/", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Archive entry '{entry.FullName}' is outside '{expectedRoot}'.");
                if (IsLink(entry)) throw new InvalidDataException($"Archive link '{entry.FullName}' is not allowed.");

                var relative = name.Replace('/', Path.DirectorySeparatorChar);
                var target = Path.GetFullPath(Path.Combine(destination, relative));
                if (!target.StartsWith(destination + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Archive entry '{entry.FullName}' escapes its destination.");
                if (entry.FullName.EndsWith('/') || entry.FullName.EndsWith('\\'))
                {
                    Directory.CreateDirectory(target);
                    continue;
                }
                if (!targets.Add(target)) throw new InvalidDataException($"Duplicate archive entry '{entry.FullName}'.");
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                entry.ExtractToFile(target, overwrite: false);
                files++;
            }
            if (files == 0) throw new InvalidDataException("The update archive contains no files.");

            var root = Path.Combine(destination, expectedRoot.Replace('/', Path.DirectorySeparatorChar));
            if (kind == UpdateComponentKind.Application)
            {
                if (!File.Exists(Path.Combine(root, "gwgui.exe")))
                    throw new InvalidDataException("The application archive does not contain GW GUI/gwgui.exe.");
            }
            EmulationModuleManifest? manifest = null;
            if (kind == UpdateComponentKind.Module)
                manifest = ValidateModule(root, componentId, expectedVersion);
            return new(root, manifest);
        }
        catch
        {
            if (Directory.Exists(destination)) Directory.Delete(destination, recursive: true);
            throw;
        }
    }

    public ValidatedUpdateArchive ExtractModuleValidated(string archivePath, string destinationDirectory)
    {
        using var zip = ZipFile.OpenRead(Path.GetFullPath(archivePath));
        var manifests = zip.Entries
            .Where(entry => entry.FullName.Replace('\\', '/').Split('/') is ["Modules", _, "module.json"])
            .ToArray();
        if (manifests.Length != 1)
            throw new InvalidDataException("The module archive must contain exactly one Modules/<id>/module.json.");
        EmulationModuleManifest manifest;
        using (var content = manifests[0].Open())
            manifest = JsonSerializer.Deserialize<EmulationModuleManifest>(content, JsonOptions)
                ?? throw new InvalidDataException("The module manifest is empty.");
        return ExtractValidated(archivePath, destinationDirectory, manifest.Id,
            UpdateComponentKind.Module, manifest.ModuleVersion);
    }

    private static EmulationModuleManifest ValidateModule(string root, string componentId, string expectedVersion)
    {
        var manifestPath = Path.Combine(root, "module.json");
        if (!File.Exists(manifestPath)) throw new InvalidDataException("The module archive does not contain module.json.");
        var manifest = JsonSerializer.Deserialize<EmulationModuleManifest>(File.ReadAllText(manifestPath), JsonOptions)
            ?? throw new InvalidDataException("The module manifest is empty.");
        if (!string.Equals(manifest.Id, componentId, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(manifest.ModuleVersion, expectedVersion, StringComparison.Ordinal))
            throw new InvalidDataException("The module archive identity or version does not match the update plan.");
        ValidateManifest(manifest, root);
        return manifest;
    }

    private static void ValidateManifest(EmulationModuleManifest manifest, string root)
    {
        if (manifest.SchemaVersion != EmulationHostApi.ManifestSchemaVersion)
            throw new InvalidDataException($"Unsupported module manifest schema '{manifest.SchemaVersion}'.");
        if (string.IsNullOrWhiteSpace(manifest.Id)
            || !manifest.Id.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_'))
            throw new InvalidDataException("The module manifest id is invalid.");
        if (string.IsNullOrWhiteSpace(manifest.EntryAssembly)
            || Path.GetFileName(manifest.EntryAssembly) != manifest.EntryAssembly
            || !manifest.EntryAssembly.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("The module manifest entry assembly is invalid.");
        if (!Version.TryParse(manifest.ModuleVersion, out var moduleVersion) || moduleVersion.Build < 0)
            throw new InvalidDataException("The module manifest version is invalid.");
        if (!Version.TryParse(manifest.HostApiMinimum, out var minimum) || minimum.Build >= 0
            || !Version.TryParse(manifest.HostApiMaximum, out var maximum) || maximum.Build >= 0
            || minimum > maximum)
            throw new InvalidDataException("The module manifest host API range is invalid.");
        if (!Uri.TryCreate(manifest.UpdateCatalogUrl, UriKind.Absolute, out var catalogUri)
            || catalogUri.Scheme != Uri.UriSchemeHttps)
            throw new InvalidDataException("The module manifest update catalog URL must be absolute HTTPS.");
        if (!File.Exists(Path.Combine(root, manifest.EntryAssembly)))
            throw new InvalidDataException("The module manifest entry assembly is missing from the archive.");
    }

    private static bool IsLink(ZipArchiveEntry entry)
    {
        var unixType = (entry.ExternalAttributes >> 16) & 0xF000;
        var windowsAttributes = (FileAttributes)(entry.ExternalAttributes & 0xFFFF);
        return unixType == 0xA000 || (windowsAttributes & FileAttributes.ReparsePoint) != 0;
    }
}

public sealed record ValidatedUpdateArchive(
    string RootDirectory,
    EmulationModuleManifest? ModuleManifest);
