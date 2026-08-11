namespace GWGUI.MediaEngine.Containers.I86f;

internal static class I86fLayout
{
    public const int MinimumFileLength = 8;
    public const int FileFlagsOffset = 6;
    public const int FileFlagsLength = 2;
    public const int TrackTableOffset = 8;
    public const int TrackTableEntrySize = 4;
    public const int TrackTableEntriesPerSide = 256;
    public const int TwoSideTrackTableEntries = 512;
    public const int StandardTrackHeaderSize = 6;
    public const int ExtendedTrackHeaderSize = 10;
    public const int TrackFlagsOffset = 0;
    public const int ExplicitBitCountOffset = 2;
    public const int WordBitAlignment = 16;
    public const int BytesPerWord = 2;
    public const int BitsPerByte = 8;
    public const byte MostSignificantBitMask = 0x80;
    public const uint TicksPerBitCell = 40;
}
