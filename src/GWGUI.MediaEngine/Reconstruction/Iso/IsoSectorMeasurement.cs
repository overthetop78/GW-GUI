namespace GWGUI.MediaEngine.Reconstruction.Iso;

/// <summary>Regroupe la géométrie et la numérotation majoritaires mesurées dans des candidats ISO.</summary>
/// <param name="SectorSize">Taille sectorielle majoritaire, en octets.</param>
/// <param name="Cylinders">Nombre de cylindres observé.</param>
/// <param name="Heads">Nombre de faces observé.</param>
/// <param name="SectorsPerTrack">Nombre majoritaire de secteurs distincts par piste.</param>
/// <param name="SectorOrder">Numéros de secteurs distincts dans l'ordre croissant.</param>
/// <param name="ZeroBased">Indique si la numérotation commence à zéro.</param>
internal sealed record IsoSectorMeasurement(int SectorSize, int Cylinders, int Heads, int SectorsPerTrack, int[] SectorOrder, bool ZeroBased);
