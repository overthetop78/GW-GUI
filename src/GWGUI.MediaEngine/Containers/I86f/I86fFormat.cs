namespace GWGUI.MediaEngine.Containers.I86f;

/// <summary>Définit l'identification binaire d'un conteneur 86F.</summary>
internal static class I86fFormat
{
    /// <summary>Signature 86F interprétée en entier little-endian.</summary>
    public const uint Signature = 0x46423638;
    /// <summary>Position de la signature, en octets depuis le début du fichier.</summary>
    public const int SignatureOffset = 0;
    /// <summary>Longueur de la signature, en octets.</summary>
    public const int SignatureLength = 4;
}
