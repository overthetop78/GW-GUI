namespace GWGUI.MediaEngine.Containers.Apple.TwoImg;

/// <summary>Regroupe la signature et la version du format de conteneur Apple 2IMG pris en charge.</summary>
public static class TwoImgFormat
{
    /// <summary>Signature ASCII placée au début de tout conteneur 2IMG.</summary>
    public const string Signature = "2IMG";

    /// <summary>Version de l’en-tête 2IMG reconnue par le lecteur.</summary>
    public const ushort SupportedVersion = 1;

    /// <summary>Représentation ASCII mémorisée de <see cref="Signature"/>.</summary>
    private static readonly byte[] EncodedSignature = System.Text.Encoding.ASCII.GetBytes(Signature);

    /// <summary>Obtient la signature 2IMG sous forme d’octets ASCII en lecture seule.</summary>
    public static ReadOnlySpan<byte> SignatureBytes => EncodedSignature;
}
