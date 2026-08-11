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
}
