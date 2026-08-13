namespace GWGUI.MediaEngine.Containers.Apple.TwoImg;

/// <summary>Regroupe la signature et la version du format de conteneur Apple 2IMG pris en charge.</summary>
public static class TwoImgFormat
{
    /// <summary>Version de l’en-tête 2IMG reconnue par le lecteur.</summary>
    public const ushort SupportedVersion = 1;

    /// <summary>Obtient les quatre octets immuables de la signature 2IMG.</summary>
    public static ReadOnlySpan<byte> SignatureBytes => "2IMG"u8;

    /// <summary>Obtient l'identifiant de l'application enregistré dans les conteneurs produits.</summary>
    public static ReadOnlySpan<byte> CreatorBytes => "GWGU"u8;
}
