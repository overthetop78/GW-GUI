namespace GWGUI.MediaEngine.Containers.Dec.Rx02;

/// <summary>Construit les erreurs propres aux dumps physiques DEC RX02.</summary>
internal static class DecRx02Exceptions
{
    /// <summary>Crée l'erreur signalant une capacité différente du dump complet attendu.</summary>
    public static InvalidDataException IncompleteImage(int observedLength, int expectedLength) => new($"Le dump DEC RX02 contient {observedLength} octet(s) ; un dump physique complet doit en contenir {expectedLength}.");
}
