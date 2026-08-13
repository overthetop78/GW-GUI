namespace GWGUI.MediaEngine.TrackImages;

/// <summary>Identifie une caractéristique physique ou logique localisée dans une piste protégée.</summary>
public enum TrackFeatureKind
{
    /// <summary>Marque d'index ou de synchronisation.</summary>
    IndexMark,
    /// <summary>Zone de remplissage entre structures.</summary>
    Gap,
    /// <summary>Erreur de contrôle intentionnelle.</summary>
    IntentionalChecksumError,
    /// <summary>Zone faible dont la lecture peut varier.</summary>
    WeakRegion
}
