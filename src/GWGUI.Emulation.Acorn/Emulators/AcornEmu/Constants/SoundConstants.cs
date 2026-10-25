namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;

/// <summary>Paramètres du SN76489AN : division d'horloge, périodes et atténuation logarithmique.</summary>
internal static class SoundConstants
{
    /// <summary>Un compteur de ton avance toutes les seize périodes de l'horloge d'entrée.</summary>
    internal const int ClockDivider = 16;
    /// <summary>Sur la variante TI, un registre de période nul représente 1024, et non un ton arrêté.</summary>
    internal const int ZeroPeriod = 1024;
    /// <summary>État initial du registre de bruit de la variante A, avec les étages de sortie (référence MAME).</summary>
    internal const int NoiseSeed = 0x10000;
    /// <summary>Chaque voie reçoit au plus le quart de l'amplitude PCM, pour éviter l'écrêtage du mélange.</summary>
    internal static readonly IReadOnlyList<int> Amplitudes = Array.AsReadOnly(Enumerable.Range(0, 16).Select(value => value == 15 ? 0 : (int)Math.Round(short.MaxValue / 4.0 * Math.Pow(10, -value / 10.0))).ToArray());
}
