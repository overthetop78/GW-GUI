namespace GWGUI.MediaEngine.FileSystems.Acorn.Adfs;

/// <summary>Construit les erreurs propres aux catalogues ADFS.</summary>
public static class AcornAdfsExceptions
{
    /// <summary>Crée l'erreur signalant une image non reconnue.</summary>
    public static InvalidDataException UnsupportedImage(int blockSize, int blockCount) => new($"The image does not contain a supported Acorn ADFS catalogue ({blockSize}-byte blocks, {blockCount} blocks). ");
    /// <summary>Crée l'erreur signalant un répertoire invalide.</summary>
    public static InvalidDataException InvalidDirectory(int address) => new($"The ADFS directory at address {address} is invalid or incomplete.");
}
