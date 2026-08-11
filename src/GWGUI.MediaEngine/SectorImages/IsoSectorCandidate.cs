using GWGUI.MediaEngine.Decoding;

namespace GWGUI.MediaEngine.SectorImages;

internal sealed record IsoSectorCandidate(DecodedSector Sector, int Revolution, int? SourceTrack = null);
