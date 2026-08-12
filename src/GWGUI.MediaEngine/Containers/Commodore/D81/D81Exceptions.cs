namespace GWGUI.MediaEngine.Containers.Commodore.D81;

/// <summary>Construit les erreurs propres aux conteneurs D81.</summary>
internal static class D81Exceptions
{
    /// <summary>Crée l'erreur signalant une longueur différente de celle du format.</summary>
    public static InvalidDataException InvalidLength(int observedLength, int expectedLength) => new($"L'image D81 contient {observedLength} octet(s) ; elle doit en contenir exactement {expectedLength}.");
}
