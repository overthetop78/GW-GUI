
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.Formats.Floppy.CpcDsk;

/// <summary>Réunit la représentation exacte du conteneur CPCEMU et son image sectorielle exploitable.</summary>
public sealed record CpcDskImage(CpcDskContainerKind Kind, byte Cylinders, byte Heads, IReadOnlyList<CpcDskTrack> Tracks, SectorImage SectorImage);
