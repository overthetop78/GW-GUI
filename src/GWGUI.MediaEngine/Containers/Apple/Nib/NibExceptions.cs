namespace GWGUI.MediaEngine.Containers.Apple.Nib;

/// <summary>Construit les erreurs produites pendant la validation d'un conteneur NIB.</summary>
internal static class NibExceptions
{
    /// <summary>Crée l'erreur signalant que la longueur totale n'est pas un multiple positif de la longueur d'une piste NIB.</summary>
    /// <param name="observedLength">Longueur totale observée, en octets.</param>
    /// <param name="trackLengthBytes">Longueur d'une piste NIB, en octets.</param>
    /// <returns>Exception contenant la longueur totale observée et la longueur d'une piste.</returns>
    public static InvalidDataException InvalidLength(int observedLength, int trackLengthBytes) => new($"The Apple NIB image length {observedLength} is invalid; it must be a positive multiple of {trackLengthBytes} bytes.");
    /// <summary>Crée l'erreur signalant un nombre de pistes invalide.</summary>
    public static InvalidDataException InvalidTrackCount(int observedCount) => new($"Le conteneur NIB doit contenir au moins une piste ; nombre observé : {observedCount}.");
    /// <summary>Crée l'erreur signalant une piste trop longue.</summary>
    public static InvalidDataException TrackTooLong(int track, int observedBits, int maximumBits) => new($"La piste NIB {track} contient {observedBits} bits et dépasse la limite de {maximumBits} bits.");
}
