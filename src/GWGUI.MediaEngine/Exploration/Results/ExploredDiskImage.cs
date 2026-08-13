using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Exploration.Contracts;
using GWGUI.MediaEngine.Exploration.Documents;
using GWGUI.MediaEngine.Exploration.Metadata;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Exploration.Results;

/// <summary>Décrit le résultat immuable de l'exploration d'une image de média.</summary>
public sealed record ExploredDiskImage : IImageDisquette
{
    /// <summary>Construit un résultat en copiant les collections détectées et en conservant séparément le volume principal.</summary>
    public ExploredDiskImage(
        string sourcePath,
        SectorImage image,
        FileSystemVolume volume,
        DiskImageMetadata metadata,
        bool fileSystemRecognized = true,
        IEnumerable<ExploredFileSystem>? detectedFileSystems = null,
        IEnumerable<SectorImage>? detectedSectorImages = null,
        string? primaryFormatId = null,
        ScpImage? scpImage = null)
    {
        SourcePath = sourcePath;
        Image = image;
        Volume = volume;
        Metadata = metadata;
        FileSystemRecognized = fileSystemRecognized;
        DetectedFileSystems = (detectedFileSystems ?? []).ToArray();
        DetectedSectorImages = (detectedSectorImages ?? [])
            .GroupBy(item => item.FormatId, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();
        DetectedImageFormatIds = DetectedFileSystems
            .Select(item => item.FormatId)
            .Concat(DetectedSectorImages.Select(item => item.FormatId))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        PrimaryFormatId = primaryFormatId ?? DetectedFileSystems.FirstOrDefault()?.FormatId ?? image.FormatId;
        ScpImage = scpImage;
        TypeImage = Path.GetExtension(sourcePath).TrimStart('.').ToUpperInvariant();
        VersionImage = scpImage?.Header.Version;
        TailleImage = scpImage?.FileSize ?? (File.Exists(sourcePath) ? new FileInfo(sourcePath).Length : image.Capacity);
        MetadonneesImage = DiskImageContractData.CreateMetadata(sourcePath, image, scpImage);
        Pistes = DiskImageContractData.CreateTracks(image, scpImage);
        FormatsDetectes = DiskImageContractData.CreateFormats(
            image,
            volume,
            metadata,
            fileSystemRecognized,
            DetectedFileSystems,
            DetectedSectorImages);
        Diagnostics = DiskImageContractData.CreateDiagnostics(DetectedFileSystems.SelectMany(item => item.Volume.Warnings).Concat(volume.Warnings).Distinct(StringComparer.Ordinal));
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
    /// <summary>Obtient chaque image sectorielle réellement décodée sans catalogue associé.</summary>
    public IReadOnlyList<SectorImage> DetectedSectorImages { get; }
    /// <summary>Obtient le format correspondant au volume principal actuellement présenté.</summary>
    public string PrimaryFormatId { get; }
    /// <summary>Obtient les métadonnées techniques calculées lors de la construction.</summary>
    public DiskImageMetadata Metadata { get; }
    /// <summary>Obtient le conteneur SCP complet lorsque la source est une capture de flux.</summary>
    public ScpImage? ScpImage { get; }
    /// <inheritdoc />
    public string TypeImage { get; }
    /// <inheritdoc />
    public int? VersionImage { get; }
    /// <inheritdoc />
    public long TailleImage { get; }
    /// <inheritdoc />
    public IMetadonneesImage MetadonneesImage { get; }
    /// <inheritdoc />
    public IReadOnlyList<IPiste> Pistes { get; }
    /// <inheritdoc />
    public IReadOnlyList<IFormatDetecte> FormatsDetectes { get; }
    /// <inheritdoc />
    public IReadOnlyList<IDiagnostic> Diagnostics { get; }
    /// <summary>Indique que l'image valide utilise un chargeur plutôt qu'un catalogue de fichiers.</summary>
    public bool UsesCustomSectorLoader => !FileSystemRecognized && Metadata.Content.HasCataloglessOrganization;

    /// <summary>Construit une vue du même résultat avec l'interprétation demandée comme sélection courante.</summary>
    public ExploredDiskImage? SelectFormat(string formatId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(formatId);
        var selectedFileSystem = DetectedFileSystems.FirstOrDefault(item =>
            string.Equals(item.FormatId, formatId, StringComparison.OrdinalIgnoreCase));
        if (selectedFileSystem is not null)
        {
            return new ExploredDiskImage(
                SourcePath,
                selectedFileSystem.Image,
                selectedFileSystem.Volume,
                Metadata,
                true,
                DetectedFileSystems,
                DetectedSectorImages,
                selectedFileSystem.FormatId,
                ScpImage);
        }

        var selectedImage = DetectedSectorImages.FirstOrDefault(item =>
            string.Equals(item.FormatId, formatId, StringComparison.OrdinalIgnoreCase));
        if (selectedImage is null)
        {
            return null;
        }

        IReadOnlyList<FileSystemEntry> entries;
        if (Metadata.Content.HasCataloglessOrganization)
        {
            entries = [];
        }
        else
        {
            entries = PhysicalSectorTreeBuilder.Build(selectedImage);
        }

        var volume = new FileSystemVolume(
            Path.GetFileNameWithoutExtension(SourcePath),
            selectedImage.FormatId,
            selectedImage.Capacity,
            0,
            null,
            null,
            entries,
            []);
        return new ExploredDiskImage(
            SourcePath,
            selectedImage,
            volume,
            Metadata,
            false,
            DetectedFileSystems,
            DetectedSectorImages,
            selectedImage.FormatId,
            ScpImage);
    }
}
