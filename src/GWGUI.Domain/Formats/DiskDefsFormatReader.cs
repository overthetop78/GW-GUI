using System.Text.RegularExpressions;

namespace GWGUI.Domain.Formats;

public static partial class DiskDefsFormatReader
{
    [GeneratedRegex(@"^\s*disk\s+([\w,.-]+)", RegexOptions.IgnoreCase)]
    private static partial Regex DiskRegex();

    [GeneratedRegex("^\\s*import\\s+([\\w,.-]*)\\s*\"([^\"]+)\"", RegexOptions.IgnoreCase)]
    private static partial Regex ImportRegex();

    public static IReadOnlySet<string> Read(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("A diskdefs file is required.", nameof(filePath));
        var formats = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        ReadFile(Path.GetFullPath(filePath), "", formats, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        return formats;
    }

    private static void ReadFile(string filePath, string prefix, HashSet<string> formats, HashSet<string> activeFiles)
    {
        if (!File.Exists(filePath)) throw new FileNotFoundException("A referenced diskdefs file was not found.", filePath);
        if (!activeFiles.Add(filePath)) throw new InvalidDataException("A diskdefs import cycle was detected.");
        try
        {
            foreach (var sourceLine in File.ReadLines(filePath))
            {
                var line = sourceLine.Split('#', 2)[0];
                var disk = DiskRegex().Match(line);
                if (disk.Success)
                {
                    formats.Add((prefix + disk.Groups[1].Value).ToLowerInvariant());
                    continue;
                }
                var import = ImportRegex().Match(line);
                if (!import.Success) continue;
                var importedPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(filePath)!, import.Groups[2].Value));
                ReadFile(importedPath, prefix + import.Groups[1].Value, formats, activeFiles);
            }
        }
        finally { activeFiles.Remove(filePath); }
    }
}
