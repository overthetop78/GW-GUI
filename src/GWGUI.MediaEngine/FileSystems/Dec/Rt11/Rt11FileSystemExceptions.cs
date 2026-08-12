namespace GWGUI.MediaEngine.FileSystems.Dec.Rt11;

/// <summary>Construit les diagnostics propres aux volumes RT-11.</summary>
public static class Rt11FileSystemExceptions
{
    /// <summary>Construit ou décode la valeur RT-11 associée à <c>InvalidHomeBlock</c>.</summary>
    public static InvalidDataException InvalidHomeBlock(string signature, int directoryBlock) => new($"Le home block RT-11 est invalide (signature '{signature}', bloc de répertoire {directoryBlock}).");
    /// <summary>Construit ou décode la valeur RT-11 associée à <c>InvalidSegment</c>.</summary>
    public static string InvalidSegment(int segment, int block, string reason) => $"Le segment RT-11 {segment} au bloc {block} est invalide : {reason}.";
    /// <summary>Construit ou décode la valeur RT-11 associée à <c>InvalidEntrySize</c>.</summary>
    public static string InvalidEntrySize(int segment, int size) => $"Le segment RT-11 {segment} annonce une taille d'entrée invalide de {size} octets.";
    /// <summary>Construit ou décode la valeur RT-11 associée à <c>EmptyName</c>.</summary>
    public static string EmptyName(int block, int offset) => $"L'entrée RT-11 au bloc de données {block}, offset {offset}, possède un nom vide.";
    /// <summary>Construit ou décode la valeur RT-11 associée à <c>MissingContent</c>.</summary>
    public static string MissingContent(string name, IEnumerable<int> blocks) => $"{name} : blocs RT-11 absents ou tronqués : {string.Join(", ", blocks)}.";
}
