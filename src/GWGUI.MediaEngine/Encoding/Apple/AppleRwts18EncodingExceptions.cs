namespace GWGUI.MediaEngine.Encoding.Apple;

/// <summary>Construit les erreurs d'encodage d'une image RWTS18.</summary>
internal static class AppleRwts18EncodingExceptions
{
    /// <summary>Crée l'erreur signalant un format source incompatible.</summary>
    public static InvalidDataException UnsupportedSource(string formatId) => new($"Le format source '{formatId}' n'est pas une image Apple II RWTS18.");
    /// <summary>Crée l'erreur signalant un secteur absent ou d'une taille incorrecte.</summary>
    public static InvalidDataException InvalidSector(int cylinder, int sector, int observedSize, int expectedSize) => new($"Le secteur RWTS18 {cylinder}:{sector} contient {observedSize} octets au lieu de {expectedSize}.");
    /// <summary>Crée l'erreur signalant une piste trop longue pour le conteneur cible.</summary>
    public static InvalidDataException TrackTooLong(int cylinder, int observedBits, int maximumBits) => new($"La piste RWTS18 {cylinder} contient {observedBits} bits et dépasse la limite de {maximumBits} bits.");
}
