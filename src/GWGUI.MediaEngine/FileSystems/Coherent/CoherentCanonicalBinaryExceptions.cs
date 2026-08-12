namespace GWGUI.MediaEngine.FileSystems.Coherent;

/// <summary>Construit les erreurs de lecture des entiers canoniques COHERENT.</summary>
internal static class CoherentCanonicalBinaryExceptions
{
    /// <summary>Crée l'erreur signalant une longueur insuffisante.</summary>
    public static ArgumentException InsufficientLength(int observedLength, int expectedLength, string parameterName) => new($"La valeur canonique COHERENT contient {observedLength} octet(s) ; {expectedLength} sont requis.", parameterName);
}
