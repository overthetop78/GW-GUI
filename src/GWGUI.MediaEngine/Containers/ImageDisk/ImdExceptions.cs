namespace GWGUI.MediaEngine.Containers.ImageDisk;

/// <summary>Construit les erreurs paramétrées produites pendant la lecture ImageDisk.</summary>
internal static class ImdExceptions
{
    /// <summary>Signale une signature ou un terminateur de commentaire absent.</summary>
    public static InvalidDataException MissingSignature(int commentEnd, int observedLength) => new($"The image does not contain an ImageDisk header: comment terminator at {commentEnd}, file length {observedLength} bytes.");
    /// <summary>Signale une section tronquée avec sa position et ses longueurs.</summary>
    public static InvalidDataException TruncatedSection(ImdSection section, int offset, int requiredLength, int availableLength) => new($"The ImageDisk {section} section at offset {offset} is truncated: {requiredLength} bytes are required, {availableLength} are available.");
    /// <summary>Signale un mode ou un nombre de secteurs invalide.</summary>
    public static InvalidDataException InvalidTrackHeader(ImdMode mode, int sectorCount) => new($"The ImageDisk track header is invalid: mode {(byte)mode}, sector count {sectorCount}.");
    /// <summary>Signale un code de taille sectorielle invalide.</summary>
    public static InvalidDataException InvalidSizeCode(byte sizeCode) => new($"The ImageDisk sector-size code {sizeCode} is invalid.");
    /// <summary>Signale un type d'enregistrement sectoriel invalide.</summary>
    public static InvalidDataException InvalidRecordType(byte recordType) => new($"The ImageDisk sector-record type {recordType} is invalid.");
    /// <summary>Signale l'absence de tout secteur déclaré.</summary>
    public static InvalidDataException NoSectors() => new("The ImageDisk image contains no sectors.");
}
