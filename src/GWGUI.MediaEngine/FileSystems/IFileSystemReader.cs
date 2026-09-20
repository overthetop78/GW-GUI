using MediaVolumeDescriptor = global::GWGUI.MediaFileSystems.Contracts.MediaVolumeDescriptor;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces.Exploration;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.FileSystems;

/// <summary>Détecte et lit un système de fichiers dans une image sectorielle.</summary>
public interface IFileSystemReader : IMediaFileSystemReader
{
    /// <summary>Identifiant technique central du lecteur.</summary>
    new string Id { get; }
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

    IReadOnlySet<MediaRepresentationKind> IMediaFileSystemReader.RepresentationKinds =>
        new HashSet<MediaRepresentationKind> { MediaRepresentationKind.Sectors };

    bool IMediaFileSystemReader.CanRead(MediaImageDocument document, MediaVolumeDescriptor volume)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(volume);
        return document.Representation is SectorMediaImageRepresentation sectors &&
               volume.Start == 0 &&
               volume.Length == sectors.Image.Capacity &&
               CanRead(sectors.Image);
    }

    FileSystemVolume IMediaFileSystemReader.Read(MediaImageDocument document, MediaVolumeDescriptor volume)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(volume);
        if (document.Representation is not SectorMediaImageRepresentation sectors ||
            volume.Start != 0 ||
            volume.Length != sectors.Image.Capacity)
            throw new NotSupportedException("The legacy sector file-system reader requires a whole sector image volume.");
        return Read(sectors.Image);
    }
}
