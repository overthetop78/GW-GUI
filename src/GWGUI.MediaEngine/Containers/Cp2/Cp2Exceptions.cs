using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Cp2;

/// <summary>Construit les erreurs paramétrées produites pendant la lecture CP2.</summary>
internal static class Cp2Exceptions
{
    public static InvalidDataException MissingSignature() => new("The file does not contain a SNATCH-IT CP2 signature.");
    public static InvalidDataException InvalidDescriptionBlock(int offset, int declaredLength, int availableLength) => new($"The CP2 track-description block at offset {offset} is invalid: declared {declaredLength} bytes, {availableLength} available.");
    public static InvalidDataException TruncatedDescriptionBlock(int offset, int declaredLength, int availableLength) => new($"The CP2 track-description block at offset {offset} is truncated: declared {declaredLength} bytes, {availableLength} available.");
    public static InvalidDataException TruncatedSectorData(SectorAddress address, int offset, int size, int availableLength) => new($"CP2 sector {address.Cylinder}:{address.Head}:{address.Number} at offset {offset} requires {size} bytes, {availableLength} available.");
    public static InvalidDataException NoSectors() => new("The CP2 image contains no readable sectors.");
    public static InvalidDataException InvalidGeometry(int heads, int sectorsPerTrack) => new($"The CP2 image geometry is invalid: {heads} heads and {sectorsPerTrack} sectors per track.");
    public static InvalidDataException InvalidSectorDescriptorCount(int observedCount, int maximumCount) => new($"The CP2 sector-description count {observedCount} is invalid; maximum {maximumCount}.");
}
