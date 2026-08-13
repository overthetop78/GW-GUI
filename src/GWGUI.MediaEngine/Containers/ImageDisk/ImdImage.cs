using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.ImageDisk;

/// <summary>Réunit le commentaire, les pistes ImageDisk et leur représentation sectorielle.</summary>
public sealed record ImdImage(string Comment, IReadOnlyList<ImdTrack> Tracks, SectorImage SectorImage);
