namespace GWGUI.MediaEngine.Containers.Atari.Msa;

internal static class MsaLayout
{
    public const int HeaderSize = 10;
    public const int SignatureOffset = 0;
    public const int SectorsPerTrackOffset = 2;
    public const int HeadsOffset = 4;
    public const int StartCylinderOffset = 6;
    public const int EndCylinderOffset = 8;
    public const int TrackLengthFieldSize = 2;
    public const int SectorSize = 512;
    public const int MinimumSectorsPerTrack = 1;
    public const int MaximumSectorsPerTrack = 36;
    public const int MinimumHeadCount = 1;
    public const int MaximumHeadCount = 2;
    public const int MaximumCylinder = 255;
    public const int RleSequenceSize = 4;
    public const int RleValueOffset = 1;
    public const int RleCountOffset = 2;
}
