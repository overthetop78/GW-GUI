using System.IO;
using GWGUI.Emulation.HardDisks;

namespace GWGUI.App.Services.Emulation;

/// <summary>Builds the validated list of files owned by one hard disk image entry point.</summary>
internal static class HardDiskImageSetResolver
{
    internal static IReadOnlyList<string> Resolve(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var bundle = FindSparseBundle(fullPath);
        if (bundle is not null) return ResolveSparseBundle(bundle);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("The disk image does not exist.", fullPath);
        if (!Path.GetExtension(fullPath).Equals(".vmdk", StringComparison.OrdinalIgnoreCase)) return [fullPath];
        using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);
        var members = DiskImageDependencyReader.ReadMembers(stream, fullPath);
        if (members.Count == 0) return [fullPath];
        var directory = Path.GetDirectoryName(fullPath)!;
        foreach (var member in members)
        {
            if (!string.Equals(Path.GetDirectoryName(member), directory, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("A VMDK set member is outside the descriptor directory.");
            if (!File.Exists(member)) throw new InvalidDataException("A VMDK set member is missing.");
        }
        return new[] { fullPath }.Concat(members).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static string? FindSparseBundle(string path)
    {
        var entry = Directory.Exists(path) ? new DirectoryInfo(path) : new FileInfo(path).Directory;
        while (entry is not null)
        {
            if (entry.Extension.Equals(".sparsebundle", StringComparison.OrdinalIgnoreCase)) return entry.FullName;
            entry = entry.Parent;
        }
        return null;
    }

    private static IReadOnlyList<string> ResolveSparseBundle(string root)
    {
        RejectRedirect(root);
        var bands = Path.Combine(root, "bands");
        if (!Directory.Exists(bands)) throw new InvalidDataException("The sparsebundle bands directory is missing.");
        RejectRedirect(bands);
        var required = new[] { "Info.plist", "Info.bckup", "token" }
            .Select(name => Path.Combine(root, name)).ToArray();
        if (required.Any(path => !File.Exists(path)) || !File.Exists(Path.Combine(bands, "0")))
            throw new InvalidDataException("The sparsebundle image set is incomplete.");
        var files = new List<string>(required);
        foreach (var path in Directory.EnumerateFiles(bands, "*", SearchOption.TopDirectoryOnly))
        {
            RejectRedirect(path);
            if (!long.TryParse(Path.GetFileName(path), System.Globalization.NumberStyles.HexNumber,
                    System.Globalization.CultureInfo.InvariantCulture, out _))
                throw new InvalidDataException("The sparsebundle contains an unexpected band name.");
            files.Add(Path.GetFullPath(path));
        }
        var allowedFiles = files.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (Directory.EnumerateFiles(root, "*", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFullPath).Any(path => !allowedFiles.Contains(path)))
            throw new InvalidDataException("The sparsebundle contains an unexpected file.");
        if (Directory.EnumerateDirectories(root, "*", SearchOption.TopDirectoryOnly)
            .Any(path => !string.Equals(Path.GetFullPath(path), bands, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidDataException("The sparsebundle contains an unexpected directory.");
        return files.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static void RejectRedirect(string path)
    {
        if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
            throw new IOException("A redirected image set member cannot be deleted.");
    }
}
