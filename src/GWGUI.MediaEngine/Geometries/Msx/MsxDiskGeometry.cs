namespace GWGUI.MediaEngine.Geometries.Msx;

/// <summary>Décrit une géométrie de disquette MSX-DOS et l'identifiant de format associé.</summary>
/// <param name="Capacity">Capacité totale de l'image, en octets.</param>
/// <param name="FormatId">Identifiant technique du format MSX-DOS.</param>
/// <param name="Cylinders">Nombre de cylindres de la géométrie.</param>
/// <param name="Heads">Nombre de faces de la géométrie.</param>
/// <param name="SectorsPerTrack">Nombre de secteurs présents sur chaque piste.</param>
public sealed record MsxDiskGeometry(int Capacity, string FormatId, int Cylinders, int Heads, int SectorsPerTrack);
