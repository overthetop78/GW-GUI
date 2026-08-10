namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>
/// Définit les largeurs d'entrée de cellule de bit SCP prises en charge par le lecteur.
/// </summary>
public enum ScpBitCellEncoding : byte
{
    /// <summary>
    /// Utilise la valeur historique zéro, qui représente des entrées de seize bits.
    /// </summary>
    Default16Bit = 0,

    /// <summary>
    /// Déclare explicitement des entrées de seize bits.
    /// </summary>
    Explicit16Bit = 16
}
