namespace GWGUI.Scp.Containers.Apple.DiskCopy;

/// <summary>Construit les erreurs produites pendant la validation et la lecture d’un conteneur DiskCopy.</summary>
internal static class DiskCopyExceptions
{
    /// <summary>Crée l’erreur signalant un en-tête DiskCopy tronqué.</summary>
    /// <returns>L’exception décrivant l’en-tête incomplet.</returns>
    public static InvalidDataException TruncatedHeader() =>
        new("The DiskCopy header is truncated.");

    /// <summary>Crée l’erreur signalant une charge utile absente ou située hors du conteneur.</summary>
    /// <returns>L’exception décrivant la charge utile invalide.</returns>
    public static InvalidDataException InvalidPayload() =>
        new("The DiskCopy payload is invalid.");

    /// <summary>Crée l’erreur signalant une combinaison de données et tags non reconnue.</summary>
    /// <returns>L’exception décrivant la combinaison non reconnue.</returns>
    public static InvalidDataException UnrecognizedDataAndTags() =>
        new("The DiskCopy image is neither a recognized Macintosh/ProDOS image nor a tagged Lisa image.");

    /// <summary>Crée l’erreur signalant un checksum de données sectorielles invalide.</summary>
    /// <param name="storedChecksum">Checksum lu dans l’en-tête DiskCopy.</param>
    /// <param name="calculatedChecksum">Checksum calculé à partir des données sectorielles.</param>
    /// <returns>L’exception contenant les deux checksums comparés.</returns>
    public static InvalidDataException InvalidDataChecksum(uint storedChecksum, uint calculatedChecksum) =>
        new($"The DiskCopy data checksum is invalid: stored 0x{storedChecksum:X8}, calculated 0x{calculatedChecksum:X8}.");

    /// <summary>Crée l’erreur signalant un checksum de tags sectoriels invalide.</summary>
    /// <param name="storedChecksum">Checksum lu dans l’en-tête DiskCopy.</param>
    /// <param name="calculatedChecksum">Checksum calculé à partir des tags sectoriels.</param>
    /// <returns>L’exception contenant les deux checksums comparés.</returns>
    public static InvalidDataException InvalidTagChecksum(uint storedChecksum, uint calculatedChecksum) =>
        new($"The DiskCopy tag checksum is invalid: stored 0x{storedChecksum:X8}, calculated 0x{calculatedChecksum:X8}.");

    /// <summary>Crée l’erreur signalant qu’un checksum a reçu un nombre impair d’octets.</summary>
    /// <param name="byteCount">Nombre d’octets reçu par le calcul.</param>
    /// <param name="parameterName">Nom du paramètre contenant la séquence invalide.</param>
    /// <returns>L’exception contenant le nombre d’octets rejeté.</returns>
    public static ArgumentException InvalidChecksumByteCount(int byteCount, string parameterName) =>
        new($"A DiskCopy checksum requires an even number of bytes, but received {byteCount}.", parameterName);
}
