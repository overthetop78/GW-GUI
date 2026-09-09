using System.IO.Compression;
using System.Text.Json;
using GWGUI.Updates.Contracts;

namespace GWGUI.Updates.Services;

public sealed class UpdateArchiveValidator
{
    public string ExtractValidated(string archivePath, string destinationDirectory,
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
            else ValidateModule(root, componentId, expectedVersion);
            return root;
        }
        catch
        {
            if (Directory.Exists(destination)) Directory.Delete(destination, recursive: true);
            throw;
        }
    }

    private static void ValidateModule(string root, string componentId, string expectedVersion)
    {
        var manifestPath = Path.Combine(root, "module.json");
        if (!File.Exists(manifestPath)) throw new InvalidDataException("The module archive does not contain module.json.");
        using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        var manifest = document.RootElement;
        if (!manifest.TryGetProperty("id", out var id)
            || !string.Equals(id.GetString(), componentId, StringComparison.OrdinalIgnoreCase)
            || !manifest.TryGetProperty("moduleVersion", out var version)
            || !string.Equals(version.GetString(), expectedVersion, StringComparison.Ordinal))
            throw new InvalidDataException("The module archive identity or version does not match the update plan.");
    }

    private static bool IsLink(ZipArchiveEntry entry)
    {
        var unixType = (entry.ExternalAttributes >> 16) & 0xF000;
        var windowsAttributes = (FileAttributes)(entry.ExternalAttributes & 0xFFFF);
        return unixType == 0xA000 || (windowsAttributes & FileAttributes.ReparsePoint) != 0;
    }
}
