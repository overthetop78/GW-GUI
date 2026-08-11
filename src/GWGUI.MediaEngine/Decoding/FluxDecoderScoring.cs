namespace GWGUI.MediaEngine.Decoding;

/// <summary>Définit les poids utilisés pour comparer automatiquement les résultats des décodeurs de flux.</summary>
/// <remarks>Les scores sont sans unité. La confiance consommée par les calculs est normalisée entre zéro et un.</remarks>
internal static class FluxDecoderScoring
{
    public const double ValidSectorBaseScore = 4;
    public const double ValidSectorRatioWeight = 1;
    public const double ValidSectorConfidenceWeight = 0.1;
    public const double UnverifiedSectorBaseScore = 3;
    public const double InvalidSectorConfidenceWeight = 0.01;
    public const double RawFluxBaseScore = 1;
    public const double StructuredFluxBaseScore = 2;

    /// <summary>Calcule le score de sélection automatique d'un résultat.</summary>
    /// <param name="result">Résultat à évaluer.</param><returns>Score automatique.</returns>
    public static double Calculate(FluxDecodeResult result)
    {
        var sectors = result.Sectors ?? [];
        var valid = sectors.Count(sector => sector.IntegrityValid == true);
        var invalid = sectors.Count(sector => sector.IntegrityValid == false);
        if (valid > 0) return ValidSectorBaseScore + valid / (double)Math.Max(1, valid + invalid) * ValidSectorRatioWeight + result.Confidence * ValidSectorConfidenceWeight;
        if (sectors.Count > 0 && invalid == 0) return UnverifiedSectorBaseScore + result.Confidence;
        if (invalid > 0) return result.Confidence * InvalidSectorConfidenceWeight;
        if (result.DecoderId == FluxCodecIds.Raw) return RawFluxBaseScore + result.Confidence;
        if (result.Structures.Count > 0) return StructuredFluxBaseScore + result.Confidence;
        return result.Confidence;
    }

    /// <summary>Calcule le score lexicographique utilisé lorsqu'un décodeur est imposé.</summary>
    /// <param name="result">Résultat à évaluer.</param><returns>Composantes ordonnées du score.</returns>
    public static (int ValidSectors, int InvalidSectorPenalty, int SectorsWithData, double Confidence, int Structures) CalculateExplicit(FluxDecodeResult result)
    {
        var sectors = result.Sectors ?? [];
        return (sectors.Count(sector => sector.IntegrityValid == true), -sectors.Count(sector => sector.IntegrityValid == false), sectors.Count(sector => sector.Data is not null), result.Confidence, result.Structures.Count);
    }
}
