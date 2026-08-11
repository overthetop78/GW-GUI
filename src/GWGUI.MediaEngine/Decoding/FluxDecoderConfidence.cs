namespace GWGUI.MediaEngine.Decoding;

/// <summary>Calcule les indices de confiance communs aux décodeurs de flux.</summary>
internal static class FluxDecoderConfidence
{
    private const int SectorWeight = 2;
    private const double StandardDivisor = 20;
    private const double MaximumConfidence = 1;

    /// <summary>Calcule la confiance standard à partir des nombres de secteurs et de structures reconnus.</summary>
    public static double CalculateStandard(int sectorCount, int structureCount) => Calculate(sectorCount, structureCount, SectorWeight, StandardDivisor);

    /// <summary>Calcule la confiance à partir des nombres de secteurs et de structures reconnus et des pondérations du décodeur.</summary>
    public static double Calculate(int sectorCount, int structureCount, int sectorWeight, double divisor) => Math.Min(MaximumConfidence, (sectorCount * sectorWeight + structureCount) / divisor);

    /// <summary>Calcule la confiance à partir des secteurs valides et de l'ensemble des secteurs détectés.</summary>
    public static double CalculateByValidity(int validSectorCount, int detectedSectorCount, double validDivisor, double detectedDivisor) => detectedSectorCount == 0 ? 0 : Math.Min(MaximumConfidence, validSectorCount / validDivisor + detectedSectorCount / detectedDivisor);
}
