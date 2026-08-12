namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Identifie le type d'anomalie temporelle d'un intervalle brut.</summary>
internal enum RawFluxAnomalyKind
{
    /// <summary>Aucune anomalie.</summary>
    None,
    /// <summary>Intervalle exceptionnellement long.</summary>
    LongInterval,
    /// <summary>Impulsion exceptionnellement courte.</summary>
    ShortPulse
}

/// <summary>Regroupe les définitions de l'analyse du flux brut.</summary>
internal static class RawFluxAnalysisDefinitions
{
    /// <summary>Identifiant technique de l'analyse brute.</summary>
    public const string CodecId = FluxCodecIds.Raw;
    /// <summary>Nom affiché de l'analyse brute.</summary>
    public const string CodecDisplayName = FluxCodecDisplayNames.Raw;
    /// <summary>Nom employé dans les descriptions de structures.</summary>
    public const string StructureDescriptionName = "Raw Flux";
    /// <summary>Confiance fixe attribuée au résultat brut.</summary>
    public const double Confidence = .05;
    /// <summary>Nombre minimal de cellules représentant un intervalle.</summary>
    public const int MinimumCellCount = 1;
    /// <summary>Nombre maximal de cellules représentant un intervalle.</summary>
    public const int MaximumCellCount = 64;
    /// <summary>Multiplicateur définissant un intervalle exceptionnellement long.</summary>
    public const double LongIntervalMultiplier = 10;
    /// <summary>Rapport définissant une impulsion exceptionnellement courte.</summary>
    public const double ShortPulseRatio = .55;
    /// <summary>Description d'un intervalle exceptionnellement long.</summary>
    public const string LongIntervalDescription = "exceptionally long flux interval";
    /// <summary>Description d'une impulsion exceptionnellement courte.</summary>
    public const string ShortPulseDescription = "exceptionally short flux pulse";
}
