namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Construit les exceptions décrivant les incohérences du modèle d'image sectorielle.</summary>
internal static class SectorImageExceptions
{
    /// <summary>Crée l'erreur signalant une dimension nulle ou négative.</summary>
    /// <param name="parameterName">Nom du paramètre contenant la dimension.</param>
    /// <param name="value">Valeur invalide reçue.</param>
    /// <returns>Exception associée à la dimension invalide.</returns>
    public static ArgumentOutOfRangeException InvalidDimension(string parameterName, int? value) => new(parameterName, value, "La dimension doit être strictement positive.");

    /// <summary>Crée l'erreur signalant l'absence d'un bloc logique.</summary>
    /// <param name="logicalBlock">Numéro du bloc logique absent.</param>
    /// <returns>Exception décrivant le bloc absent.</returns>
    public static InvalidDataException MissingBlock(int logicalBlock) => new($"Le bloc logique {logicalBlock} est absent.");

    /// <summary>Crée l'erreur signalant une taille de bloc différente de celle attendue.</summary>
    /// <param name="logicalBlock">Numéro du bloc logique concerné.</param>
    /// <param name="observedSize">Taille observée, en octets.</param>
    /// <param name="expectedSize">Taille attendue, en octets.</param>
    /// <returns>Exception décrivant l'écart de taille.</returns>
    public static InvalidDataException InvalidBlockSize(int logicalBlock, int observedSize, int expectedSize) => new($"Le bloc logique {logicalBlock} contient {observedSize} octets au lieu des {expectedSize} octets attendus.");
}
