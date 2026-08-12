using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Images;

namespace GWGUI.MediaEngine.Exploration.Interpretation;

/// <summary>Construit une identité stable pour dédupliquer les interprétations de systèmes de fichiers.</summary>
internal static class FileSystemInterpretationIdentity
{
    /// <summary>Séparateur entre deux champs d'identité.</summary>
    public const char FieldSeparator = '\0';
    /// <summary>Séparateur entre deux entrées sérialisées.</summary>
    public const char EntrySeparator = '\u001f';
    /// <summary>Séparateur entre deux segments d'un chemin d'entrée.</summary>
    public const char PathSeparator = '/';

    /// <summary>Construit l'identité d'une interprétation depuis sa famille de format et son volume.</summary>
    /// <param name="interpretation">Interprétation à identifier.</param>
    /// <returns>Identité stable de la famille et du contenu logique.</returns>
    public static string Create(ExploredFileSystem interpretation) => $"{FormatFamily(interpretation.FormatId)}{FieldSeparator}{CreateVolume(interpretation.Volume)}";

    /// <summary>Construit l'identité d'un volume depuis son nom et ses entrées triées.</summary>
    /// <param name="volume">Volume à identifier.</param>
    /// <returns>Identité stable du volume.</returns>
    public static string CreateVolume(FileSystemVolume volume) => $"{volume.Name}{FieldSeparator}{string.Join(EntrySeparator, Entries(volume.Entries))}";

    /// <summary>Énumère récursivement les entrées dans un ordre insensible à la casse.</summary>
    /// <param name="entries">Entrées du niveau courant.</param>
    /// <param name="prefix">Chemin technique du niveau courant.</param>
    /// <returns>Segments d'identité ordonnés.</returns>
    private static IEnumerable<string> Entries(IEnumerable<FileSystemEntry> entries, string prefix = "")
    {
        foreach (var entry in entries.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase))
        {
            var path = prefix + entry.Name;
            yield return $"{path}{FieldSeparator}{entry.Kind}{FieldSeparator}{entry.Size}";
            foreach (var child in Entries(entry.Children, path + PathSeparator)) yield return child;
        }
    }

    /// <summary>Extrait la famille précédant le premier point d'un identifiant de format.</summary>
    /// <param name="formatId">Identifiant de format complet.</param>
    /// <returns>Préfixe familial ou identifiant complet sans point.</returns>
    private static string FormatFamily(string formatId)
    {
        var separator = formatId.IndexOf('.');
        return separator < 0 ? formatId : formatId[..separator];
    }
}
