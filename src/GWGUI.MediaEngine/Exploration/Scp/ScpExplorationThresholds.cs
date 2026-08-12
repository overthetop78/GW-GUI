namespace GWGUI.MediaEngine.Exploration.Scp;

/// <summary>Définit les seuils techniques du classement automatique SCP.</summary>
internal static class ScpExplorationThresholds
{
    /// <summary>Score minimal d'une image dont l'identifiant de format est conservé.</summary>
    public const double MinimumDecodedFormatScore = 0.5;
    /// <summary>Score initial signifiant qu'aucun système de fichiers n'a encore été reconnu.</summary>
    public const double NoRecognizedScore = double.NegativeInfinity;
}
