using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Atari.Msa;

/// <summary>Construit les erreurs détaillées produites pendant la lecture d'un conteneur MSA.</summary>
internal static class MsaExceptions
{
    /// <summary>Signale une image sectorielle incompatible avec le format MSA.</summary>
    public static InvalidDataException UnsupportedSectorImage(SectorImage image) => new($"Sector image '{image.FormatId}' with geometry {image.Cylinders}x{image.Heads}x{image.SectorsPerTrack} and block size {image.BlockSize} cannot be written as MSA.");
    /// <summary>Signale un secteur nécessaire absent.</summary>
    public static InvalidDataException MissingSector(int logical, int cylinder, int head, int sector) => new($"MSA logical sector {logical} ({cylinder}:{head}:{sector}) is missing.");
    /// <summary>Signale un secteur dont la taille est invalide.</summary>
    public static InvalidDataException InvalidSectorSize(int logical, int actual, int expected) => new($"MSA logical sector {logical} contains {actual} bytes; expected {expected}.");
    /// <summary>Signale un en-tête absent ou incomplet.</summary>
    public static InvalidDataException InvalidHeader(int observedLength) => new($"The MSA header is invalid; the file contains {observedLength} bytes.");
    /// <summary>Signale une géométrie MSA hors des limites acceptées.</summary>
    public static InvalidDataException InvalidGeometry(int sectors, int heads, int startCylinder, int endCylinder) => new($"The MSA geometry is invalid: {sectors} sectors, {heads} heads, cylinders {startCylinder} to {endCylinder}.");
    /// <summary>Signale un champ de longueur de piste tronqué.</summary>
    public static InvalidDataException TruncatedTrackTable(int cylinder, int head, int position, int availableLength) => new($"The MSA track-length field for cylinder {cylinder}, head {head}, at offset {position} is truncated: {availableLength} bytes available.");
    /// <summary>Signale une charge utile de piste plus courte que la longueur déclarée.</summary>
    public static InvalidDataException TruncatedTrack(int cylinder, int head, int position, int packedLength, int availableLength) => new($"MSA track {cylinder}:{head} at offset {position} declares {packedLength} bytes, {availableLength} are available.");
    /// <summary>Signale une séquence RLE incomplète.</summary>
    public static InvalidDataException TruncatedRun(int cylinder, int head, int position, int packedLength) => new($"The compressed run in MSA track {cylinder}:{head} at packed offset {position} is truncated within {packedLength} bytes.");
    /// <summary>Signale une séquence RLE dont la répétition dépasse la piste attendue.</summary>
    public static InvalidDataException InvalidRun(int cylinder, int head, int position, int count, int written, int expectedLength) => new($"The compressed run in MSA track {cylinder}:{head} at packed offset {position} has count {count}; {written} of {expectedLength} output bytes were already written.");
    /// <summary>Signale une décompression qui ne consomme ou ne produit pas la longueur attendue.</summary>
    public static InvalidDataException InvalidUnpackedLength(int cylinder, int head, int consumed, int packedLength, int written, int expectedLength) => new($"MSA track {cylinder}:{head} consumed {consumed} of {packedLength} compressed bytes and produced {written} of {expectedLength} expected bytes.");
}
