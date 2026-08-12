namespace GWGUI.MediaEngine.Reconstruction.Atari;

/// <summary>Construit les erreurs propres au routage des captures SCP Atari.</summary>
internal static class AtariScpReconstructionExceptions
{
    /// <summary>Crée l'erreur signalant qu'un identifiant demandé n'appartient pas aux familles Atari.</summary>
    /// <param name="formatId">Identifiant de format refusé.</param>
    /// <param name="parameterName">Nom du paramètre contenant l'identifiant.</param>
    /// <returns>L'exception contenant l'identifiant refusé.</returns>
    public static ArgumentException UnsupportedFormat(string formatId, string parameterName) => new($"The selected format '{formatId}' is not an Atari format.", parameterName);
}
