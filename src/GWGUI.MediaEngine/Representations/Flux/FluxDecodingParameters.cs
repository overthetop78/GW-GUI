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
}
