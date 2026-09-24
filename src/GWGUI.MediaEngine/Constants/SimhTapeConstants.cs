namespace GWGUI.MediaEngine.Constants;

/// <summary>Invariant SIMH tape record words, masks, sizes, and metadata keys.</summary>
internal static class SimhTapeConstants
{
    public const int WordSize = 4;
    public const uint TapeMark = 0x00000000;
    public const uint EraseGap = 0xfffffffe;
    public const uint EndOfMedium = 0xffffffff;
    public const uint ErrorFlag = 0x80000000;
    public const uint ReservedLengthBits = 0x7f000000;
    public const uint RecordLengthMask = 0x00ffffff;
    public const int RecordAlignment = 2;
    public const string ObjectKindMetadataKey = "simhObjectKind";
    public const string RecordErrorMetadataKey = "simhRecordError";
    public const string MarkerValueMetadataKey = "simhMarkerValue";
    public const string DataRecordKind = "dataRecord";
    public const string TapeMarkKind = "tapeMark";
    public const string EraseGapKind = "eraseGap";
    public const string EndOfMediumKind = "endOfMedium";
}
