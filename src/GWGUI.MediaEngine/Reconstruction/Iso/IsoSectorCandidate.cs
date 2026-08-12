using GWGUI.MediaEngine.Decoding;

namespace GWGUI.MediaEngine.Reconstruction.Iso;

/// <summary>Associe un secteur ISO décodé à sa révolution et, si nécessaire, à sa piste source.</summary>
/// <param name="Sector">Secteur décodé.</param>
/// <param name="Revolution">Index de la révolution source dans une capture de flux.</param>
/// <param name="SourceTrack">Index de piste source lorsqu'il provient d'un conteneur déjà organisé en pistes.</param>
internal sealed record IsoSectorCandidate(DecodedSector Sector, int Revolution, int? SourceTrack = null);
