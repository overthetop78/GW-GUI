using System.Text;

namespace GWGUI.MediaFileSystems.FileSystems.Nintendo.FamicomDisk;

/// <summary>Constantes structurales du système de fichiers Famicom Disk System.</summary>
internal static class FamicomDiskFileSystemConstants
{
    public const int DiskInfoBlockCode = 0x01;
    public const int DiskInfoLength = 56;
    public const int FileCountBlockCode = 0x02;
    public const int FileCountLength = 2;
    public const int FileHeaderBlockCode = 0x03;
    public const int FileHeaderLength = 18;
    public const int FileDataBlockCode = 0x04;
    public const int FileNumberOffset = 1;
    public const int FileIdentifierOffset = 2;
    public const int FileIdentifierLength = 3;
    public const int FileNameOffset = 5;
    public const int FileNameLength = 8;
    public const int FileAddressOffset = 13;
    public const int FileSizeOffset = 15;
    public const int FileTypeOffset = 17;
    public const int FirstSide = 0;
    public const int EmptyLength = 0;
    public const string FileSystemId = "nintendo-fds";
    public const string FileSystemDisplayName = "Famicom Disk System";
    public const string DefaultFileNameFormat = "file{0:D2}";
    public const string SideMetadata = "side";
    public const string FileNumberMetadata = "fileNumber";
    public const string FileAddressMetadata = "address";
    public const string FileTypeMetadata = "type";
    public const string FileEntryType = "fds-file";
    public const string SideAttribute = "side";
    public const string FileAttribute = "file";
    public static readonly byte[] FileIdentifier = Encoding.ASCII.GetBytes("HVC");
}
