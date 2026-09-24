namespace GWGUI.MediaFileSystems.FileSystems.Atari.SpartaDos;

/// <summary>Décrit les champs binaires du système de fichiers SpartaDOS.</summary>
internal static class SpartaDosFileSystemLayout
{
    public const int FirstSectorNumber = 1;
    public const int BootSectorNumber = 1;
    public const int IdentificationOffset = 7;
    public const byte Identification = 0x80;
    public const int RootDirectoryMapOffset = 9;
    public const int TotalSectorCountOffset = 11;
    public const int FreeSectorCountOffset = 13;
    public const int BitmapSectorCountOffset = 15;
    public const int FirstBitmapSectorOffset = 16;
    public const int FirstFileAllocationSectorOffset = 18;
    public const int FirstDirectoryAllocationSectorOffset = 20;
    public const int VolumeNameOffset = 22;
    public const int VolumeNameLength = 8;
    public const int TrackCountOffset = 30;
    public const int SectorSizeOffset = 31;
    public const byte DoubleDensitySectorSizeCode = 0;
    public const byte SingleDensitySectorSizeCode = 128;
    public const int VersionOffset = 32;
    public const byte Version1 = 0x10;
    public const byte Version2 = 0x20;
    public const int VolumeSequenceOffset = 38;
    public const int VolumeRandomOffset = 39;
    public const int BootFileMapOffset = 40;
    public const int WriteLockOffset = 42;
    public const byte WriteLocked = 0xff;
    public const int MinimumBootSectorLength = WriteLockOffset + 1;

    public const int SectorMapNextOffset = 0;
    public const int SectorMapPreviousOffset = 2;
    public const int SectorMapDataOffset = 4;
    public const int SectorNumberSize = sizeof(ushort);

    public const int DirectoryEntrySize = 23;
    public const int DirectoryHeaderLengthOffset = 3;
    public const int DirectoryHeaderNameOffset = 6;
    public const int DirectoryHeaderNameLength = 8;
    public const int DirectoryFirstEntryOffset = DirectoryEntrySize;
    public const int DirectoryStatusOffset = 0;
    public const int DirectoryMapOffset = 1;
    public const int DirectoryLengthOffset = 3;
    public const int DirectoryNameOffset = 6;
    public const int DirectoryNameLength = 8;
    public const int DirectoryExtensionOffset = 14;
    public const int DirectoryExtensionLength = 3;
    public const int DirectoryDateOffset = 17;
    public const int DirectoryTimeOffset = 20;
    public const int PackedLengthSize = 3;
    public const int DateFieldLength = 3;
    public const int TimeFieldLength = 3;
    public const byte NamePadding = 0x20;

    public const string VolumeAttribute = "spartados";
    public const string Version1Attribute = "version-1";
    public const string Version2Attribute = "version-2";
    public const string WriteProtectedAttribute = "write-protected";
    public const string ProtectedAttribute = "protected";
    public const string HiddenAttribute = "hidden";
    public const string ArchivedAttribute = "archived";
    public const string SparseAttribute = "sparse";
    public const string DirectoryNativeType = "directory";
    public const string FileNativeType = "file";
}
