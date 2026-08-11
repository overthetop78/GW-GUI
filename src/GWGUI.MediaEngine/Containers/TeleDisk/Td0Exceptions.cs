namespace GWGUI.MediaEngine.Containers.TeleDisk;

/// <summary>Construit les erreurs détaillées produites pendant la lecture et le décodage TeleDisk.</summary>
internal static class Td0Exceptions
{
    /// <summary>Signale une section tronquée avec sa position et ses longueurs.</summary>
    /// <param name="section">Section concernée.</param><param name="position">Position, en octets.</param><param name="expectedLength">Longueur attendue.</param><param name="availableLength">Longueur disponible.</param>
    /// <returns>Erreur de données construite.</returns>
    public static InvalidDataException Truncated(Td0Section section, int position, int expectedLength, int availableLength) => new($"The TeleDisk {section} at offset {position} is truncated: {expectedLength} bytes expected, {availableLength} available.");
    /// <summary>Signale une compression avancée non prise en charge.</summary>
    /// <returns>Erreur de données construite.</returns>
    public static InvalidDataException AdvancedCompression() => new("Advanced TeleDisk compression is not supported.");
    /// <summary>Signale une signature différente des signatures TeleDisk.</summary>
    /// <returns>Erreur de données construite.</returns>
    public static InvalidDataException InvalidSignature() => new("The image is not a TeleDisk image.");
    /// <summary>Signale un CRC d'en-tête global incorrect.</summary>
    /// <param name="stored">CRC stocké.</param><param name="calculated">CRC calculé.</param>
    /// <returns>Erreur de données construite.</returns>
    public static InvalidDataException InvalidHeaderCrc(ushort stored, ushort calculated) => new($"The TeleDisk header CRC is invalid: expected 0x{stored:X4}, calculated 0x{calculated:X4}.");
    /// <summary>Signale un CRC de piste incorrect.</summary>
    /// <param name="cylinder">Cylindre.</param><param name="head">Face.</param><param name="stored">CRC stocké.</param><param name="calculated">CRC calculé.</param>
    /// <returns>Erreur de données construite.</returns>
    public static InvalidDataException InvalidTrackCrc(int cylinder, int head, byte stored, byte calculated) => new($"The TeleDisk track {cylinder}/{head} CRC is invalid: expected 0x{stored:X2}, calculated 0x{calculated:X2}.");
    /// <summary>Signale un CRC de secteur incorrect.</summary>
    /// <param name="cylinder">Cylindre.</param><param name="head">Face.</param><param name="sector">Numéro de secteur.</param><param name="stored">CRC stocké.</param><param name="calculated">CRC calculé.</param>
    /// <returns>Erreur de données construite.</returns>
    public static InvalidDataException InvalidSectorCrc(int cylinder, int head, int sector, byte stored, byte calculated) => new($"The TeleDisk sector {cylinder}/{head}/{sector} CRC is invalid: expected 0x{stored:X2}, calculated 0x{calculated:X2}.");
    /// <summary>Signale un code de taille sectorielle hors limite.</summary>
    /// <param name="cylinder">Cylindre.</param><param name="head">Face.</param><param name="sector">Numéro de secteur.</param><param name="sizeCode">Code observé.</param>
    /// <returns>Erreur de données construite.</returns>
    public static InvalidDataException InvalidSizeCode(int cylinder, int head, int sector, int sizeCode) => new($"TeleDisk sector {cylinder}/{head}/{sector} has invalid size code {sizeCode}.");
    /// <summary>Signale l'absence d'une charge utile annoncée pour un secteur.</summary>
    /// <param name="cylinder">Cylindre.</param><param name="head">Face.</param><param name="sector">Numéro de secteur.</param><param name="position">Position, en octets.</param>
    /// <returns>Erreur de données construite.</returns>
    public static InvalidDataException MissingEncodedData(int cylinder, int head, int sector, int position) => new($"TeleDisk sector {cylinder}/{head}/{sector} has no encoded data at offset {position}.");
    /// <summary>Signale un cylindre logique différent du cylindre physique annoncé.</summary>
    /// <param name="expected">Cylindre annoncé.</param><param name="observed">Cylindre observé.</param>
    /// <returns>Erreur de données construite.</returns>
    public static InvalidDataException InconsistentCylinder(int expected, int observed) => new($"The TeleDisk track declares cylinder {expected}, but its last sector declares cylinder {observed}.");
    /// <summary>Signale une face logique différente de la face physique annoncée.</summary>
    /// <param name="expected">Face annoncée.</param><param name="observed">Face observée.</param>
    /// <returns>Erreur de données construite.</returns>
    public static InvalidDataException InconsistentHead(int expected, int observed) => new($"The TeleDisk track declares head {expected}, but its last sector declares head {observed}.");
    /// <summary>Signale un conteneur ne contenant aucun secteur.</summary>
    /// <returns>Erreur de données construite.</returns>
    public static InvalidDataException NoSectors() => new("The TeleDisk image contains no sectors.");
    /// <summary>Signale une charge utile à motif répété dont la longueur est incorrecte.</summary>
    /// <param name="cylinder">Cylindre.</param><param name="head">Face.</param><param name="sector">Numéro de secteur.</param><param name="observedLength">Longueur observée.</param><param name="expectedLength">Longueur attendue.</param>
    /// <returns>Erreur de données construite.</returns>
    public static InvalidDataException InvalidRepeatedPayload(int cylinder, int head, int sector, int observedLength, int expectedLength) => new($"TeleDisk sector {cylinder}/{head}/{sector} has a repeated payload of {observedLength} bytes; {expectedLength} bytes are required.");
    /// <summary>Signale une séquence encodée tronquée avec sa position et ses longueurs.</summary>
    /// <param name="cylinder">Cylindre.</param><param name="head">Face.</param><param name="sector">Numéro de secteur.</param><param name="encoding">Encodage.</param><param name="position">Position dans la charge utile.</param><param name="expectedLength">Longueur attendue.</param><param name="availableLength">Longueur disponible.</param>
    /// <returns>Erreur de données construite.</returns>
    public static InvalidDataException TruncatedEncoding(int cylinder, int head, int sector, Td0SectorEncoding encoding, int position, int expectedLength, int availableLength) => new($"TeleDisk sector {cylinder}/{head}/{sector} encoding {encoding} is truncated at offset {position}: {expectedLength} bytes required, {availableLength} available.");
    /// <summary>Signale un identifiant d'encodage non pris en charge.</summary>
    /// <param name="cylinder">Cylindre.</param><param name="head">Face.</param><param name="sector">Numéro de secteur.</param><param name="encoding">Encodage observé.</param>
    /// <returns>Erreur de données construite.</returns>
    public static InvalidDataException UnsupportedEncoding(int cylinder, int head, int sector, Td0SectorEncoding encoding) => new($"TeleDisk sector {cylinder}/{head}/{sector} uses unsupported encoding {encoding}.");
    /// <summary>Signale une longueur décodée différente de la taille sectorielle attendue.</summary>
    /// <param name="cylinder">Cylindre.</param><param name="head">Face.</param><param name="sector">Numéro de secteur.</param><param name="encoding">Encodage.</param><param name="observedLength">Longueur produite.</param><param name="expectedLength">Longueur attendue.</param>
    /// <returns>Erreur de données construite.</returns>
    public static InvalidDataException InvalidDecodedLength(int cylinder, int head, int sector, Td0SectorEncoding encoding, int observedLength, int expectedLength) => new($"TeleDisk sector {cylinder}/{head}/{sector} encoding {encoding} expands to {observedLength} bytes instead of {expectedLength}.");
}
