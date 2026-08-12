using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Reconstruction.EpsonQx10;

namespace GWGUI.MediaEngine.Geometries.Epson;

/// <summary>Répertorie les dispositions de pistes et géométries Epson QX-10 prises en charge.</summary>
internal static class EpsonQx10GeometryCatalog
{
    /// <summary>Nombre maximal de pistes d'une image booter.</summary>
    public const int BooterTrackCount = 15;
    /// <summary>Nombre de petites pistes propre au format 399 Kio.</summary>
    public const int Format399SmallTrackCount = 1;
    /// <summary>Nombre minimal de petites pistes propre au format 396 Kio.</summary>
    public const int Format396MinimumSmallTrackCount = 4;
    /// <summary>Nombre minimal de petites pistes propre au format LOGO.</summary>
    public const int LogoMinimumSmallTrackCount = 6;
    /// <summary>Disposition d'une piste 320 Kio ou petite piste.</summary>
    public static EpsonQx10TrackGeometry Layout320 { get; } = new(1, 16, 256);
    /// <summary>Disposition d'une piste 400 Kio.</summary>
    public static EpsonQx10TrackGeometry Layout400 { get; } = new(1, 5, 1024);
    /// <summary>Disposition des pistes de données d'une image booter.</summary>
    public static EpsonQx10TrackGeometry LayoutBooterData { get; } = new(1, 17, 256);
    /// <summary>Disposition commune des pistes de données de 512 octets.</summary>
    public static EpsonQx10TrackGeometry LayoutData { get; } = new(1, 10, 512);
    /// <summary>Disposition alternative des pistes LOGO.</summary>
    public static EpsonQx10TrackGeometry LayoutLogoAlternate { get; } = new(2, 10, 512);

    /// <summary>Résout la géométrie correspondant exactement à l'identifiant Epson demandé.</summary>
    public static EpsonQx10Geometry Resolve(string formatId) => formatId.ToLowerInvariant() switch
    {
        DiskImageFormatIds.EpsonQx10_320 => EpsonQx10Geometry.Uniform(DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, Layout320),
        DiskImageFormatIds.EpsonQx10_400 => EpsonQx10Geometry.Uniform(DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, Layout400),
        DiskImageFormatIds.EpsonQx10Booter => new(BooterTrackCount, DiskGeometryConstants.SingleSidedHeadCount, (cylinder, _) => cylinder == 0 ? Layout320 : LayoutBooterData),
        DiskImageFormatIds.EpsonQx10_399 => new(DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, (cylinder, head) => cylinder == 0 && head == 0 ? Layout320 : LayoutData),
        DiskImageFormatIds.EpsonQx10Logo => new(DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, (cylinder, _) => cylinder switch
        {
            0 or 1 or 4 => Layout320,
            5 or 6 => LayoutLogoAlternate,
            3 or 7 => default,
            _ => LayoutData
        }),
        DiskImageFormatIds.EpsonQx10_396 => new(DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, (cylinder, _) => cylinder <= 1 ? Layout320 : LayoutData),
        _ => throw EpsonQx10Exceptions.InvalidFormat(formatId)
    };
}

/// <summary>Décrit la numérotation, le nombre et la taille des secteurs d'une piste Epson.</summary>
internal readonly record struct EpsonQx10TrackGeometry(int FirstSector, int Count, int SectorSize);

/// <summary>Décrit une géométrie Epson QX-10 dont la disposition peut varier par piste.</summary>
internal sealed record EpsonQx10Geometry(int Cylinders, int Heads, Func<int, int, EpsonQx10TrackGeometry> Track)
{
    /// <summary>Énumère les dispositions de toutes les pistes dans l'ordre cylindre-face.</summary>
    public IEnumerable<EpsonQx10TrackGeometry> AllTracks
    {
        get
        {
            for (var cylinder = 0; cylinder < Cylinders; cylinder++)
                for (var head = 0; head < Heads; head++)
                    yield return Track(cylinder, head);
        }
    }

    /// <summary>Crée une géométrie utilisant la même disposition sur toutes les pistes.</summary>
    public static EpsonQx10Geometry Uniform(int cylinders, int heads, EpsonQx10TrackGeometry track) => new(cylinders, heads, (_, _) => track);
}
