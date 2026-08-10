namespace GWGUI.MediaEngine.Containers.Apple.Woz;

/// <summary>Construit les erreurs produites pendant la validation d’un conteneur WOZ.</summary>
internal static class WozExceptions
{
    /// <summary>Crée l’erreur signalant un en-tête WOZ invalide.</summary>
    /// <returns>Exception décrivant l’en-tête invalide.</returns>
    public static InvalidDataException InvalidHeader() => new("The WOZ header is invalid.");

    /// <summary>Crée l’erreur signalant un type de disque WOZ non pris en charge.</summary>
    /// <param name="observedDiskType">Type de disque observé dans le chunk INFO.</param>
    /// <returns>Exception contenant le type observé.</returns>
    public static NotSupportedException UnsupportedDiskType(byte observedDiskType) =>
        new($"WOZ disk type {observedDiskType} is not supported; an Apple II 5.25-inch disk is required.");

    /// <summary>Crée l’erreur signalant l’absence d’un chunk obligatoire.</summary>
    /// <param name="chunkId">Identifiant du chunk absent ou incomplet.</param>
    /// <returns>Exception contenant l’identifiant du chunk.</returns>
    public static InvalidDataException MissingRequiredChunk(string chunkId) =>
        new($"The required WOZ chunk '{chunkId}' is missing or incomplete.");

    /// <summary>Crée l’erreur signalant une charge utile de chunk tronquée.</summary>
    /// <param name="chunkId">Identifiant du chunk tronqué.</param>
    /// <returns>Exception contenant l’identifiant du chunk.</returns>
    public static InvalidDataException TruncatedChunk(string chunkId) =>
        new($"The WOZ {chunkId} chunk is truncated.");

    /// <summary>Crée l’erreur signalant qu’une entrée TMAP référence des données hors limites.</summary>
    /// <param name="track">Piste Apple II examinée.</param>
    /// <param name="descriptor">Index de descripteur référencé par TMAP.</param>
    /// <returns>Exception contenant la piste et le descripteur rejetés.</returns>
    public static InvalidDataException TrackReferenceOutOfBounds(int track, int descriptor) =>
        new($"WOZ track {track} references out-of-bounds descriptor {descriptor}.");

    /// <summary>Crée l’erreur signalant un CRC32 WOZ incohérent.</summary>
    /// <param name="storedCrc">CRC32 stocké dans l’en-tête.</param>
    /// <param name="computedCrc">CRC32 calculé sur les chunks.</param>
    /// <returns>Exception contenant les CRC stocké et calculé.</returns>
    public static InvalidDataException InvalidCrc(uint storedCrc, uint computedCrc) =>
        new($"The WOZ CRC32 is invalid: stored 0x{storedCrc:X8}, computed 0x{computedCrc:X8}.");
}
