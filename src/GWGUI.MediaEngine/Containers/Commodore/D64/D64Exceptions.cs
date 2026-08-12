namespace GWGUI.MediaEngine.Containers.Commodore.D64;

/// <summary>Construit les erreurs propres aux conteneurs D64.</summary>
internal static class D64Exceptions
{
    /// <summary>Crée l'erreur signalant une taille absente des dispositions reconnues.</summary>
    public static InvalidDataException UnknownLength(int observedLength, IEnumerable<int> acceptedLengths) => new($"L'image D64 contient {observedLength} octet(s) ; tailles acceptées : {string.Join(", ", acceptedLengths)}.");
    /// <summary>Crée l'erreur signalant une carte d'erreurs de longueur incorrecte.</summary>
    public static InvalidDataException InvalidErrorMap(int expectedBlocks, int availableEntries) => new($"La carte d'erreurs D64 contient {availableEntries} entrée(s) pour {expectedBlocks} blocs.");
}
