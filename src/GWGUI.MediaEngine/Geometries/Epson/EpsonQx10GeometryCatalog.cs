using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Reconstruction.EpsonQx10;
using System.Collections.ObjectModel;

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

    /// <summary>Géométrie uniforme de 320 Kio.</summary>
    public static EpsonQx10Geometry Geometry320 { get; } = EpsonQx10Geometry.Uniform(DiskImageFormatIds.EpsonQx10_320, DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, Layout320);
    /// <summary>Géométrie uniforme de 400 Kio.</summary>
    public static EpsonQx10Geometry Geometry400 { get; } = EpsonQx10Geometry.Uniform(DiskImageFormatIds.EpsonQx10_400, DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, Layout400);
    /// <summary>Géométrie de l'image d'amorçage.</summary>
    public static EpsonQx10Geometry GeometryBooter { get; } = new(DiskImageFormatIds.EpsonQx10Booter, BooterTrackCount, DiskGeometryConstants.SingleSidedHeadCount, (cylinder, _) => cylinder == 0 ? Layout320 : LayoutBooterData);
    /// <summary>Géométrie variable de 399 Kio.</summary>
    public static EpsonQx10Geometry Geometry399 { get; } = new(DiskImageFormatIds.EpsonQx10_399, DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, (cylinder, head) => cylinder == 0 && head == 0 ? Layout320 : LayoutData);
    /// <summary>Géométrie variable LOGO.</summary>
    public static EpsonQx10Geometry GeometryLogo { get; } = new(DiskImageFormatIds.EpsonQx10Logo, DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, (cylinder, _) => cylinder switch { 0 or 1 or 4 => Layout320, 5 or 6 => LayoutLogoAlternate, 3 or 7 => default, _ => LayoutData });
    /// <summary>Géométrie variable de 396 Kio.</summary>
    public static EpsonQx10Geometry Geometry396 { get; } = new(DiskImageFormatIds.EpsonQx10_396, DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, (cylinder, _) => cylinder <= 1 ? Layout320 : LayoutData);
    /// <summary>Collection non modifiable de toutes les géométries Epson cataloguées.</summary>
    public static IReadOnlyDictionary<string, EpsonQx10Geometry> All { get; } = new ReadOnlyDictionary<string, EpsonQx10Geometry>(new[] { Geometry320, Geometry400, GeometryBooter, Geometry399, GeometryLogo, Geometry396 }.ToDictionary(geometry => geometry.FormatId, StringComparer.OrdinalIgnoreCase));
    /// <summary>Formats essayés lors de la reconstruction automatique d'une capture SCP.</summary>
    public static IReadOnlyList<string> ScpCandidateFormatIds { get; } = Array.AsReadOnly(new[] { DiskImageFormatIds.EpsonQx10_396, DiskImageFormatIds.EpsonQx10_399, DiskImageFormatIds.EpsonQx10_320, DiskImageFormatIds.EpsonQx10_400, DiskImageFormatIds.EpsonQx10Logo });

    /// <summary>Valide que deux entrées du catalogue ne décrivent pas la même géométrie.</summary>
    static EpsonQx10GeometryCatalog()
    {
        var duplicate = All.Values.GroupBy(Signature).FirstOrDefault(group => group.Skip(1).Any());
        if (duplicate is not null) throw new InvalidOperationException($"Duplicate Epson QX-10 geometry: {string.Join(", ", duplicate.Select(geometry => geometry.FormatId))}.");
    }

    /// <summary>Résout la géométrie correspondant exactement à l'identifiant Epson demandé.</summary>
    /// <param name="formatId">Identifiant central de la disposition Epson.</param>
    /// <returns>La géométrie variable ou uniforme associée à l'identifiant.</returns>
    /// <exception cref="ArgumentException">L'identifiant ne correspond à aucune disposition Epson prise en charge.</exception>
    public static EpsonQx10Geometry Resolve(string formatId) => All.TryGetValue(formatId, out var geometry) ? geometry : throw EpsonQx10Exceptions.InvalidFormat(formatId);

    /// <summary>Construit une signature stable depuis toutes les pistes d'une géométrie.</summary>
    private static string Signature(EpsonQx10Geometry geometry) => $"{geometry.Cylinders}:{geometry.Heads}:{string.Join(';', geometry.AllTracks.Select(track => $"{track.FirstSector},{track.Count},{track.SectorSize}"))}";
}

/// <summary>Décrit la numérotation, le nombre et la taille des secteurs d'une piste Epson.</summary>
/// <param name="FirstSector">Premier numéro de secteur de la piste.</param>
/// <param name="Count">Nombre de secteurs de la piste.</param>
/// <param name="SectorSize">Taille de chaque secteur, en octets.</param>
internal readonly record struct EpsonQx10TrackGeometry(int FirstSector, int Count, int SectorSize);

/// <summary>Décrit une géométrie Epson QX-10 dont la disposition peut varier par piste.</summary>
/// <param name="Cylinders">Nombre de cylindres.</param>
/// <param name="Heads">Nombre de faces.</param>
/// <param name="Track">Fonction retournant la disposition d'un cylindre et d'une face.</param>
internal sealed record EpsonQx10Geometry(string FormatId, int Cylinders, int Heads, Func<int, int, EpsonQx10TrackGeometry> Track)
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
    /// <param name="cylinders">Nombre de cylindres.</param>
    /// <param name="heads">Nombre de faces.</param>
    /// <param name="track">Disposition répétée sur chaque piste.</param>
    /// <returns>La géométrie uniforme demandée.</returns>
    public static EpsonQx10Geometry Uniform(string formatId, int cylinders, int heads, EpsonQx10TrackGeometry track) => new(formatId, cylinders, heads, (_, _) => track);
}
