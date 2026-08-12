using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Macintosh;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.Recognition.Apple;
using GWGUI.MediaEngine.SectorImages.Builders;
using GWGUI.MediaEngine.SectorImages.Builders.Apple;

namespace GWGUI.MediaEngine.Containers.Apple.Raw;

/// <summary>Lit les représentations Apple 3,5 pouces Lisa, Macintosh GCR, ProDOS et Macintosh MFM.</summary>
internal static class Apple35RawImageReader
{
    /// <summary>Géométrie linéaire Macintosh MFM de 1,44 Mio.</summary>
    private static readonly LinearSectorImageGeometry MacintoshMfmGeometry = new(MacintoshMfm1440Geometry.SectorSize, MacintoshMfm1440Geometry.CylinderCount, MacintoshMfm1440Geometry.HeadCount, MacintoshMfm1440Geometry.SectorsPerTrack);

    /// <summary>Applique les sondes Lisa, Macintosh et ProDOS à la disposition de capacité déjà validée.</summary>
    public static AppleRawImageReadResult Read(ReadOnlyMemory<byte> data, string extension, AppleRawImageLayout layout)
    {
        if (layout == AppleRawImageLayoutCatalog.Apple400K && AppleRawImageProbe.LooksLikeLisaOffice(data.Span)) return new(MacintoshGcrSectorImageBuilder.Create(data, DiskImageFormatIds.AppleLisaRaw, MacintoshGcrGeometry.ForHeads(MacintoshGcrGeometry.SingleSidedHeadCount)), AppleRawImageMatchKind.ValidatedStructure);
        var macintosh = AppleRawImageProbe.ProbeMac(data.Span);
        if (macintosh is not null) return ReadMacintosh(data, layout, macintosh.Value);
        if (layout == AppleRawImageLayoutCatalog.Apple800K && AppleRawImageProbe.LooksLikeProDos(data.Span)) return new(MacintoshGcrSectorImageBuilder.Create(data, DiskImageFormatIds.AppleIIProDos, MacintoshGcrGeometry.ForHeads(MacintoshGcrGeometry.DoubleSidedHeadCount)), AppleRawImageMatchKind.ValidatedStructure);
        if (layout == AppleRawImageLayoutCatalog.Macintosh1440K && AppleRawImageProbe.LooksLikeProDos(data.Span)) return new(LinearSectorImageBuilder.Create(data, DiskImageFormatIds.AppleIIProDos, MacintoshMfmGeometry), AppleRawImageMatchKind.ValidatedStructure);
        throw AppleRawImageExceptions.KnownCapacityWithoutStructure(data.Length, extension, ["Lisa", "MFS", "HFS", "ProDOS"]);
    }

    /// <summary>Construit une image Macintosh GCR zonée pour 400/800 Kio ou une image MFM linéaire pour 1,44 Mio.</summary>
    private static AppleRawImageReadResult ReadMacintosh(ReadOnlyMemory<byte> data, AppleRawImageLayout layout, MacintoshFileSystemKind fileSystem)
    {
        if (layout == AppleRawImageLayoutCatalog.Macintosh1440K) return new(LinearSectorImageBuilder.Create(data, DiskImageFormatIds.Mac1440, MacintoshMfmGeometry), AppleRawImageMatchKind.ValidatedStructure);
        var formatId = fileSystem == MacintoshFileSystemKind.Mfs ? DiskImageFormatIds.AppleMacMfs : DiskImageFormatIds.AppleMacHfs;
        var heads = layout == AppleRawImageLayoutCatalog.Apple400K ? MacintoshGcrGeometry.SingleSidedHeadCount : MacintoshGcrGeometry.DoubleSidedHeadCount;
        return new(MacintoshGcrSectorImageBuilder.Create(data, formatId, MacintoshGcrGeometry.ForHeads(heads)), AppleRawImageMatchKind.ValidatedStructure);
    }
}
