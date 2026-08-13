namespace GWGUI.MediaEngine.Encoding;

/// <summary>Définit les clés techniques communes des attributs transmis aux encodeurs de piste.</summary>
public static class TrackEncodingAttributeKeys
{
    /// <summary>Clé du nombre de secteurs de la piste.</summary>
    public const string SectorsPerTrack = "sectorsPerTrack";
    /// <summary>Clé du code de format attendu par l'encodeur.</summary>
    public const string Format = "format";
    /// <summary>Clé du nombre de pistes physiques par face.</summary>
    public const string TracksPerSide = "tracksPerSide";
    /// <summary>Retourne la clé technique du tag portant l'index indiqué.</summary>
    /// <param name="index">Index du tag.</param>
    /// <returns>Clé technique correspondante.</returns>
    public static string Tag(int index) => $"tag{index}";
}
