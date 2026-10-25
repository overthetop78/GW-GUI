using System.IO;
using System.Text.RegularExpressions;

namespace GWGUI.MediaEngine.Images.Formats;

public static partial class DiskDefsFormatReader
{
    private static readonly Lazy<IReadOnlyDictionary<string, DiskDefinition>> Embedded = new(ReadEmbedded);

    internal static IReadOnlyDictionary<string, DiskDefinition> Definitions => Embedded.Value;

    private static readonly Lazy<IReadOnlyDictionary<string, DiskDefinition>> Raw = new(() =>
    {
        var curated = new BuiltInImageFormatCatalog().Formats.Select(format => format.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return Definitions.Where(pair => !curated.Contains(pair.Key)
            && pair.Value.Tracks.Count > 0
            && pair.Value.Tracks.All(track => track.Codec is "ibm.fm" or "ibm.mfm" or "apple2.gcr"
                or "datageneral" or "hp.mmfm" or "micropolis" or "northstar"))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
    });

    internal static IReadOnlyDictionary<string, DiskDefinition> RawDefinitions => Raw.Value;

    public static (int Cylinders, int Heads)? GetDimensions(string formatId)
    {
        var id = GwFormatArgument.FromCatalogId(formatId) ?? formatId;
        return Definitions.TryGetValue(id, out var definition) ? (definition.Cylinders, definition.Heads) : null;
    }

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

    private static IReadOnlyDictionary<string, DiskDefinition> ReadEmbedded()
    {
        var result = new Dictionary<string, DiskDefinition>(StringComparer.OrdinalIgnoreCase);
        var assembly = typeof(DiskDefsFormatReader).Assembly;
        void ReadResource(string name, string prefix)
        {
            using var stream = assembly.GetManifestResourceStream("GWGUI.MediaEngine.Images.Formats.Definitions." + name)
                ?? throw new InvalidDataException($"Missing disk definition resource '{name}'.");
            using var reader = new StreamReader(stream);
            string? id = null;
            var cylinders = 0;
            var heads = 0;
            var tracks = new List<DiskTrackDefinition>();
            Dictionary<string, string>? parameters = null;
            while (reader.ReadLine() is { } source)
            {
                var line = source.Split('#', 2)[0].Trim();
                if (line.Length == 0) continue;
                var import = ImportRegex().Match(line);
                if (import.Success)
                {
                    ReadResource(import.Groups[2].Value, prefix + import.Groups[1].Value);
                    continue;
                }
                var disk = DiskRegex().Match(line);
                if (disk.Success)
                {
                    id = prefix + disk.Groups[1].Value;
                    cylinders = heads = 0;
                    tracks = [];
                }
                else if (line.StartsWith("tracks ", StringComparison.Ordinal))
                {
                    var words = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                    parameters = new(StringComparer.OrdinalIgnoreCase);
                    tracks.Add(new(words[1], words[2], parameters));
                }
                else if (line == "end")
                {
                    if (parameters is not null) parameters = null;
                    else if (id is not null)
                    {
                        result.Add(id, new(id, cylinders, heads, tracks.ToArray()));
                        id = null;
                    }
                }
                else if (line.Split('=', 2, StringSplitOptions.TrimEntries) is { Length: 2 } pair)
                {
                    if (parameters is not null) parameters[pair[0]] = pair[1];
                    else if (pair[0] == "cyls") cylinders = int.Parse(pair[1], System.Globalization.CultureInfo.InvariantCulture);
                    else if (pair[0] == "heads") heads = int.Parse(pair[1], System.Globalization.CultureInfo.InvariantCulture);
                }
            }
        }
        ReadResource("diskdefs.cfg", "");
        return result;
    }
}
