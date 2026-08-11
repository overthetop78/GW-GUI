namespace GWGUI.MediaEngine.Containers.Cp2;

/// <summary>Décrit les positions et tailles binaires du conteneur CP2.</summary>
internal static class Cp2Layout
{
    public const int MinimumFileLength = 34;
    public const int SignatureOffset = 0;
    public const int FirstGroupOffset = 28;
    public const int GroupHeaderSize = 4;
    public const int MetadataLengthOffset = 2;
    public const int LengthFieldSize = 2;
    public const int FramingSize = 2;
    public const int MetadataLengthAdjustment = 1;
    public const int TrackDescriptorSize = 387;
    public const int SectorDescriptorSize = 16;
    public const int TrackHeaderSize = 7;
    public const int TrackCylinderOffset = 0;
    public const int TrackHeadOffset = 1;
    public const int TrackSectorCountOffset = 2;
    public const int MaximumSectorDescriptorCount = 23;
    public const int SectorCylinderOffset = 0;
    public const int SectorHeadOffset = 1;
    public const int SectorNumberOffset = 2;
    public const int SectorSizeCodeOffset = 3;
    public const int SectorPositionOffset = 5;
    public const int SectorPositionLength = 2;
    public const int BaseSectorSize = 128;
    public const int MaximumSectorSizeCode = 7;
    public const int ReconstructedSectorSize = 512;
}
