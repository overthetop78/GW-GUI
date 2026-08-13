namespace GWGUI.MediaEngine.Geometries.Ibm;

/// <summary>Décrit une géométrie sectorielle IBM PC.</summary>
/// <param name="FormatId">Identifiant central du format.</param>
/// <param name="Cylinders">Nombre de cylindres.</param>
/// <param name="Heads">Nombre de têtes.</param>
/// <param name="SectorsPerTrack">Nombre de secteurs par piste.</param>
public readonly record struct IbmPcGeometry(string FormatId, int Cylinders, int Heads, int SectorsPerTrack)
{
    /// <summary>Capacité brute du profil en octets.</summary>
    public int Capacity => checked(Cylinders * Heads * SectorsPerTrack * FileSystems.Fat12.FatBootSectorLayout.SectorSize);
}
