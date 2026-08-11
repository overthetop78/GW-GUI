namespace GWGUI.MediaEngine.Representations.Flux;

/// <summary>Regroupe les paramètres fixes utilisés pour reconstruire et lire les transitions de flux.</summary>
internal static class FluxDecodingParameters
{
    /// <summary>Durée minimale acceptée pour une cellule de bit, en ticks.</summary>
    public const double MinimumBitCellTicks = 1d;
    /// <summary>Nombre maximal de cellules représentées par un intervalle FM ou MFM.</summary>
    public const int MaximumFmMfmCellsPerInterval = 32;
    /// <summary>Nombre maximal de cellules représentées par un intervalle NRZI ou NRZI doublé.</summary>
    public const int MaximumNrziCellsPerInterval = 64;
    /// <summary>Nombre de bits réservés initialement par intervalle pendant la reconstruction.</summary>
    public const int EstimatedBitsPerInterval = 4;
    /// <summary>Rapport minimal entre la cellule observée et l'horloge courante pour accepter un échantillon.</summary>
    public const double MinimumAcceptedSampleRatio = 0.7d;
    /// <summary>Rapport maximal entre la cellule observée et l'horloge courante pour accepter un échantillon.</summary>
    public const double MaximumAcceptedSampleRatio = 1.3d;
    /// <summary>Part de l'écart observé appliquée à l'horloge lors de chaque adaptation.</summary>
    public const double ClockAdaptationCoefficient = 0.08d;
    /// <summary>Rapport minimal entre l'horloge PLL et son centre.</summary>
    public const double MinimumPllClockRatio = 0.9d;
    /// <summary>Rapport maximal entre l'horloge PLL et son centre.</summary>
    public const double MaximumPllClockRatio = 1.1d;
    /// <summary>Fraction d'un cycle utilisée pour détecter le prochain bit.</summary>
    public const double HalfCycle = 0.5d;
    /// <summary>Fraction de la phase résiduelle conservée après une transition.</summary>
    public const double PllPhaseRetention = 0.4d;
    /// <summary>Coefficient appliqué à chaque correction de l'horloge PLL.</summary>
    public const double PllCorrectionCoefficient = 0.05d;
    /// <summary>Nombre maximal de zéros autorisant une correction directe de l'horloge PLL.</summary>
    public const int MaximumZerosForDirectPllCorrection = 3;
    /// <summary>Diviseur du percentile bas utilisé par les estimations FM et NRZI.</summary>
    public const int LowPercentileDivisor = 50;
    /// <summary>Diviseur sélectionnant le cinquième inférieur des intervalles pour l'estimation non-FM.</summary>
    public const int LowerClusterDivisor = 5;
    /// <summary>Diviseur convertissant l'intervalle robuste en durée de cellule non-FM.</summary>
    public const double RobustIntervalToBitCellDivisor = 2d;
    /// <summary>Durée de cellule utilisée lorsque l'estimation ne trouve aucun intervalle positif, en ticks.</summary>
    public const double FallbackBitCellTicks = MinimumBitCellTicks;
    /// <summary>Nombre de bits de données lus pour produire un octet.</summary>
    public const int BitsPerByte = 8;
    /// <summary>Nombre de bits comparés par la recherche d'un motif <see cref="ushort"/>.</summary>
    public const int UshortPatternBitCount = 16;
    /// <summary>Nombre maximal de bits comparés par la recherche d'un motif <see cref="uint"/>.</summary>
    public const int MaximumUintPatternBitCount = 32;
    /// <summary>Nombre de cellules MFM parcourues pour décoder un bit de données.</summary>
    public const int MfmCellsPerDataBit = 2;
    /// <summary>Nombre de cellules FM parcourues pour décoder un bit de données avec <c>DecodeFmByte32</c>.</summary>
    public const int FmCellsPerDataBit = 4;
}
