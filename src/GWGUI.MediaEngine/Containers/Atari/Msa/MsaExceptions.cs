namespace GWGUI.MediaEngine.Containers.Atari.Msa;

internal static class MsaExceptions
{
    public static InvalidDataException InvalidHeader(int observedLength) => new($"The MSA header is invalid; the file contains {observedLength} bytes.");
    public static InvalidDataException InvalidGeometry(int sectors, int heads, int startCylinder, int endCylinder) => new($"The MSA geometry is invalid: {sectors} sectors, {heads} heads, cylinders {startCylinder} to {endCylinder}.");
    public static InvalidDataException TruncatedTrackTable(int cylinder, int head, int position, int availableLength) => new($"The MSA track-length field for cylinder {cylinder}, head {head}, at offset {position} is truncated: {availableLength} bytes available.");
    public static InvalidDataException TruncatedTrack(int cylinder, int head, int position, int packedLength, int availableLength) => new($"MSA track {cylinder}:{head} at offset {position} declares {packedLength} bytes, {availableLength} are available.");
    public static InvalidDataException TruncatedRun(int cylinder, int head, int position, int packedLength) => new($"The compressed run in MSA track {cylinder}:{head} at packed offset {position} is truncated within {packedLength} bytes.");
    public static InvalidDataException InvalidRun(int cylinder, int head, int position, int count, int written, int expectedLength) => new($"The compressed run in MSA track {cylinder}:{head} at packed offset {position} has count {count}; {written} of {expectedLength} output bytes were already written.");
    public static InvalidDataException InvalidUnpackedLength(int cylinder, int head, int consumed, int packedLength, int written, int expectedLength) => new($"MSA track {cylinder}:{head} consumed {consumed} of {packedLength} compressed bytes and produced {written} of {expectedLength} expected bytes.");
}
