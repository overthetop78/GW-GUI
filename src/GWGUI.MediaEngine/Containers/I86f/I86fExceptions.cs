namespace GWGUI.MediaEngine.Containers.I86f;

internal static class I86fExceptions
{
    public static InvalidDataException MissingSignature(int observedLength) => new($"The file does not contain an 86F signature; its observed length is {observedLength} bytes.");
    public static InvalidDataException IncompleteTrackTable(int expectedLength, int observedLength) => new($"The 86F track table is incomplete: {expectedLength} bytes are required, {observedLength} are available.");
    public static InvalidDataException TrackOffsetOutsideRange(int logicalTrack, uint offset, int fileLength) => new($"86F track {logicalTrack} points to offset {offset}, outside the {fileLength}-byte image.");
    public static InvalidDataException InvalidTrackRange(int logicalTrack, int offset, int nextOffset, int headerSize, int fileLength) => new($"86F track {logicalTrack} has an invalid range: offset {offset}, next offset {nextOffset}, header {headerSize} bytes, file length {fileLength} bytes.");
    public static InvalidDataException InvalidBitCount(int logicalTrack, int bitCount) => new($"86F track {logicalTrack} has an invalid bit-cell count of {bitCount}.");
    public static InvalidDataException TruncatedTrack(int logicalTrack, int offset, int nextOffset, int expectedLength, int availableLength) => new($"86F track {logicalTrack} is truncated between offsets {offset} and {nextOffset}: {expectedLength} data bytes are required, {availableLength} are available.");
}
