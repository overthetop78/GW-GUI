namespace GWGUI.MediaFileSystems.FileSystems.Xbox.Xdvdfs;

internal static class XdvdfsFileSystemConstants
{
    public const string FileSystemId = Definitions.FileSystemIds.Xdvdfs;
    public const string FileSystemDisplayName = "Xbox XDVDFS";
    public const string FileAttribute = "XDVDFS";
    public const string FileEntryType = "xdvdfs-file";
    public const string DirectoryEntryType = "xdvdfs-directory";
    public const string SectorMetadata = "sector";
    public const string LengthMetadata = "length";
    public const int SectorSize = 2048;
    public const int VolumeDescriptorSector = 32;
    public const int VolumeDescriptorLength = SectorSize;
    public const int MagicOffset = 0;
    public const int RootDirectorySectorOffset = 0x14;
    public const int RootDirectorySizeOffset = 0x18;
    public const int DescriptorTrailerOffset = 0x7EC;
    public const int LeftChildOffset = 0x00;
    public const int RightChildOffset = 0x02;
    public const int FileStartSectorOffset = 0x04;
    public const int FileLengthOffset = 0x08;
    public const int FileAttributesOffset = 0x0C;
    public const int FileNameLengthOffset = 0x0D;
    public const int FileNameOffset = 0x0E;
    public const int MinimumEntryLength = FileNameOffset;
    public const byte DirectoryAttribute = 0x10;
    public const int MaximumDirectoryDepth = 256;
    public const int MaximumFileContentLength = 64 * 1024 * 1024;
    public const string MagicText = "MICROSOFT*XBOX*MEDIA";
}
