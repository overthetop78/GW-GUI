using MediaSourceDescriptor = global::GWGUI.MediaEngine.Contracts.MediaSourceDescriptor;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Contracts.Explorer;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp;
using GWGUI.MediaEngine.Images.Reading.Documents;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Inspection;
using GWGUI.MediaEngine.Images.Reading;
using GWGUI.MediaEngine.Images.Models.Flux;
using GWGUI.MediaEngine.Images.Models.Sectors;
using FileSystemRegistry = GWGUI.MediaFileSystems.Exploration.SectorFileSystemRegistry;
using System.IO;

namespace GWGUI.MediaEngine.Images.Reading;

/// <summary>ReconnaÃ®t une image de mÃ©dia et construit son document d'exploration technique.</summary>
public sealed class DiskImageExplorer
{
    /// <summary>Common media recognition and reading service.</summary>
    private readonly MediaImageReadingService readingService;
    /// <summary>Registre des lecteurs de systÃ¨mes de fichiers.</summary>
    private readonly FileSystemRegistry fileSystems;
    /// <summary>Service spÃ©cialisÃ© dans l'exploration automatique des captures SCP.</summary>
    private readonly ScpImageExplorationService scpExploration;
    /// <summary>Service partagÃ© de normalisation et d'interprÃ©tation des images.</summary>
    private readonly DiskImageInterpretationService interpretations;
    /// <summary>Fabrique partagÃ©e des documents d'exploration.</summary>
    private readonly DiskImageDocumentFactory documents;

    /// <summary>Initialise l'explorateur avec les services partagÃ©s composÃ©s par le moteur.</summary>
    internal DiskImageExplorer(MediaImageReadingService readingService, FileSystemRegistry fileSystems, ScpImageExplorationService scpExploration, DiskImageInterpretationService interpretations, DiskImageDocumentFactory documents)
    {
        this.readingService = readingService;
        this.fileSystems = fileSystems;
        this.scpExploration = scpExploration;
        this.interpretations = interpretations;
        this.documents = documents;
    }

    /// <summary>Identifiants de formats associÃ©s aux lecteurs de systÃ¨mes de fichiers disponibles.</summary>
    public IReadOnlySet<string> SupportedFormatIds => fileSystems.SupportedFormatIds;

    /// <summary>CrÃ©e un explorateur utilisant la composition par dÃ©faut de MediaEngine.</summary>
    public static DiskImageExplorer CreateDefault() => MediaEngineFactory.CreateDefaultExplorer();

    /// <summary>ReconnaÃ®t le contenu, applique Ã©ventuellement une sÃ©lection explicite et explore ses systÃ¨mes de fichiers.</summary>
    /// <param name="path">Chemin de l'image Ã  explorer.</param>
    /// <param name="formatId">Format sectoriel explicitement demandÃ©, ou <see langword="null"/> pour la dÃ©tection automatique.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture et la reconnaissance.</param>
    /// <returns>Document contenant l'image reconnue et ses interprÃ©tations de systÃ¨mes de fichiers.</returns>
    /// <exception cref="FileNotFoundException">Le chemin n'existe pas.</exception>
    /// <exception cref="DiskImageCandidatesRejectedException">Un conteneur candidat est identifiÃ© mais corrompu.</exception>
    /// <exception cref="OperationCanceledException">Le jeton demande l'annulation.</exception>
    public async Task<ExploredDiskImage> ExploreAsync(string path, string? formatId = null, CancellationToken cancellationToken = default)
        => await ExploreAsync(path, formatId, null, cancellationToken).ConfigureAwait(false);

