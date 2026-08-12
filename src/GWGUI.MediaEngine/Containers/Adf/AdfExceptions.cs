namespace GWGUI.MediaEngine.Containers.Adf;

/// <summary>Construit les erreurs propres aux conteneurs ADF.</summary>
internal static class AdfExceptions
{
    /// <summary>Crée l'erreur signalant une taille ne correspondant à aucune géométrie ADF cataloguée.</summary>
    public static InvalidDataException InvalidSize(int actualSize, IEnumerable<int> acceptedSizes) => new($"ADF image contains {actualSize} bytes; accepted sizes are {string.Join(", ", acceptedSizes)} bytes.");
}
