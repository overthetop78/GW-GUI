namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>
/// Définit les faces dont les pistes sont présentes dans un conteneur SCP.
/// </summary>
public enum ScpHeadSelection : byte
{
    /// <summary>
    /// Indique que les deux faces sont présentes.
    /// </summary>
    Both = 0,

    /// <summary>
    /// Indique que seule la face zéro, située en dessous, est présente.
    /// </summary>
    Side0 = 1,

    /// <summary>
    /// Indique que seule la face un, située au-dessus, est présente.
    /// </summary>
    Side1 = 2
}
