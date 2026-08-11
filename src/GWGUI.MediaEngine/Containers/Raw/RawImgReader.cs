using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Cpm;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Images.Interpretations;
using GWGUI.MediaEngine.Recognition.Amstrad;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Raw;

/// <summary>Lit les images IMG ambiguës et départage les interprétations IBM, Amstrad CPC et Amstrad PCW prises en charge.</summary>
internal sealed class RawImgReader
{
    /// <summary>Lit le fichier IMG puis conserve ou réidentifie l'image construite par le Reader IBM selon son contenu.</summary>
    /// <param name="path">Chemin de l'image IMG brute.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture et la construction.</param>
    /// <returns>Image sectorielle IBM, Amstrad CPC ou Amstrad PCW validée.</returns>
    /// <exception cref="InvalidDataException">La géométrie ne peut pas être déterminée par le Reader IBM.</exception>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        var hasFatBpb = IbmPcImageReader.HasValidBpbGeometry(bytes);
        var image = IbmPcImageReader.Create(bytes, cancellationToken);
        if (!hasFatBpb && CpmDirectoryProbe.FindCpcRawDirectory(bytes) is not null) return SectorImageInterpretation.Retag(image, DiskImageFormatIds.AmstradCpc);
        if (!hasFatBpb && PcwDiskSpecificationProbe.LooksLikePcwDiskSpecification(bytes)) return SectorImageInterpretation.Retag(image, DiskImageFormatIds.AmstradPcw);
        return image;
    }
}