    /// <summary>Explore l'image et publie les unités réelles de l'analyse SCP lorsqu'elle s'applique.</summary>
    public async Task<ExploredDiskImage> ExploreAsync(
        string path,
        string? formatId,
        IProgress<ScpExplorationProgress>? progress,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path)) throw new FileNotFoundException($"L'image de média '{path}' n'existe pas.", path);
        MediaImageDocument document;
        try
        {
            document = await ReadDocumentAsync(path, formatId, cancellationToken).ConfigureAwait(false);
        }
        catch (NotSupportedException)
        {
            return documents.CreateUnknown(path);
        }

        return await ExploreAsync(document, formatId, progress, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Explores an already loaded media document without opening its source path again.</summary>
    public async Task<ExploredDiskImage> ExploreAsync(
        MediaImageDocument document,
        string? formatId = null,
        IProgress<ScpExplorationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        var path = document.Source.PrimaryPath;

        if (document.Representation is FluxMediaImageRepresentation flux)
        {
            if (!document.FormatId.Equals(DiskImageFormatIds.RawScp, StringComparison.OrdinalIgnoreCase)) return documents.CreateUnknown(path);
            var scpImage = ProtectedTrackScpImageAdapter.Create(flux.Image, document.Metadata);
            var explored = await scpExploration.ExploreAutomaticallyAsync(
                path,
                scpImage,
                progress,
                cancellationToken).ConfigureAwait(false);
            return formatId is null ? explored : explored.SelectFormat(formatId) ?? documents.CreateUnknown(path);
        }

        if (document.Representation is not SectorMediaImageRepresentation sectors) return documents.CreateUnknown(path);
        var image = sectors.Image;
        var result = formatId is null ? ReadAutomatically(image) : ReadExplicitly(image, formatId);
        return documents.Create(path, result.Image, result.Detected, [result.Image]);
    }

    /// <summary>Explore une capture SCP déjà acquise en mémoire tout en conservant son chemin de destination.</summary>
    public Task<ExploredDiskImage> ExploreScpAsync(
        string path,
        ScpImage image,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(image);
        return scpExploration.ExploreAutomaticallyAsync(path, image, null, cancellationToken);
    }

    /// <summary>Reads through the common chain and retries without a downstream sector-format hint when necessary.</summary>
    private async Task<MediaImageDocument> ReadDocumentAsync(string path, string? formatId, CancellationToken cancellationToken)
    {
        var containerFormatId = Path.GetExtension(path).Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase)
            ? null
            : formatId;
        var source = new MediaSourceDescriptor(path, [], RequestedFormatId: containerFormatId);
        try
        {
            return await readingService.ReadAsync(source, cancellationToken).ConfigureAwait(false);
        }
        catch (NotSupportedException) when (formatId is not null)
        {
            return await readingService.ReadAsync(source with { RequestedFormatId = null }, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>Lit les systÃ¨mes de fichiers directement reconnus, puis la premiÃ¨re interprÃ©tation supplÃ©mentaire exploitable.</summary>
    private (SectorImage Image, IReadOnlyList<ExploredFileSystem> Detected) ReadAutomatically(SectorImage image)
    {
        var candidates = new[] { image }.Concat(interpretations.AdditionalImageCandidates(image));
        var detected = fileSystems.ReadDistinctCandidates(candidates)
            .Select(candidate => new ExploredFileSystem(
                candidate.Match.ReaderId,
                (SectorImage)candidate.Image,
                FileSystemVolumeMapper.ConvertVolume(candidate.Match.Volume)))
            .ToArray();
        return (image, detected);
    }

    /// <summary>Lit le systÃ¨me de fichiers correspondant au format explicitement demandÃ©.</summary>
    private (SectorImage Image, IReadOnlyList<ExploredFileSystem> Detected) ReadExplicitly(SectorImage image, string formatId)
    {
        var selectedImage = image.FormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase) ? image : image.WithFormatId(formatId);
        if (fileSystems.TryRead(selectedImage, formatId, out var match))
        {
            return (selectedImage, [new(match.ReaderId, selectedImage,
                FileSystemVolumeMapper.ConvertVolume(match.Volume))]);
        }
        return (selectedImage, []);
    }

}
