namespace GWGUI.MediaEngine.Containers.TeleDisk;

internal static class Td0Exceptions
{
    public static InvalidDataException Truncated(Td0Section section, int position, int expectedLength, int availableLength) => new($"The TeleDisk {section} at offset {position} is truncated: {expectedLength} bytes expected, {availableLength} available.");
    public static InvalidDataException AdvancedCompression() => new("Advanced TeleDisk compression is not supported.");
    public static InvalidDataException InvalidSignature() => new("The image is not a TeleDisk image.");
    public static InvalidDataException InvalidHeaderCrc(ushort stored, ushort calculated) => new($"The TeleDisk header CRC is invalid: expected 0x{stored:X4}, calculated 0x{calculated:X4}.");
    public static InvalidDataException InvalidTrackCrc(int cylinder, int head, byte stored, byte calculated) => new($"The TeleDisk track {cylinder}/{head} CRC is invalid: expected 0x{stored:X2}, calculated 0x{calculated:X2}.");
    public static InvalidDataException InvalidSectorCrc(int cylinder, int head, int sector, byte stored, byte calculated) => new($"The TeleDisk sector {cylinder}/{head}/{sector} CRC is invalid: expected 0x{stored:X2}, calculated 0x{calculated:X2}.");
    public static InvalidDataException InvalidSizeCode(int cylinder, int head, int sector, int sizeCode) => new($"TeleDisk sector {cylinder}/{head}/{sector} has invalid size code {sizeCode}.");
    public static InvalidDataException MissingEncodedData(int cylinder, int head, int sector, int position) => new($"TeleDisk sector {cylinder}/{head}/{sector} has no encoded data at offset {position}.");
    public static InvalidDataException InconsistentCylinder(int expected, int observed) => new($"The TeleDisk track declares cylinder {expected}, but its last sector declares cylinder {observed}.");
    public static InvalidDataException InconsistentHead(int expected, int observed) => new($"The TeleDisk track declares head {expected}, but its last sector declares head {observed}.");
    public static InvalidDataException NoSectors() => new("The TeleDisk image contains no sectors.");
    public static InvalidDataException InvalidRepeatedPayload(int cylinder, int head, int sector, int observedLength, int expectedLength) => new($"TeleDisk sector {cylinder}/{head}/{sector} has a repeated payload of {observedLength} bytes; {expectedLength} bytes are required.");
    public static InvalidDataException TruncatedEncoding(int cylinder, int head, int sector, Td0SectorEncoding encoding, int position, int expectedLength, int availableLength) => new($"TeleDisk sector {cylinder}/{head}/{sector} encoding {encoding} is truncated at offset {position}: {expectedLength} bytes required, {availableLength} available.");
    public static InvalidDataException UnsupportedEncoding(int cylinder, int head, int sector, Td0SectorEncoding encoding) => new($"TeleDisk sector {cylinder}/{head}/{sector} uses unsupported encoding {encoding}.");
    public static InvalidDataException InvalidDecodedLength(int cylinder, int head, int sector, Td0SectorEncoding encoding, int observedLength, int expectedLength) => new($"TeleDisk sector {cylinder}/{head}/{sector} encoding {encoding} expands to {observedLength} bytes instead of {expectedLength}.");
}
