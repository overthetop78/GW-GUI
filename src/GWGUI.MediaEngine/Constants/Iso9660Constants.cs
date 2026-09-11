namespace GWGUI.MediaEngine.Constants;

/// <summary>Invariant ISO 9660 volume descriptor and directory record dimensions.</summary>
internal static class Iso9660Constants
{
    public const int LogicalBlockSize = 2_048;
    public const int FirstVolumeDescriptorBlock = 16;
    public const int VolumeDescriptorIdentifierOffset = 1;
    public const int VolumeDescriptorIdentifierLength = 5;
    public const string VolumeDescriptorIdentifier = "CD001";
    public const byte PrimaryVolumeDescriptorType = 1;
    public const byte SupplementaryVolumeDescriptorType = 2;
    public const byte VolumeDescriptorTerminatorType = 255;
    public const int VolumeIdentifierOffset = 40;
    public const int VolumeIdentifierLength = 32;
    public const int VolumeSpaceSizeOffset = 80;
    public const int LogicalBlockSizeOffset = 128;
    public const int RootDirectoryRecordOffset = 156;
    public const int DirectoryRecordMinimumLength = 34;
    public const int DirectoryExtentOffset = 2;
    public const int DirectoryDataLengthOffset = 10;
    public const int DirectoryDateOffset = 18;
    public const int DirectoryFlagsOffset = 25;
    public const byte DirectoryFlag = 0x02;
    public const int DirectoryFileIdentifierLengthOffset = 32;
    public const int DirectoryFileIdentifierOffset = 33;
    public const int SuspSignatureLength = 2;
    public const int SuspLengthOffset = 2;
    public const int SuspVersionOffset = 3;
    public const byte SuspVersion = 1;
    public const string RockRidgeAlternateNameSignature = "NM";
    public const byte RockRidgeAlternateNameSignatureFirstByte = (byte)'N';
    public const byte RockRidgeAlternateNameSignatureSecondByte = (byte)'M';
    public const int RockRidgeAlternateNameMinimumLength = 5;
    public const int RockRidgeAlternateNameFlagsOffset = 4;
    public const int RockRidgeAlternateNameContentOffset = 5;
    public const byte RockRidgeAlternateNameContinueFlag = 0x01;
    public const byte RockRidgeAlternateNameCurrentFlag = 0x02;
    public const byte RockRidgeAlternateNameParentFlag = 0x04;
    public const byte SuspExtensionReferenceSignatureFirstByte = (byte)'E';
    public const byte SuspExtensionReferenceSignatureSecondByte = (byte)'R';
    public const int SuspExtensionReferenceMinimumLength = 8;
    public const int SuspExtensionIdentifierLengthOffset = 4;
    public const int SuspExtensionIdentifierOffset = 8;
    public const string RockRidgeExtensionIdentifier = "RRIP_1991A";
    public const string RockRidgeIeeeExtensionIdentifier = "IEEE_P1282";
}
