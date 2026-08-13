namespace GWGUI.MediaEngine.Containers.TeleDisk;

/// <summary>Conserve une piste TeleDisk et sa carte ordonnée de secteurs.</summary>
public sealed record Td0Track(byte Cylinder, byte Head, IReadOnlyList<Td0Sector> Sectors);
