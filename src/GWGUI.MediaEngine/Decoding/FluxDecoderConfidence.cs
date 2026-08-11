namespace GWGUI.MediaEngine.Decoding;

/// <summary>Calcule les indices de confiance communs aux décodeurs de flux.</summary>
internal static class FluxDecoderConfidence
{
    private const int SectorWeight = 2;
    private const double StandardDivisor = 20;
    private const double MaximumConfidence = 1;

    /// <summary>Calcule la confiance standard à partir des nombres de secteurs et de structures reconnus.</summary>
    public static double CalculateStandard(int sectorCount, int structureCount) => Math.Min(MaximumConfidence, (sectorCount * SectorWeight + structureCount) / StandardDivisor);
}
