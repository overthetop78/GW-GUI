namespace GWGUI.MediaEngine.Containers.TeleDisk;

/// <summary>Conserve l'adresse, la taille, les indicateurs et les données d'un secteur TeleDisk.</summary>
public sealed record Td0Sector(byte Cylinder, byte Head, byte Number, byte SizeCode, byte Flags, IReadOnlyList<byte>? Data);
