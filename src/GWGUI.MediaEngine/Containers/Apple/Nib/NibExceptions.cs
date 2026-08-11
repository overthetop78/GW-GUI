namespace GWGUI.MediaEngine.Containers.Apple.Nib;

/// <summary>Construit les erreurs produites pendant la validation d'un conteneur NIB.</summary>
internal static class NibExceptions
{
    /// <summary>Crée l'erreur signalant que la longueur totale n'est pas un multiple positif de la longueur d'une piste NIB.</summary>
    /// <param name="observedLength">Longueur totale observée, en octets.</param>
    /// <param name="trackLengthBytes">Longueur d'une piste NIB, en octets.</param>
    /// <returns>Exception contenant la longueur totale observée et la longueur d'une piste.</returns>
    public static InvalidDataException InvalidLength(int observedLength, int trackLengthBytes) => new($"The Apple NIB image length {observedLength} is invalid; it must be a positive multiple of {trackLengthBytes} bytes.");
}
