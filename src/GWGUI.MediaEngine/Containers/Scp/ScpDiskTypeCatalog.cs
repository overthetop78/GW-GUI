using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>Résout le type SCP depuis l'identité et la géométrie centrales d'une image.</summary>
internal static class ScpDiskTypeCatalog
{
    public static ScpDiskType Resolve(SectorImage image)
    {
        var formatId = image.FormatId;
        if (formatId.Equals(DiskImageFormatIds.AmigaDosHighDensity, StringComparison.OrdinalIgnoreCase)) return ScpDiskType.AmigaHighDensity;
        if (formatId.StartsWith(DiskImageFormatIds.AmigaPrefix, StringComparison.OrdinalIgnoreCase)) return ScpDiskType.Amiga;
        if (formatId.Equals(DiskImageFormatIds.Atari90, StringComparison.OrdinalIgnoreCase)) return ScpDiskType.Atari8BitSingleDensity;
        if (formatId.Equals(DiskImageFormatIds.Atari130, StringComparison.OrdinalIgnoreCase)) return ScpDiskType.Atari8BitEnhancedDensity;
        if (formatId.StartsWith(DiskImageFormatIds.AtariPrefix, StringComparison.OrdinalIgnoreCase)) return ScpDiskType.Atari8BitDoubleDensity;
        if (formatId.StartsWith(DiskImageFormatIds.AtariStPrefix, StringComparison.OrdinalIgnoreCase)) return image.Heads == DiskGeometryConstants.SingleSidedHeadCount ? ScpDiskType.AtariStSingleSided : ScpDiskType.AtariStDoubleSided;
        if (IsIbmPc360Family(formatId)) return ScpDiskType.IbmPc360;
        if (formatId.Equals(DiskImageFormatIds.Ibm720, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.Ibm800, StringComparison.OrdinalIgnoreCase)) return ScpDiskType.IbmPc720;
        if (formatId.Equals(DiskImageFormatIds.Ibm1200, StringComparison.OrdinalIgnoreCase)) return ScpDiskType.IbmPc1200;
        if (IsIbmPc1440Family(formatId)) return ScpDiskType.IbmPc1440;
        if (formatId.Equals(DiskImageFormatIds.AmstradCpc, StringComparison.OrdinalIgnoreCase)) return ScpDiskType.AmstradCpc;
        return ResolveOther(image.Capacity);
    }

    private static bool IsIbmPc360Family(string formatId) => formatId.Equals(DiskImageFormatIds.Ibm160, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.Ibm180, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.Ibm320, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.Ibm360, StringComparison.OrdinalIgnoreCase);

    private static bool IsIbmPc1440Family(string formatId) => formatId.Equals(DiskImageFormatIds.Ibm1440, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.Ibm1680, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.IbmDmf, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.Ibm2880, StringComparison.OrdinalIgnoreCase);

    private static ScpDiskType ResolveOther(long capacity) => (capacity / DataSizeConstants.BytesPerKibibyte) switch
    {
        720 or 800 or 880 => ScpDiskType.Other720,
        1200 => ScpDiskType.Other1200,
        1440 => ScpDiskType.Other1440,
        _ => ScpDiskType.Other320
    };
}
