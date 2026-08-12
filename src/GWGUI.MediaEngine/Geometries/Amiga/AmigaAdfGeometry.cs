using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Reconstruction;

namespace GWGUI.MediaEngine.Geometries.Amiga;

/// <summary>Définit les géométries ADF Amiga double et haute densité.</summary>
public static class AmigaAdfGeometry
{
    /// <summary>Capacité double densité en octets.</summary>
    public const int DoubleDensityCapacity = AmigaMfmFormat.SectorByteCount * DiskGeometryConstants.EightyTrackCylinderCount * DiskGeometryConstants.DoubleSidedHeadCount * AmigaMfmFormat.DoubleDensitySectorsPerTrack;
    /// <summary>Capacité haute densité en octets.</summary>
    public const int HighDensityCapacity = AmigaMfmFormat.SectorByteCount * DiskGeometryConstants.EightyTrackCylinderCount * DiskGeometryConstants.DoubleSidedHeadCount * AmigaMfmFormat.HighDensitySectorsPerTrack;
    /// <summary>Géométrie Amiga double densité.</summary>
    public static RegularSectorGeometry DoubleDensity { get; } = new(DiskImageFormatIds.AmigaDos, AmigaMfmFormat.SectorByteCount, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, AmigaMfmFormat.DoubleDensitySectorsPerTrack);
    /// <summary>Géométrie Amiga haute densité.</summary>
    public static RegularSectorGeometry HighDensity { get; } = new(DiskImageFormatIds.AmigaDosHighDensity, AmigaMfmFormat.SectorByteCount, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, AmigaMfmFormat.HighDensitySectorsPerTrack);
}
