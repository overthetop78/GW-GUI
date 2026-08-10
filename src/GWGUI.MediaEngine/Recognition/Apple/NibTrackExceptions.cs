namespace GWGUI.MediaEngine.Recognition.Apple;

/// <summary>Construit les erreurs produites pendant la validation d’une image NIB.</summary>
internal static class NibTrackExceptions
{
    /// <summary>Crée l’erreur signalant une longueur d’image NIB incompatible avec ses pistes.</summary>
    /// <param name="observedLength">Longueur totale observée, en octets.</param>
    /// <param name="expectedTrackLength">Longueur attendue d’une piste, en octets.</param>
    /// <returns>Exception contenant les longueurs observée et attendue.</returns>
    public static InvalidDataException InvalidLength(int observedLength, int expectedTrackLength) =>
        new($"The Apple NIB image length {observedLength} is invalid; it must be a positive multiple of {expectedTrackLength} bytes.");
}
