namespace GWGUI.MediaEngine.Representations.Flux;

/// <summary>Définit le mode utilisé pour estimer la durée d'une cellule de flux.</summary>
internal enum FluxTimingMode
{
    /// <summary>Estime la durée sans appliquer la distribution propre au codage FM.</summary>
    NonFm,
    /// <summary>Estime la durée à partir de la distribution propre au codage FM.</summary>
    Fm
}
