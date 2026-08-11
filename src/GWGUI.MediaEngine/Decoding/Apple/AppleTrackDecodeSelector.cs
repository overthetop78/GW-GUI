namespace GWGUI.MediaEngine.Decoding.Apple;

/// <summary>Décode et classe une piste avec les codecs Apple II standard et RWTS18.</summary>
internal sealed class AppleTrackDecodeSelector
{
    /// <summary>Décodeur des pistes Apple II standards.</summary>
    private readonly AppleIIGcrDecoder standardDecoder = new();
    /// <summary>Décodeur des pistes Apple II RWTS18.</summary>
    private readonly AppleRwts18Decoder rwts18Decoder = new();

    /// <summary>Retourne les secteurs compatibles avec la piste attendue et le score de chaque famille.</summary>
    /// <param name="bits">Bits de la piste à décoder.</param>
    /// <param name="track">Numéro de la piste attendue.</param>
    /// <returns>Candidats Apple II standard et RWTS18 accompagnés de leurs scores.</returns>
    public AppleTrackDecodeResult Decode(bool[] bits, int track)
    {
        var standard = standardDecoder.DecodeBits(bits).Sectors.Where(sector => sector.Cylinder == track && sector.Number is >= AppleTrackSelectionRules.StandardMinimumSectorNumber and <= AppleTrackSelectionRules.StandardMaximumSectorNumber && sector.Data is { Count: AppleTrackSelectionRules.StandardSectorSize }).ToArray();
        var rwts18 = rwts18Decoder.DecodeBits(bits).Sectors.Where(sector => sector.Cylinder == track && sector.Number is >= AppleTrackSelectionRules.Rwts18MinimumSectorNumber and <= AppleTrackSelectionRules.Rwts18MaximumSectorNumber && sector.Data is { Count: AppleTrackSelectionRules.Rwts18SectorSize }).ToArray();
        return new(standard, Score(standard), rwts18, Score(rwts18));
    }

    /// <summary>Calcule le score d'un ensemble de secteurs selon leur diversité et leur intégrité.</summary>
    /// <param name="sectors">Secteurs dont le score doit être calculé.</param>
    /// <returns>Score donnant la priorité aux secteurs distincts puis à leur intégrité.</returns>
    private static int Score(IReadOnlyList<DecodedSector> sectors) => sectors.Select(sector => sector.Number).Distinct().Count() * AppleTrackSelectionRules.DistinctSectorScoreWeight + sectors.Count(sector => sector.IntegrityValid == true) * AppleTrackSelectionRules.IntegrityScoreWeight + sectors.Count;
}
