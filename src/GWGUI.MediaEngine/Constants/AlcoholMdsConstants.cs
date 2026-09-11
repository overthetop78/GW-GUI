namespace GWGUI.MediaEngine.Constants;

/// <summary>Invariant MDS signatures, layout offsets, sizes, and flags used by the supported reader.</summary>
public static class AlcoholMdsConstants
{
    public const string Signature = "MEDIA DESCRIPTOR";
    public const int HeaderSize = 92;
    public const int SignatureOffset = 0;
    public const int SignatureLength = 16;
    public const int VersionOffset = 16;
    public const int MediumTypeOffset = 18;
    public const int SessionCountOffset = 20;
    public const int DiskStructureOffset = 64;
    public const int SessionsOffsetOffset = 80;
    public const int SessionBlockSize = 24;
    public const int SessionNumberOffset = 8;
    public const int SessionTotalBlocksOffset = 10;
    public const int SessionLeadInBlocksOffset = 11;
    public const int SessionFirstTrackOffset = 12;
    public const int SessionLastTrackOffset = 14;
    public const int SessionTrackBlocksOffset = 20;
    public const int TrackBlockSize = 80;
    public const int TrackModeOffset = 0;
    public const int TrackSubchannelModeOffset = 1;
    public const int TrackAdrControlOffset = 2;
    public const int TrackNumberOffset = 4;
    public const int TrackMinuteOffset = 9;
    public const int TrackSecondOffset = 10;
    public const int TrackFrameOffset = 11;
    public const int TrackExtraOffset = 12;
    public const int TrackSectorSizeOffset = 16;
    public const int TrackStartSectorOffset = 36;
    public const int TrackStartOffsetOffset = 40;
    public const int TrackSessionOffset = 48;
    public const int TrackFooterOffset = 52;
    public const int TrackFooterSize = 16;
    public const int FooterFileNameOffset = 0;
    public const int FooterWideCharacterOffset = 4;
    public const int MaximumSupportedMajorVersion = 1;
    public const ushort DvdMediumFlag = 0x10;
    public const byte LeadOutTrackNumber = 0xA2;
    public const int FramesPerSecond = 75;
    public const int SecondsPerMinute = 60;
}
