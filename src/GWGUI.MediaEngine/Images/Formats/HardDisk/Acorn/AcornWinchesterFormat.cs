using System.Collections.Frozen;
using GWGUI.MediaEngine.Contracts;

namespace GWGUI.MediaEngine.Images.Formats.HardDisk.Acorn;

/// <summary>Disposition des images Winchester Acorn : secteurs DAT et descripteur SCSI DSC séparé.</summary>
internal static class AcornWinchesterFormat
{
    internal const int SectorSize = 256, SectorsPerTrack = 33, DescriptorSize = 33;
    internal const string DescriptorMetadataKey = "acorn.dsc";
    internal static readonly IReadOnlySet<string> Extensions = new[] { ".dat" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    internal static readonly IReadOnlySet<string> AssociatedExtensions = new[] { ".dsc" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>Lit les cylindres big-endian et les têtes du descripteur utilisé par BeebEm, B-em et b2.</summary>
    internal static HardDiskGeometry? Geometry(ReadOnlySpan<byte> descriptor)
    {
        if (descriptor.Length < 22) return null;
        var cylinders = (descriptor[13] << 8) | descriptor[14];
        var heads = descriptor[15];
        return cylinders == 0 || heads == 0 ? null : new(cylinders, heads, SectorsPerTrack, SectorSize);
    }
}
