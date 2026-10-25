namespace GWGUI.Emulation.Constants;

/// <summary>Format unique des sauvegardes d’état écrites et lues par tous les modules.</summary>
public static class EmulationSavedStateConstants
{
    /// <summary>Version de l’en-tête complet ; les anciens formats propres aux familles ne sont pas acceptés.</summary>
    public const int CurrentFormatVersion = 1;
    /// <summary>Marqueur commun de fichier, indépendant de la famille et de l’émulateur.</summary>
    public static ReadOnlySpan<byte> Magic => "GWGUISTATE1"u8;
    /// <summary>Limite commune de l’enveloppe JSON, en octets.</summary>
    public const int MaximumHeaderLength = 1024 * 1024;
    internal const int ReadRetryCount = 64;
    internal const int ReadRetryDelayMilliseconds = 1;
}
