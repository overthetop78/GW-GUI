namespace GWGUI.MediaEngine.Containers.I86f;

/// <summary>Construit les erreurs paramétrées produites pendant la lecture d'un conteneur 86F.</summary>
internal static class I86fExceptions
{
    /// <summary>Signale une signature 86F absente ou inaccessible.</summary>
    public static InvalidDataException MissingSignature(int observedLength) => new($"The file does not contain an 86F signature; its observed length is {observedLength} bytes.");
    /// <summary>Signale une table de pistes tronquée.</summary>
    public static InvalidDataException IncompleteTrackTable(int expectedLength, int observedLength) => new($"The 86F track table is incomplete: {expectedLength} bytes are required, {observedLength} are available.");
    /// <summary>Signale une position de piste qui dépasse la plage représentable.</summary>
    public static InvalidDataException TrackOffsetOutsideRange(int logicalTrack, uint offset, int fileLength) => new($"86F track {logicalTrack} points to offset {offset}, outside the {fileLength}-byte image.");
    /// <summary>Signale une plage de piste incohérente.</summary>
    public static InvalidDataException InvalidTrackRange(int logicalTrack, int offset, int nextOffset, int headerSize, int fileLength) => new($"86F track {logicalTrack} has an invalid range: offset {offset}, next offset {nextOffset}, header {headerSize} bytes, file length {fileLength} bytes.");
    /// <summary>Signale un nombre de cellules de bits nul ou négatif.</summary>
    public static InvalidDataException InvalidBitCount(int logicalTrack, int bitCount) => new($"86F track {logicalTrack} has an invalid bit-cell count of {bitCount}.");
    /// <summary>Signale une charge utile de piste tronquée.</summary>
    public static InvalidDataException TruncatedTrack(int logicalTrack, int offset, int nextOffset, long expectedLength, int availableLength) => new($"86F track {logicalTrack} is truncated between offsets {offset} and {nextOffset}: {expectedLength} data bytes are required, {availableLength} are available.");
}
