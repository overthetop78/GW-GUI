using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Atari;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Visualization;

namespace GWGUI.MediaEngine.Visualization.Policies;

/// <summary>Détermine l'encodage et l'adressage de visualisation des images Atari.</summary>
internal sealed class AtariVisualizationPolicy : SectorImageVisualizationPolicy
{
    /// <inheritdoc />
    public override bool CanHandle(SectorImage image) => image.FormatId.StartsWith(DiskImageFormatIds.AtariPrefix, StringComparison.OrdinalIgnoreCase) || image.FormatId.StartsWith(DiskImageFormatIds.AtariStPrefix, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override string EncoderId(SectorImage image) => image.FormatId.Equals(DiskImageFormatIds.Atari90, StringComparison.OrdinalIgnoreCase) ? FluxCodecIds.IsoFm : FluxCodecIds.IsoMfm;

    /// <inheritdoc />
    public override SectorAddress VisualAddress(SectorImage image, SectorAddress address)
    {
        if (!image.FormatId.StartsWith(DiskImageFormatIds.AtariPrefix, StringComparison.OrdinalIgnoreCase) ||
            image.SectorsPerTrack != Atari8BitGeometry.LinearSectorsPerCylinder || image.Cylinders <= DiskGeometryConstants.EightyTrackCylinderCount) return address;
        var sectorsPerTrack = image.FormatId.Equals(DiskImageFormatIds.Atari130, StringComparison.OrdinalIgnoreCase) ? Atari8BitGeometry.EnhancedSectorsPerTrack : Atari8BitGeometry.StandardSectorsPerTrack;
        var logical = address.Cylinder;
        return new(logical / sectorsPerTrack, Atari8BitGeometry.FirstHead, logical % sectorsPerTrack + Atari8BitGeometry.FirstSectorNumber);
    }
}
