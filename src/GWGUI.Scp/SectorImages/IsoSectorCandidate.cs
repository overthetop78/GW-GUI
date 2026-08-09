using GWGUI.Scp.Decoding;

namespace GWGUI.Scp.SectorImages;

internal sealed record IsoSectorCandidate(DecodedSector Sector, int Revolution);
