namespace GWGUI.MediaEngine.Encoding.Apple;

/// <summary>Construit les erreurs d'encodage des pistes Apple II GCR standard.</summary>
internal static class AppleIITrackEncodingExceptions
{
    /// <summary>Crée l'erreur signalant un format source non représentable en NIB ou WOZ.</summary>
    public static InvalidDataException UnsupportedSource(string formatId) => new($"Le format source '{formatId}' n'est pas une image Apple II GCR 5,25 pouces représentable.");
    /// <summary>Crée l'erreur signalant une géométrie incompatible.</summary>
    public static InvalidDataException InvalidGeometry(int cylinders, int heads, int sectorsPerTrack) => new($"La géométrie Apple II {cylinders} cylindre(s), {heads} face(s) et {sectorsPerTrack} secteur(s) par piste n'est pas représentable.");
    /// <summary>Crée l'erreur signalant un secteur absent ou d'une taille incorrecte.</summary>
    public static InvalidDataException InvalidSector(int cylinder, int sector, int observedSize, int expectedSize) => new($"Le secteur Apple II {cylinder}:{sector} contient {observedSize} octets au lieu de {expectedSize}.");
    /// <summary>Crée l'erreur signalant une piste trop longue pour le conteneur cible.</summary>
    public static InvalidDataException TrackTooLong(int cylinder, int observedBits, int maximumBits) => new($"La piste Apple II {cylinder} contient {observedBits} bits et dépasse la limite de {maximumBits} bits.");
}
