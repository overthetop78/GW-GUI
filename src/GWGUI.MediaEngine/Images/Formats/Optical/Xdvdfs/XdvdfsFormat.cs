using System.Collections.Frozen;
using System.Text;
using GWGUI.MediaEngine.Constants;

namespace GWGUI.MediaEngine.Images.Formats.Optical.Xdvdfs;

internal static class XdvdfsFormat
{
    public const int SectorSize = 2048;
    public const int VolumeDescriptorSector = 32;
    public const int VolumeDescriptorLength = SectorSize;
    public const int MagicOffset = 0;
    public const int RootDirectorySectorOffset = 0x14;
    public const int RootDirectorySizeOffset = 0x18;
    public const int TrailerMagicOffset = 0x7EC;
    public const int RootDirectoryEntryMinimumLength = 14;
    public const int FileAttributesOffset = 0x0C;
    public const int FileNameLengthOffset = 0x0D;
    public const int FileNameOffset = 0x0E;
    public const int FileStartSectorOffset = 0x04;
    public const int FileLengthOffset = 0x08;
    public const int LeftChildOffset = 0x00;
    public const int RightChildOffset = 0x02;
    public const byte DirectoryAttribute = 0x10;

    public static readonly byte[] Magic = Encoding.ASCII.GetBytes("MICROSOFT*XBOX*MEDIA");

    public static readonly IReadOnlySet<string> Extensions =
        new[] { DiskImageFileExtensions.Iso }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static bool IsLengthCompatible(long length) =>
        length >= checked((long)(VolumeDescriptorSector + 1) * SectorSize)
        && length % SectorSize == 0;

    public static int AlignFour(int value) => checked((value + 3) & ~3);
}
