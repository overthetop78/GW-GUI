
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.Formats.Floppy.ImageDisk;

/// <summary>Réunit le commentaire, les pistes ImageDisk et leur représentation sectorielle.</summary>
public sealed record ImdImage(string Comment, IReadOnlyList<ImdTrack> Tracks, SectorImage SectorImage);
