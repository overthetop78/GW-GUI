namespace GWGUI.MediaEngine.Reconstruction.EpsonQx10;

/// <summary>Construit les erreurs propres à la reconstruction Epson QX-10.</summary>
internal static class EpsonQx10Exceptions
{
    /// <summary>Crée l'erreur signalant un identifiant Epson QX-10 non pris en charge.</summary>
    public static ArgumentException InvalidFormat(string formatId) => new($"The selected format '{formatId}' is not a supported Epson QX-10 format.", nameof(formatId));
}
