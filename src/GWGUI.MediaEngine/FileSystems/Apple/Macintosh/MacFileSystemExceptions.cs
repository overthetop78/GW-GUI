namespace GWGUI.MediaEngine.FileSystems.Apple.Macintosh;

/// <summary>Construit les erreurs et avertissements communs aux systèmes de fichiers Macintosh.</summary>
internal static class MacFileSystemExceptions
{
    /// <summary>Crée l'erreur signalant qu'une signature de volume ne correspond pas au système attendu.</summary>
    public static InvalidDataException InvalidVolume(string system, ushort signature) => new($"L'image ne contient pas de volume {system} ; signature observée : 0x{signature:X4}.");

    /// <summary>Construit l'avertissement signalant un bloc absent dans un fork de fichier.</summary>
    public static string MissingBlock(string file, string fork, int block) => $"{file} : le bloc {block} du fork {fork} est absent.";

    /// <summary>Construit l'avertissement signalant que le contenu obtenu d'un fork est incomplet.</summary>
    public static string IncompleteData(string file, string fork, long observedLength, long expectedLength) => $"{file} : le fork {fork} contient {observedLength} octet(s) sur les {expectedLength} attendus.";

    /// <summary>Construit l'avertissement signalant un cycle dans l'arborescence des dossiers.</summary>
    public static string DirectoryCycle(uint directoryId) => $"Le dossier Macintosh {directoryId} forme un cycle dans le catalogue.";
}
