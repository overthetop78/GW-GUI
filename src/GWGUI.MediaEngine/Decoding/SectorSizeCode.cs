namespace GWGUI.MediaEngine.Decoding;

/// <summary>Convertit les tailles sectorielles entre nombre d'octets et code exponentiel.</summary>
internal static class SectorSizeCode
{
    /// <summary>Obtient le code correspondant à une puissance de deux à partir de 128 octets, ou zéro lorsqu'aucun code ne correspond.</summary>
    /// <param name="sizeBytes">Taille du secteur en octets.</param>
    /// <returns>Code de taille compris entre zéro et sept, ou zéro si la taille n'est pas reconnue.</returns>
    public static byte FromByteCount(int sizeBytes)
    {
        for (byte code = 0; code < 8; code++) if ((128 << code) == sizeBytes) return code;
        return 0;
    }
}
