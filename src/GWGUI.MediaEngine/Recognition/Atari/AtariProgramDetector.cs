using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.MediaEngine.Recognition.Atari;

/// <summary>Recherche récursivement un programme Atari ST par extension ou signature.</summary>
internal static class AtariProgramDetector
{
    /// <summary>Indique si une entrée ou l'un de ses descendants est un programme Atari ST.</summary>
    public static bool ContainsProgram(IEnumerable<FileSystemEntry> entries)
    {
        foreach (var entry in entries)
        {
            if (entry.Kind == FileSystemEntryKind.File && (AtariProgramDefinitions.Extensions.Contains(Path.GetExtension(entry.Name)) || entry.Content is not null && entry.Content.Count >= AtariProgramDefinitions.Signature.Length && entry.Content.Take(AtariProgramDefinitions.Signature.Length).SequenceEqual(AtariProgramDefinitions.Signature.ToArray()))) return true;
            if (ContainsProgram(entry.Children)) return true;
        }
        return false;
    }
}
