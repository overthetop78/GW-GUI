using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Images.Interpretations;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Exploration;

/// <summary>Reconnaît une image de média et construit son document d'exploration technique.</summary>
public sealed class DiskImageExplorer
{
    /// <summary>Registre ordonné des politiques de reconnaissance.</summary>
    private readonly DiskImageRecognitionRegistry recognition;
    /// <summary>Registre des lecteurs de systèmes de fichiers.</summary>
    private readonly FileSystemRegistry fileSystems;
    /// <summary>Service spécialisé dans l'exploration automatique des captures SCP.</summary>
    private readonly ScpImageExplorationService scpExploration;
    /// <summary>Service partagé de normalisation et d'interprétation des images.</summary>
    private readonly DiskImageInterpretationService interpretations;

    /// <summary>Initialise l'explorateur avec les services partagés composés par le moteur.</summary>
    internal DiskImageExplorer(DiskImageRecognitionRegistry recognition, FileSystemRegistry fileSystems, ScpImageExplorationService scpExploration, DiskImageInterpretationService interpretations)
    {
        this.recognition = recognition;
        this.fileSystems = fileSystems;
        this.scpExploration = scpExploration;
        this.interpretations = interpretations;
    }

    /// <summary>Identifiants de formats associés aux lecteurs de systèmes de fichiers disponibles.</summary>
    public IReadOnlySet<string> SupportedFormatIds => fileSystems.SupportedFormatIds;

    /// <summary>Crée un explorateur utilisant la composition par défaut de MediaEngine.</summary>
    public static DiskImageExplorer CreateDefault() => Images.DiskImageExplorerFactory.CreateDefault();

    /// <summary>Reconnaît le contenu, applique éventuellement une sélection explicite et explore ses systèmes de fichiers.</summary>
    /// <param name="path">Chemin de l'image à explorer.</param>
    /// <param name="formatId">Format sectoriel explicitement demandé, ou <see langword="null"/> pour la détection automatique.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture et la reconnaissance.</param>
    /// <returns>Document contenant l'image reconnue et ses interprétations de systèmes de fichiers.</returns>
    /// <exception cref="FileNotFoundException">Le chemin n'existe pas.</exception>
    /// <exception cref="DiskImageCandidatesRejectedException">Un conteneur candidat est identifié mais corrompu.</exception>
    /// <exception cref="OperationCanceledException">Le jeton demande l'annulation.</exception>
    public async Task<ExploredDiskImage> ExploreAsync(string path, string? formatId = null, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path)) throw DiskImageExplorationExceptions.MissingImage(path);
        if (formatId is null && await HasScpSignatureAsync(path, cancellationToken).ConfigureAwait(false)) return await scpExploration.ExploreAutomaticallyAsync(path, cancellationToken).ConfigureAwait(false);

        SectorImage image;
        try
        {
            image = await recognition.ReadAsync(path, formatId, cancellationToken).ConfigureAwait(false);
        }
        catch (DiskImageNotRecognizedException)
        {
            return interpretations.Unknown(path);
        }

        var result = formatId is null ? ReadAutomatically(image) : ReadExplicitly(image, formatId);
        var unique = Deduplicate(result.Detected);
        return interpretations.CreateDocument(path, result.Image, unique, [result.Image.FormatId]);
    }

    /// <summary>Vérifie la signature SCP commune sans se fier à l'extension du chemin.</summary>
    private static async Task<bool> HasScpSignatureAsync(string path, CancellationToken cancellationToken)
    {
        var signature = new byte[ScpFormatConstants.SignatureLength];
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, ScpFormatConstants.SignatureLength, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var read = await stream.ReadAsync(signature, cancellationToken).ConfigureAwait(false);
        return read == signature.Length && ScpSignature.IsPresent(signature);
    }

    /// <summary>Lit les systèmes de fichiers directement reconnus, puis la première interprétation supplémentaire exploitable.</summary>
    private (SectorImage Image, IReadOnlyList<ExploredFileSystem> Detected) ReadAutomatically(SectorImage image)
    {
        var detected = fileSystems.ReadAll(image).Matches.Select(match => new ExploredFileSystem(image.FormatId, match.ReaderId, match.Volume)).ToList();
        if (detected.Count != 0) return (image, detected);
        foreach (var interpretation in interpretations.AdditionalFileSystemInterpretations(image))
        {
            if (!fileSystems.TryRead(interpretation, interpretation.FormatId, out var match)) continue;
            detected.Add(new(interpretation.FormatId, match.ReaderId, match.Volume));
            return (interpretation, detected);
        }
        return (image, detected);
    }

    /// <summary>Lit le système de fichiers correspondant au format explicitement demandé.</summary>
    private (SectorImage Image, IReadOnlyList<ExploredFileSystem> Detected) ReadExplicitly(SectorImage image, string formatId)
    {
        var selectedImage = image.FormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase) ? image : SectorImageInterpretation.Retag(image, formatId);
        if (fileSystems.TryRead(selectedImage, formatId, out var match) || fileSystems.TryRead(selectedImage, null, out match)) return (selectedImage, [new(formatId, match.ReaderId, match.Volume)]);
        return (selectedImage, []);
    }

    /// <summary>Supprime les interprétations identiques en conservant leur première occurrence.</summary>
    private IReadOnlyList<ExploredFileSystem> Deduplicate(IEnumerable<ExploredFileSystem> detected)
    {
        var identities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        return detected.Where(item => identities.Add(DiskImageInterpretationService.InterpretationIdentity(item))).ToArray();
    }
}
