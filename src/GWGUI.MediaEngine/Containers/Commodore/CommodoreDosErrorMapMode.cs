namespace GWGUI.MediaEngine.Containers.Commodore;

/// <summary>Détermine si une conversion D64 ou D71 conserve une carte de diagnostics sectoriels.</summary>
public enum CommodoreDosErrorMapMode
{
    /// <summary>Écrit uniquement les secteurs de données.</summary>
    None,
    /// <summary>Exige et écrit un code de diagnostic pour chaque secteur.</summary>
    Preserve
}
