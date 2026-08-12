using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Exploration.Documents;

/// <summary>Construit les documents d'exploration reconnus, physiques ou inconnus.</summary>
internal sealed class DiskImageDocumentFactory
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
    /// <param name="detectedImageFormatIds">Formats sectoriels décodés pendant l'exploration.</param>
    /// <returns>Document reconnu ou document physique de repli.</returns>
    public ExploredDiskImage Create(string path, SectorImage image, IReadOnlyList<ExploredFileSystem> detected, IReadOnlyList<string>? detectedImageFormatIds = null)
    {
        if (detected.Count > 0) return new(path, image, detected[0].Volume, true, detected, detectedImageFormatIds);
        var physical = new FileSystemVolume(Path.GetFileNameWithoutExtension(path), image.FormatId, image.Capacity, 0, null, null, PhysicalSectorTreeBuilder.Build(image), []);
        return new(path, image, physical, false, [], detectedImageFormatIds);
    }

    /// <summary>Construit un document inconnu conservant la capacité observée du fichier.</summary>
    /// <param name="path">Chemin du fichier non reconnu.</param>
    /// <returns>Document contenant une image technique inconnue.</returns>
    public ExploredDiskImage Unknown(string path)
    {
        var capacity = new FileInfo(path).Length;
        var image = new SectorImage(DiskImageFormatIds.Unknown, UnknownBlockSize, UnknownGeometryDimension, UnknownGeometryDimension, UnknownGeometryDimension, [], capacity: capacity, logicalBlockCount: UnknownLogicalBlockCount);
        return Create(path, image, []);
    }
}
