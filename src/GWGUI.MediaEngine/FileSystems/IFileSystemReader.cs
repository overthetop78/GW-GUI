using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems;

/// <summary>Détecte et lit un système de fichiers dans une image sectorielle.</summary>
public interface IFileSystemReader
{
    /// <summary>Identifiant technique central du lecteur.</summary>
    string Id { get; }
    /// <summary>Ensemble non modifiable des identifiants centraux de formats d'image acceptés.</summary>
    IReadOnlySet<string> CatalogFormatIds { get; }
    /// <summary>Indique si l'image peut contenir le système de fichiers pris en charge.</summary>
    /// <param name="image">Image sectorielle à sonder.</param>
    /// <returns><see langword="true"/> lorsque l'image est candidate.</returns>
    bool CanRead(SectorImage image);
    /// <summary>Lit entièrement le volume reconnu.</summary>
    /// <param name="image">Image sectorielle validée par <see cref="CanRead"/>.</param>
    /// <returns>Volume décodé.</returns>
    /// <exception cref="InvalidDataException">Le contenu annoncé est corrompu ou incomplet.</exception>
    FileSystemVolume Read(SectorImage image);
}
