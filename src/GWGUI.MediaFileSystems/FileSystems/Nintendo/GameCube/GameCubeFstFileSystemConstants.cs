namespace GWGUI.MediaFileSystems.FileSystems.Nintendo.GameCube;

internal static class GameCubeFstFileSystemConstants
{
    public const string FileSystemId = Definitions.FileSystemIds.NintendoGameCubeFst;
    public const string SourceExtension = ".gcm";
    public const string IsoExtension = ".iso";
    public const string FileSystemDisplayName = "Nintendo GameCube FST";
    public const string FileAttribute = "FST";
    public const string FileEntryType = "nintendo-gamecube-file";
    public const string DirectoryEntryType = "nintendo-gamecube-directory";
    public const string OffsetMetadata = "offset";
    public const string LengthMetadata = "length";
    public const int DiscHeaderLength = 0x430;
    public const int DiscMagicOffset = 0x1C;
    public const uint DiscMagic = 0xC233_9F3Du;
    public const int FstOffsetField = 0x424;
    public const int FstSizeField = 0x428;
    public const int FstEntrySize = 12;
    public const uint DirectoryFlag = 0x8000_0000u;
    public const uint NameOffsetMask = 0x00FF_FFFFu;
    public const int SectorSize = 2048;
    public const int MaximumEntryCount = 1_000_000;
    public const int MaximumDirectoryDepth = 256;
    public const int MaximumFileContentLength = 64 * 1024 * 1024;
}
