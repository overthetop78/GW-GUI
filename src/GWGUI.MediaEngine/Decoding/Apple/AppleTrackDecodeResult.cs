namespace GWGUI.MediaEngine.Decoding.Apple;

/// <summary>Contient les deux familles de secteurs décodées depuis une même piste Apple et leurs scores.</summary>
/// <param name="StandardSectors">Secteurs Apple II standards retenus.</param>
/// <param name="StandardScore">Score des secteurs Apple II standards.</param>
/// <param name="Rwts18Sectors">Secteurs RWTS18 retenus.</param>
/// <param name="Rwts18Score">Score des secteurs RWTS18.</param>
internal sealed record AppleTrackDecodeResult(IReadOnlyList<DecodedSector> StandardSectors, int StandardScore, IReadOnlyList<DecodedSector> Rwts18Sectors, int Rwts18Score);
