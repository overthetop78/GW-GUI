using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Cpm;
using GWGUI.MediaEngine.FileSystems.Fat;
using GWGUI.MediaEngine.Geometries.Ibm;
using GWGUI.MediaEngine.Images.Interpretations;
using GWGUI.MediaEngine.Recognition.Amstrad;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Builders;

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
        var hasFatBpb = FatBpbGeometryDetector.TryDetect(bytes, bytes.Length, out _);
        var geometry = IbmRawImageGeometryDetector.Detect(bytes);
        var image = IbmRawSectorImageBuilder.Create(bytes, geometry, cancellationToken);
        if (!hasFatBpb)
        {
            var logical = CpmDirectoryReader.Flatten(image);
            if (CpmDirectoryReader.FindDirectory(logical, AmstradCpmLayout.CpcSystem, AmstradCpmLayout.CpcSectorSize, allowEmpty: false, rejectLowercase: false) is not null) return SectorImageInterpretation.Retag(image, DiskImageFormatIds.AmstradCpc);
        }
        if (!hasFatBpb && AmstradCpmDiskSpecification.TryParse(bytes, out _)) return SectorImageInterpretation.Retag(image, DiskImageFormatIds.AmstradPcw);
        return image;
    }
}
