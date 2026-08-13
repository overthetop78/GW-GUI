namespace GWGUI.MediaEngine.Containers.Hfe;

/// <summary>Définit les offsets et tailles du conteneur HFE version 1.</summary>
public static class HfeLayout
{
    public const int BlockSize = 512;
    public const int SideChunkSize = 256;
    public const int SignatureOffset = 0;
    public const int SignatureLength = 8;
    public const int RevisionOffset = 8;
    public const int CylinderCountOffset = 9;
    public const int HeadCountOffset = 10;
    public const int EncodingOffset = 11;
    public const int BitRateOffset = 12;
    public const int RpmOffset = 14;
    public const int InterfaceModeOffset = 16;
    public const int WriteProtectedOffset = 17;
    public const int TrackListOffset = 18;
    public const int WriteAllowedOffset = 20;
    public const int SingleStepOffset = 21;
    public const int TrackListEntrySize = 4;
    public const int TrackOffsetOffset = 0;
    public const int TrackLengthOffset = 2;
}
