using GWGUI.MediaEngine.Geometries.Atari;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Conversion.Atari;

/// <summary>Construit les erreurs propres à la conversion sectorielle Atari ST.</summary>
internal static class AtariStConversionExceptions
{
    /// <summary>Signale un format cible absent du catalogue Atari ST.</summary>
    public static InvalidDataException UnsupportedTargetFormat(string formatId) => new($"Atari ST target format '{formatId}' is not catalogued.");
    /// <summary>Signale une transformation qui modifierait la capacité ou supprimerait des secteurs.</summary>
    public static InvalidDataException LossyGeometryChange(SectorImage source, AtariStGeometry target) => new($"Atari ST image {source.FormatId} ({source.Cylinders}x{source.Heads}x{source.SectorsPerTrack}, {source.Capacity} bytes) cannot be transformed without loss to {target.FormatId} ({target.Cylinders}x{target.Heads}x{target.SectorsPerTrack}, {target.Capacity} bytes).");
    /// <summary>Signale un secteur nécessaire absent de la source.</summary>
    public static InvalidDataException MissingSourceSector(int logicalSector) => new($"Atari ST source sector {logicalSector} is missing; geometry conversion would require inventing its contents.");
}
