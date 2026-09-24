
using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.CpcDsk;

/// <summary>Réunit la représentation exacte du conteneur CPCEMU et son image sectorielle exploitable.</summary>
public sealed record CpcDskImage(CpcDskContainerKind Kind, byte Cylinders, byte Heads, IReadOnlyList<CpcDskTrack> Tracks, SectorImage SectorImage);
