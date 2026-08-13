using GWGUI.MediaEngine.Exploration.Metadata;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Exploration.Results;

/// <summary>Décrit le résultat immuable de l'exploration d'une image de média.</summary>
public sealed record ExploredDiskImage
{
    /// <summary>Construit un résultat en copiant les collections détectées et en conservant séparément le volume principal.</summary>
    public ExploredDiskImage(string sourcePath, SectorImage image, FileSystemVolume volume, DiskImageMetadata metadata, bool fileSystemRecognized = true, IEnumerable<ExploredFileSystem>? detectedFileSystems = null, IEnumerable<string>? detectedImageFormatIds = null, string? primaryFormatId = null)
    {
        SourcePath = sourcePath;
        Image = image;
        Volume = volume;
        Metadata = metadata;
        FileSystemRecognized = fileSystemRecognized;
        DetectedFileSystems = (detectedFileSystems ?? []).ToArray();
        DetectedImageFormatIds = (detectedImageFormatIds ?? []).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        PrimaryFormatId = primaryFormatId ?? DetectedFileSystems.FirstOrDefault()?.FormatId ?? image.FormatId;
    }

    /// <summary>Obtient le chemin source.</summary>
    public string SourcePath { get; }
    /// <summary>Obtient l'image sectorielle retenue.</summary>
    public SectorImage Image { get; }
    /// <summary>Obtient le volume principal présenté par l'explorateur.</summary>
    public FileSystemVolume Volume { get; }
    /// <summary>Indique si un système de fichiers a été reconnu plutôt qu'un volume physique de repli.</summary>
    public bool FileSystemRecognized { get; }
    /// <summary>Obtient la copie des systèmes de fichiers détectés.</summary>
    public IReadOnlyList<ExploredFileSystem> DetectedFileSystems { get; }
    /// <summary>Obtient la copie ordonnée et sans doublon des formats détectés.</summary>
    public IReadOnlyList<string> DetectedImageFormatIds { get; }
    /// <summary>Obtient le format correspondant au volume principal actuellement présenté.</summary>
    public string PrimaryFormatId { get; }
    /// <summary>Obtient les métadonnées techniques calculées lors de la construction.</summary>
    public DiskImageMetadata Metadata { get; }
    /// <summary>Indique que l'image valide utilise un chargeur plutôt qu'un catalogue de fichiers.</summary>
    public bool UsesCustomSectorLoader => !FileSystemRecognized && Metadata.Content.HasCataloglessOrganization;
}
