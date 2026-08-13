namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>Définit l'unité temporelle des pistes produites par les encodeurs internes.</summary>
internal static class ScpSyntheticFluxConstants
{
    /// <summary>Durée d'un tick d'encodeur interne, en nanosecondes.</summary>
    public const int EncoderTickNanoseconds = ScpFormatConstants.ResolutionStepNanoseconds;
}
