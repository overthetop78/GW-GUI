namespace GWGUI.MediaEngine.Decoding;

internal static class FluxDecoderScoring
{
    public const double ValidSectorBaseScore = 4;
    public const double ValidSectorConfidenceWeight = 0.1;
    public const double UnverifiedSectorBaseScore = 3;
    public const double InvalidSectorConfidenceWeight = 0.01;
    public const double RawFluxBaseScore = 1;
    public const double StructuredFluxBaseScore = 2;
}
