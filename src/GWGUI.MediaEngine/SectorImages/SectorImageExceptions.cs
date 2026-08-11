namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Construit les exceptions décrivant les incohérences du modèle d'image sectorielle.</summary>
internal static class SectorImageExceptions
{
    /// <summary>Crée l'erreur signalant une dimension nulle ou négative.</summary>
    /// <param name="parameterName">Nom du paramètre contenant la dimension.</param>
    /// <param name="value">Valeur invalide reçue.</param>
    /// <param name="expectedValue">Description de la valeur attendue.</param>
    /// <returns>Exception associée à la dimension invalide.</returns>
    public static ArgumentOutOfRangeException InvalidDimension(string parameterName, object? value, object expectedValue) => new(parameterName, value, $"Valeur attendue : {expectedValue}.");

    /// <summary>Crée l'erreur signalant un identifiant de format nul, vide ou blanc.</summary>
    /// <param name="parameterName">Nom du paramètre contenant l'identifiant.</param>
    /// <param name="value">Identifiant invalide reçu.</param>
    /// <returns>Exception associée à l'identifiant invalide.</returns>
    public static ArgumentException InvalidFormatId(string parameterName, string? value) => new($"L'identifiant de format '{value}' doit contenir au moins un caractère non blanc.", parameterName);

    /// <summary>Crée l'erreur signalant l'absence d'un bloc logique.</summary>
    /// <param name="logicalBlock">Numéro du bloc logique absent.</param>
    /// <returns>Exception décrivant le bloc absent.</returns>
    public static InvalidDataException MissingBlock(int logicalBlock) => new($"Le bloc logique {logicalBlock} est absent.");

    /// <summary>Crée l'erreur signalant une taille de bloc différente de celle attendue.</summary>
    /// <param name="logicalBlock">Numéro du bloc logique concerné.</param>
    /// <param name="observedSize">Taille observée, en octets.</param>
    /// <param name="expectedSize">Taille attendue, en octets.</param>
    /// <returns>Exception décrivant l'écart de taille.</returns>
    public static InvalidDataException InvalidBlockSize(int logicalBlock, int observedSize, int expectedSize) => InvalidPropertyValue(nameof(SectorBlock.Data), observedSize, expectedSize, logicalBlock);

    /// <summary>Crée l'erreur décrivant une valeur de propriété incompatible avec l'invariant attendu.</summary>
    /// <param name="propertyName">Nom de la propriété concernée.</param>
    /// <param name="observedValue">Valeur observée.</param>
    /// <param name="expectedValue">Valeur ou règle attendue.</param>
    /// <param name="logicalBlock">Numéro du bloc logique concerné, lorsqu'il existe.</param>
    /// <returns>Exception contenant toutes les valeurs de diagnostic.</returns>
    public static InvalidDataException InvalidPropertyValue(string propertyName, object? observedValue, object? expectedValue, int? logicalBlock = null)
    {
        var block = logicalBlock is null ? string.Empty : $" du bloc logique {logicalBlock}";
        return new($"La propriété {propertyName}{block} vaut '{observedValue}' ; valeur attendue : '{expectedValue}'.");
    }
}
