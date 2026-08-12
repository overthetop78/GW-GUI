namespace GWGUI.MediaEngine.Encoding;

/// <summary>Définit les durées utilisées lorsqu'une requête d'encodage ne fournit aucune valeur particulière.</summary>
internal static class TrackEncodingDefaults
{
    /// <summary>Durée par défaut d'une cellule binaire, en ticks.</summary>
    public const uint BitCellTicks = 40;
    /// <summary>Durée par défaut d'une révolution complète, en ticks.</summary>
    public const uint IndexTimeTicks = 8_000_000;
}
