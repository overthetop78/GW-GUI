
using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.ImageDisk;

/// <summary>Réunit le commentaire, les pistes ImageDisk et leur représentation sectorielle.</summary>
public sealed record ImdImage(string Comment, IReadOnlyList<ImdTrack> Tracks, SectorImage SectorImage);
