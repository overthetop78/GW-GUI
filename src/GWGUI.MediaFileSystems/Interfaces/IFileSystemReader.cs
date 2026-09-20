using MediaVolumeDescriptor = global::GWGUI.MediaFileSystems.Contracts.MediaVolumeDescriptor;
using IMediaImageDocument = global::GWGUI.MediaFileSystems.Interfaces.IMediaImageDocument;
using IMediaSectorImage = global::GWGUI.MediaFileSystems.Interfaces.IMediaSectorImage;
using IMediaSectorRepresentation = global::GWGUI.MediaFileSystems.Interfaces.IMediaSectorRepresentation;
using GWGUI.MediaFileSystems.Interfaces.Exploration;

namespace GWGUI.MediaFileSystems;

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
    bool CanRead(IMediaSectorImage image);
    /// <summary>Lit entièrement le volume reconnu.</summary>
    /// <param name="image">Image sectorielle validée par <see cref="CanRead"/>.</param>
    /// <returns>Volume décodé.</returns>
    /// <exception cref="InvalidDataException">Le contenu annoncé est corrompu ou incomplet.</exception>
    FileSystemVolume Read(IMediaSectorImage image);

    bool IMediaFileSystemReader.CanRead(IMediaImageDocument document, MediaVolumeDescriptor volume)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(volume);
        return document.Representation is IMediaSectorRepresentation sectors &&
               volume.Start == 0 &&
               volume.Length == sectors.Image.Capacity &&
               CanRead(sectors.Image);
    }

    FileSystemVolume IMediaFileSystemReader.Read(IMediaImageDocument document, MediaVolumeDescriptor volume)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(volume);
        if (document.Representation is not IMediaSectorRepresentation sectors ||
            volume.Start != 0 ||
            volume.Length != sectors.Image.Capacity)
            throw new NotSupportedException("The legacy sector file-system reader requires a whole sector image volume.");
        return Read(sectors.Image);
    }
}
