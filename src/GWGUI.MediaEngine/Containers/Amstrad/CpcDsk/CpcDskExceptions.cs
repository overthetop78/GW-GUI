namespace GWGUI.MediaEngine.Containers.Amstrad.CpcDsk;

/// <summary>
/// Construit les erreurs de validation produites pendant la lecture d’un conteneur CPCEMU DSK.
/// </summary>
internal static class CpcDskExceptions
{
    /// <summary>Crée l’erreur signalant un bloc d’informations disque tronqué.</summary>
    /// <returns>L’exception décrivant l’en-tête tronqué.</returns>
    public static InvalidDataException TruncatedHeader() =>
        new("The Amstrad DSK header is truncated.");

    /// <summary>Crée l’erreur signalant qu’aucune signature CPCEMU DSK connue n’a été trouvée.</summary>
    /// <returns>L’exception décrivant la signature non reconnue.</returns>
    public static InvalidDataException UnrecognizedSignature() =>
        new("The file is not a CPCEMU DSK image.");

    /// <summary>Crée l’erreur signalant une géométrie CPCEMU DSK invalide.</summary>
    /// <returns>L’exception décrivant la géométrie invalide.</returns>
    public static InvalidDataException InvalidGeometry() =>
        new("The Amstrad DSK geometry is invalid.");

    /// <summary>Crée l’erreur signalant une table de tailles de pistes Extended invalide.</summary>
    /// <returns>L’exception décrivant la table invalide.</returns>
    public static InvalidDataException InvalidExtendedTrackTable() =>
        new("The extended Amstrad DSK track table is invalid.");

    /// <summary>Crée l’erreur signalant une piste dont les octets sont tronqués.</summary>
    /// <param name="trackIndex">Index linéaire de la piste dans le conteneur.</param>
    /// <returns>L’exception contenant l’index de la piste tronquée.</returns>
    public static InvalidDataException TruncatedTrack(int trackIndex) =>
        new($"Amstrad DSK track {trackIndex} is truncated.");

    /// <summary>Crée l’erreur signalant une signature de piste invalide.</summary>
    /// <param name="trackIndex">Index linéaire de la piste dans le conteneur.</param>
    /// <returns>L’exception contenant l’index de la piste invalide.</returns>
    public static InvalidDataException InvalidTrackHeader(int trackIndex) =>
        new($"Amstrad DSK track {trackIndex} has an invalid header.");

    /// <summary>Crée l’erreur signalant une table de descripteurs de secteurs invalide.</summary>
    /// <param name="trackIndex">Index linéaire de la piste contenant la table.</param>
    /// <returns>L’exception contenant l’index de la piste invalide.</returns>
    public static InvalidDataException InvalidSectorTable(int trackIndex) =>
        new($"Amstrad DSK track {trackIndex} has an invalid sector table.");

    /// <summary>Crée l’erreur signalant des données sectorielles tronquées.</summary>
    /// <param name="cylinder">Numéro de cylindre déclaré par la piste.</param>
    /// <param name="head">Numéro de face déclaré par la piste.</param>
    /// <param name="sectorId">Identifiant du secteur tronqué.</param>
    /// <returns>L’exception contenant l’adresse physique du secteur tronqué.</returns>
    public static InvalidDataException TruncatedSector(int cylinder, int head, int sectorId) =>
        new($"Amstrad DSK sector {cylinder}:{head}:{sectorId} is truncated.");

    /// <summary>Crée l’erreur signalant qu’aucun secteur n’a été extrait du conteneur.</summary>
    /// <returns>L’exception décrivant l’absence de secteurs.</returns>
    public static InvalidDataException NoSectors() =>
        new("The Amstrad DSK image contains no sectors.");
}
