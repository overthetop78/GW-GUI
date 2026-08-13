namespace GWGUI.MediaEngine.Containers.ImageDisk;

/// <summary>Conserve l'adresse, la taille, l'état et les données développées d'un secteur ImageDisk.</summary>
public sealed record ImdSector(byte Cylinder, byte Head, byte Number, int Size, ImdSectorRecordType RecordType, IReadOnlyList<byte> Data);
