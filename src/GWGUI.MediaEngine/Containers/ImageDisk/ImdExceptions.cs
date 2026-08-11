namespace GWGUI.MediaEngine.Containers.ImageDisk;

internal static class ImdExceptions
{
    public static InvalidDataException MissingSignature(int commentEnd, int observedLength) => new($"The image does not contain an ImageDisk header: comment terminator at {commentEnd}, file length {observedLength} bytes.");
    public static InvalidDataException TruncatedSection(ImdSection section, int offset, int requiredLength, int availableLength) => new($"The ImageDisk {section} section at offset {offset} is truncated: {requiredLength} bytes are required, {availableLength} are available.");
    public static InvalidDataException InvalidTrackHeader(ImdMode mode, int sectorCount) => new($"The ImageDisk track header is invalid: mode {(byte)mode}, sector count {sectorCount}.");
    public static InvalidDataException InvalidSizeCode(byte sizeCode) => new($"The ImageDisk sector-size code {sizeCode} is invalid.");
    public static InvalidDataException InvalidRecordType(byte recordType) => new($"The ImageDisk sector-record type {recordType} is invalid.");
    public static InvalidDataException NoSectors() => new("The ImageDisk image contains no sectors.");
}
