using System.IO;
using System.Text;
using System.Text.Json;

namespace GWGUI.MediaAudit;

internal static class TemporaryMediaSignatureCatalog
{
    private const string NoExtension = "(aucune)";

    public static void Record(MediaAuditReport report, string outputDirectory, string repositoryRoot)
    {
        var outputRoot = Directory.GetParent(outputDirectory)?.Parent?.FullName
            ?? throw new InvalidOperationException("Le dossier racine de l'audit est introuvable.");
        var statePath = Path.Combine(outputRoot, "content-signature-state.json");
        var documentPath = Path.Combine(repositoryRoot, "docs", "project", "media-content-signatures.md");
        var state = ReadState(statePath);
        if (!state.ProcessedSources.Add(report.Source.Sha256)) return;

        AddObservedEdges(state, "Média", report.Source.ContentStartHex, report.Source.ContentEndHex, report.Source.Extension);
        foreach (var entry in report.Volumes.SelectMany(volume => Flatten(volume.Entries)))
        {
            if (entry.Kind == "File" && entry.ContentExtracted && !entry.SyntheticName)
            {
                AddObservedEdges(state, "Fichier", entry.ContentStartHex, entry.ContentEndHex, Extension(entry.Name));
                continue;
            }

            if (entry.Kind == "Directory" && !entry.SyntheticName && !string.IsNullOrWhiteSpace(entry.NativeTypeId))
                Add(state, "Dossier", null, null, entry.NativeTypeId, NoExtension);
        }

        WriteState(statePath, state);
        WriteDocument(documentPath, state);
    }

    private static void AddObservedEdges(SignatureState state, string element, string? start, string? end, string extension)
    {
        var usableStart = IsUsefulHex(start) ? start : null;
        var usableEnd = IsUsefulHex(end) ? end : null;
        if (usableStart is not null) Add(state, element, usableStart, null, null, extension);
        if (usableEnd is not null) Add(state, element, null, usableEnd, null, extension);
        if (usableStart is not null && usableEnd is not null)
            Add(state, element, usableStart, usableEnd, null, extension);
    }

    private static void Add(SignatureState state, string element, string? start, string? end, string? sequence, string extension)
    {
        var entry = state.Entries.FirstOrDefault(candidate =>
            candidate.Element == element && candidate.Start == start && candidate.End == end && candidate.Sequence == sequence);
        if (entry is null)
        {
            entry = new SignatureEntry { Element = element, Start = start, End = end, Sequence = sequence };
            state.Entries.Add(entry);
        }
        entry.Occurrences++;
        entry.Extensions.Add(string.IsNullOrWhiteSpace(extension) ? NoExtension : extension.ToLowerInvariant());
    }

    private static bool IsUsefulHex(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 4 || value.Length % 2 != 0) return false;
        var firstByte = value[..2];
        for (var index = 2; index < value.Length; index += 2)
            if (!value.AsSpan(index, 2).SequenceEqual(firstByte)) return true;
        return false;
    }

    private static string Extension(string name)
    {
        var extension = Path.GetExtension(name);
        return string.IsNullOrWhiteSpace(extension) ? NoExtension : extension;
    }

    private static IEnumerable<FileEntryAudit> Flatten(IEnumerable<FileEntryAudit> entries)
    {
        foreach (var entry in entries)
        {
            yield return entry;
            foreach (var child in Flatten(entry.Children)) yield return child;
        }
    }

    private static SignatureState ReadState(string path)
    {
        if (!File.Exists(path)) return new SignatureState();
        return JsonSerializer.Deserialize<SignatureState>(File.ReadAllText(path)) ?? new SignatureState();
    }

    private static void WriteState(string path, SignatureState state)
    {
        var temporaryPath = path + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(state), new UTF8Encoding(false));
        const int maximumAttempts = 20;
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                File.Move(temporaryPath, path, true);
                return;
            }
            catch (UnauthorizedAccessException) when (attempt < maximumAttempts)
            {
                Thread.Sleep(50);
            }
            catch (IOException) when (attempt < maximumAttempts)
            {
                Thread.Sleep(50);
            }
        }
    }

    private static void WriteDocument(string path, SignatureState state)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Signatures rencontrées dans les médias");
        builder.AppendLine();
        builder.AppendLine("| Élément | Signature de début | Signature de fin | Séquence reconnaissable | Extensions | Occurrences |");
        builder.AppendLine("| --- | --- | --- | --- | --- | ---: |");
        foreach (var entry in state.Entries
                     .OrderByDescending(candidate => candidate.Occurrences)
                     .ThenBy(candidate => candidate.Element, StringComparer.Ordinal)
                     .ThenBy(candidate => candidate.Start, StringComparer.Ordinal)
                     .ThenBy(candidate => candidate.End, StringComparer.Ordinal)
                     .ThenBy(candidate => candidate.Sequence, StringComparer.Ordinal))
        {
            builder.Append("| ").Append(entry.Element).Append(" | ")
                .Append(Code(entry.Start)).Append(" | ")
                .Append(Code(entry.End)).Append(" | ")
                .Append(Code(entry.Sequence)).Append(" | ")
                .Append(string.Join(", ", entry.Extensions.Order(StringComparer.OrdinalIgnoreCase))).Append(" | ")
                .Append(entry.Occurrences).AppendLine(" |");
        }
        File.WriteAllText(path, builder.ToString(), new UTF8Encoding(false));
    }

    private static string Code(string? value) => value is null ? string.Empty : $"`{value.Replace("|", "\\|").Replace("`", "\\`")}`";

    private sealed class SignatureState
    {
        public HashSet<string> ProcessedSources { get; init; } = new(StringComparer.Ordinal);
        public List<SignatureEntry> Entries { get; init; } = [];
    }

    private sealed class SignatureEntry
    {
        public string Element { get; init; } = string.Empty;
        public string? Start { get; init; }
        public string? End { get; init; }
        public string? Sequence { get; init; }
        public int Occurrences { get; set; }
        public HashSet<string> Extensions { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
