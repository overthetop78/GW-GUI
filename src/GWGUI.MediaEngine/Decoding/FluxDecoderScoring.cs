namespace GWGUI.MediaEngine.Decoding;

/// <summary>Définit les poids utilisés pour comparer automatiquement les résultats des décodeurs de flux.</summary>
internal static class FluxDecoderScoring
{
    public const double ValidSectorBaseScore = 4;
    public const double ValidSectorRatioWeight = 1;
    public const double ValidSectorConfidenceWeight = 0.1;
    public const double UnverifiedSectorBaseScore = 3;
    public const double InvalidSectorConfidenceWeight = 0.01;
    public const double RawFluxBaseScore = 1;
    public const double StructuredFluxBaseScore = 2;

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
}
