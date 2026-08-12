namespace GWGUI.MediaEngine.FileSystems.Apple.Dos;

/// <summary>Contient les champs validés du VTOC Apple DOS.</summary>
public sealed record AppleDosVtoc(byte[] Data, int Tracks, int SectorsPerTrack, int CatalogTrack, int CatalogSector, byte VolumeNumber, int FreeSectorCount);
