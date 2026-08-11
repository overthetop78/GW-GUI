using GWGUI.MediaEngine.Flux;

namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Sélectionne le meilleur résultat parmi les tentatives PLL ISO MFM.</summary>
internal static class IsoMfmPllSelector
{
    /// <summary>Décode les facteurs dans leur ordre défini et conserve le premier meilleur score.</summary>
    public static FluxDecodeResult Select(FluxRevolution revolution, double centre, Func<FluxBitstream, FluxDecodeResult> decode)
    {
        FluxDecodeResult? best = null;
        var bestScore = int.MinValue;
        foreach (var factor in IsoMfmFormat.PllFactors)
        {
            var candidate = decode(FluxTransitionDecoder.DecodePll(revolution.FluxIntervals, centre * factor));
            if (factor == 1d && candidate.Sectors.Count > 0 && candidate.Sectors.All(sector => sector.Data is not null && sector.IntegrityValid == true)) return candidate;
            var score = Score(candidate);
            if (score <= bestScore) continue;
            best = candidate;
            bestScore = score;
        }
        return best!;
    }

    /// <summary>Calcule le score pondéré d'un résultat.</summary>
    public static int Score(FluxDecodeResult result) => result.Sectors.Count(sector => sector.IntegrityValid == true) * IsoMfmFormat.ValidSectorScoreWeight + result.Sectors.Count(sector => sector.Data is not null) * IsoMfmFormat.DataSectorScoreWeight + result.Sectors.Count * IsoMfmFormat.SectorScoreWeight;
}
