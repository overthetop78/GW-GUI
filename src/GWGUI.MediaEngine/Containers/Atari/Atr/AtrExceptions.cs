namespace GWGUI.MediaEngine.Containers.Atari.Atr;

using GWGUI.MediaEngine.SectorImages;

/// <summary>Construit les erreurs produites pendant la validation d'un conteneur ATR.</summary>
internal static class AtrExceptions
{
    /// <summary>Signale un identifiant absent du catalogue ATR en écriture.</summary>
    public static InvalidDataException UnsupportedFormat(string formatId) => new($"ATR target format '{formatId}' is not supported.");
    /// <summary>Signale une image sectorielle incompatible avec le profil demandé.</summary>
    public static InvalidDataException IncompatibleSectorImage(SectorImage image, AtrFormatProfile profile) => new($"Sector image '{image.FormatId}' contains {image.BlockCount} sectors and {image.Capacity} bytes; ATR profile '{profile.FormatId}' requires {profile.SectorCount} sectors and {profile.PayloadLength} bytes.");
    /// <summary>Signale un secteur requis absent.</summary>
    public static InvalidDataException MissingSector(int sector) => new($"ATR sector {sector} is missing.");
    /// <summary>Signale une taille sectorielle incompatible.</summary>
    public static InvalidDataException InvalidSectorSize(int sector, int actual, int expected) => new($"ATR sector {sector} contains {actual} bytes; expected {expected}.");
    /// <summary>Crée l'erreur signalant un en-tête trop court ou une signature invalide.</summary>
    /// <param name="observedLength">Longueur observée du fichier, en octets.</param>
    /// <param name="expectedMinimumLength">Longueur minimale attendue, en octets.</param>
    /// <param name="observedSignature">Signature observée, lorsqu'elle peut être lue.</param>
    /// <param name="expectedSignature">Signature ATR attendue.</param>
    /// <returns>Exception décrivant les valeurs observées et attendues.</returns>
    public static InvalidDataException InvalidHeader(long observedLength, int expectedMinimumLength, ushort? observedSignature, ushort expectedSignature) =>
        new($"The ATR header is invalid: length {observedLength} bytes (minimum {expectedMinimumLength}), signature {(observedSignature.HasValue ? $"0x{observedSignature.Value:X4}" : "unavailable")} (expected 0x{expectedSignature:X4}).");

    /// <summary>Crée l'erreur signalant une taille sectorielle non prise en charge.</summary>
    /// <param name="observedSectorSize">Taille observée, en octets.</param>
    /// <param name="expectedSectorSizes">Tailles acceptées, en octets.</param>
    /// <returns>Exception contenant les tailles observée et attendues.</returns>
    public static InvalidDataException UnsupportedSectorSize(int observedSectorSize, params int[] expectedSectorSizes) =>
        new($"The ATR sector size {observedSectorSize} bytes is not supported; expected {string.Join(", ", expectedSectorSizes)} bytes.");

    /// <summary>Crée l'erreur signalant une longueur déclarée différente de la charge utile observée.</summary>
    /// <param name="observedPayloadLength">Longueur observée de la charge utile, en octets.</param>
    /// <param name="declaredPayloadLength">Longueur déclarée dans l'en-tête, en octets.</param>
    /// <returns>Exception contenant les deux longueurs.</returns>
    public static InvalidDataException PayloadLengthMismatch(long observedPayloadLength, long declaredPayloadLength) =>
        new($"The ATR payload length is {observedPayloadLength} bytes; the header declares {declaredPayloadLength} bytes.");

    /// <summary>Crée l'erreur signalant une charge utile qui ne contient pas des secteurs complets.</summary>
    /// <param name="observedPayloadLength">Longueur observée de la charge utile, en octets.</param>
    /// <param name="bootAreaLength">Longueur attendue de la zone d'amorçage, en octets.</param>
    /// <param name="sectorSize">Taille nominale attendue des secteurs suivants, en octets.</param>
    /// <returns>Exception contenant les longueurs nécessaires au diagnostic.</returns>
    public static InvalidDataException TruncatedPayload(long observedPayloadLength, int bootAreaLength, int sectorSize) =>
        new($"The ATR payload length {observedPayloadLength} bytes does not contain a {bootAreaLength}-byte boot area followed by complete {sectorSize}-byte sectors.");
}
