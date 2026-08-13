using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Exploration.Results;

namespace GWGUI.MediaEngine.Exploration.Interpretation;

/// <summary>Construit une identitÃ© stable pour dÃ©dupliquer les interprÃ©tations de systÃ¨mes de fichiers.</summary>
internal static class FileSystemInterpretationIdentity
{
    /// <summary>SÃ©parateur entre deux champs d'identitÃ©.</summary>
    public const char FieldSeparator = '\0';
    /// <summary>SÃ©parateur entre deux entrÃ©es sÃ©rialisÃ©es.</summary>
    public const char EntrySeparator = '\u001f';
    /// <summary>SÃ©parateur entre deux segments d'un chemin d'entrÃ©e.</summary>
    public const char PathSeparator = '/';

    /// <summary>Construit l'identitÃ© d'une interprÃ©tation depuis sa famille de format et son volume.</summary>
    /// <param name="interpretation">InterprÃ©tation Ã  identifier.</param>
    /// <returns>IdentitÃ© stable de la famille et du contenu logique.</returns>
    public static string Create(ExploredFileSystem interpretation) => $"{FormatFamily(interpretation.FormatId)}{FieldSeparator}{CreateVolume(interpretation.Volume)}";

    /// <summary>Construit l'identitÃ© d'un volume depuis son nom et ses entrÃ©es triÃ©es.</summary>
    /// <param name="volume">Volume Ã  identifier.</param>
    /// <returns>IdentitÃ© stable du volume.</returns>
    public static string CreateVolume(FileSystemVolume volume) => $"{volume.Name}{FieldSeparator}{string.Join(EntrySeparator, Entries(volume.Entries))}";

    /// <summary>Ã‰numÃ¨re rÃ©cursivement les entrÃ©es dans un ordre insensible Ã  la casse.</summary>
    /// <param name="entries">EntrÃ©es du niveau courant.</param>
    /// <param name="prefix">Chemin technique du niveau courant.</param>
    /// <returns>Segments d'identitÃ© ordonnÃ©s.</returns>
    private static IEnumerable<string> Entries(IEnumerable<FileSystemEntry> entries, string prefix = "")
    {
        foreach (var entry in entries.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase))
        {
            var path = prefix + entry.Name;
            yield return $"{path}{FieldSeparator}{entry.Kind}{FieldSeparator}{entry.Size}";
            foreach (var child in Entries(entry.Children, path + PathSeparator)) yield return child;
        }
    }

    /// <summary>Extrait la famille prÃ©cÃ©dant le premier point d'un identifiant de format.</summary>
    /// <param name="formatId">Identifiant de format complet.</param>
    /// <returns>PrÃ©fixe familial ou identifiant complet sans point.</returns>
    internal static string FormatFamily(string formatId)
    {
        var separator = formatId.IndexOf('.');
        return separator < 0 ? formatId : formatId[..separator];
    }
}
