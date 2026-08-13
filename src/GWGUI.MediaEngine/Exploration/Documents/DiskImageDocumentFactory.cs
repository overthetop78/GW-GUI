using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Exploration.Metadata;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;

using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.Exploration.Documents;

/// <summary>Construit les documents d'exploration reconnus, physiques ou inconnus.</summary>
internal sealed class DiskImageDocumentFactory(DiskImageMetadataFactory metadataFactory)
{
    /// <summary>Taille technique du bloc d'une image inconnue.</summary>
    public const int UnknownBlockSize = 1;
    /// <summary>Dimension minimale de chaque axe de la géométrie inconnue.</summary>
    public const int UnknownGeometryDimension = 1;
    /// <summary>Nombre logique minimal de blocs d'une image inconnue.</summary>
    public const int UnknownLogicalBlockCount = 1;

    /// <summary>Construit le document final et son arborescence physique de repli.</summary>
    /// <param name="path">Chemin source présenté par le document.</param>
    /// <param name="image">Image sectorielle retenue.</param>
    /// <param name="detected">Systèmes de fichiers détectés dans leur ordre.</param>
    /// <param name="detectedImages">Images sectorielles réellement décodées pendant l'exploration.</param>
    /// <returns>Document reconnu ou document physique de repli.</returns>
    public ExploredDiskImage Create(
        string path,
        SectorImage image,
        IReadOnlyList<ExploredFileSystem> detected,
        IReadOnlyList<SectorImage>? detectedImages = null,
        ScpImage? scpImage = null)
    {
        var images = detectedImages ?? detected.Select(item => item.Image).ToArray();
        var formatIds = detected
            .Select(item => item.FormatId)
            .Concat(images.Select(item => item.FormatId))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var metadata = metadataFactory.Create(image, formatIds);
        if (detected.Count > 0)
        {
            return new(
                path,
                image,
                detected[0].Volume,
                metadata,
                true,
                detected,
                images,
                detected[0].FormatId,
                scpImage);
        }

        var entries = metadata.Content.HasCataloglessOrganization ? [] : PhysicalSectorTreeBuilder.Build(image);
        var physical = new FileSystemVolume(Path.GetFileNameWithoutExtension(path), image.FormatId, image.Capacity, 0, null, null, entries, []);
        return new(
            path,
            image,
            physical,
            metadata,
            false,
            [],
            images,
            scpImage: scpImage);
    }

    /// <summary>Construit un document inconnu conservant la capacité observée du fichier.</summary>
    /// <param name="path">Chemin du fichier non reconnu.</param>
    /// <returns>Document contenant une image technique inconnue.</returns>
    public ExploredDiskImage CreateUnknown(string path, ScpImage? scpImage = null)
    {
        var capacity = new FileInfo(path).Length;
        var image = new SectorImage(DiskImageFormatIds.Unknown, UnknownBlockSize, UnknownGeometryDimension, UnknownGeometryDimension, UnknownGeometryDimension, [], capacity: capacity, logicalBlockCount: UnknownLogicalBlockCount);
        return Create(path, image, [], scpImage: scpImage);
    }
}
