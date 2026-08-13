namespace GWGUI.MediaEngine.Containers.ImageDisk;

/// <summary>Conserve l'en-tête et les cartes d'une piste ImageDisk.</summary>
public sealed record ImdTrack(ImdMode Mode, byte Cylinder, byte Head, IReadOnlyList<ImdSector> Sectors);
