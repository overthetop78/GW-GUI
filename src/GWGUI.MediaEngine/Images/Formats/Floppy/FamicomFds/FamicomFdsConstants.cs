using System.Text;
using GWGUI.MediaFileSystems.Constants;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.FamicomFds;

/// <summary>Constantes communes au conteneur Famicom Disk System.</summary>
internal static class FamicomFdsConstants
{
    public const int HeaderLength = 16;
    public const int SideLength = 65_500;
    public const int SideCountOffset = 4;
    public const int MinimumSideCount = 1;
    public const string HeaderedMetadata = FamicomDiskMetadataConstants.Headered;
    public const string HeaderLengthMetadata = FamicomDiskMetadataConstants.HeaderLength;
    public const string SideCountMetadata = FamicomDiskMetadataConstants.SideCount;
    public const string DeclaredSideCountMetadata = FamicomDiskMetadataConstants.DeclaredSideCount;
    public const string SideLengthMetadata = FamicomDiskMetadataConstants.SideLength;
    public const string SideNamePrefix = FamicomDiskMetadataConstants.SideNamePrefix;
    public const string SideNameSuffix = FamicomDiskMetadataConstants.SideNameSuffix;
    public const string SystemIdMetadata = "systemId";
    public const string MediaRoleMetadata = "mediaRole";
    public const string SystemId = "nintendo";
    public const string MediaRole = "floppy";
    public const string VolumeName = "Famicom Disk System";
    public const string SideNameFormat = "side{0:D2}";
    public static readonly byte[] Signature = Encoding.ASCII.GetBytes("FDS\x1A");
}
