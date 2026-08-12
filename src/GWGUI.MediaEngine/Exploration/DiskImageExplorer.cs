using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Exploration.Documents;
using GWGUI.MediaEngine.Exploration.Interpretation;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Exploration;

/// <summary>ReconnaÃ®t une image de mÃ©dia et construit son document d'exploration technique.</summary>
public sealed class DiskImageExplorer
{
    /// <summary>Registre ordonnÃ© des politiques de reconnaissance.</summary>
    private readonly DiskImageRecognitionRegistry recognition;
    /// <summary>Registre des lecteurs de systÃ¨mes de fichiers.</summary>
    private readonly FileSystemRegistry fileSystems;
    /// <summary>Service spÃ©cialisÃ© dans l'exploration automatique des captures SCP.</summary>
    private readonly ScpImageExplorationService scpExploration;
    /// <summary>Service partagÃ© de normalisation et d'interprÃ©tation des images.</summary>
    private readonly DiskImageInterpretationService interpretations;
    /// <summary>Fabrique partagÃ©e des documents d'exploration.</summary>
    private readonly DiskImageDocumentFactory documents;

    /// <summary>Initialise l'explorateur avec les services partagÃ©s composÃ©s par le moteur.</summary>
    internal DiskImageExplorer(DiskImageRecognitionRegistry recognition, FileSystemRegistry fileSystems, ScpImageExplorationService scpExploration, DiskImageInterpretationService interpretations, DiskImageDocumentFactory documents)
    {
        this.recognition = recognition;
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
            return documents.CreateUnknown(path);
        }

        var result = formatId is null ? ReadAutomatically(image) : ReadExplicitly(image, formatId);
        var unique = Deduplicate(result.Detected);
        return documents.Create(path, result.Image, unique, [result.Image.FormatId]);
    }

    /// <summary>VÃ©rifie la signature SCP commune sans se fier Ã  l'extension du chemin.</summary>
    private static async Task<bool> HasScpSignatureAsync(string path, CancellationToken cancellationToken)
    {
        var signature = new byte[ScpFormatConstants.SignatureLength];
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, ScpFormatConstants.SignatureLength, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var read = await stream.ReadAsync(signature, cancellationToken).ConfigureAwait(false);
        return read == signature.Length && ScpSignature.IsPresent(signature);
    }

    /// <summary>Lit les systÃ¨mes de fichiers directement reconnus, puis la premiÃ¨re interprÃ©tation supplÃ©mentaire exploitable.</summary>
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

    /// <summary>Lit le systÃ¨me de fichiers correspondant au format explicitement demandÃ©.</summary>
    private (SectorImage Image, IReadOnlyList<ExploredFileSystem> Detected) ReadExplicitly(SectorImage image, string formatId)
    {
        var selectedImage = image.FormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase) ? image : image.WithFormatId(formatId);
        if (fileSystems.TryRead(selectedImage, formatId, out var match) || fileSystems.TryRead(selectedImage, null, out match)) return (selectedImage, [new(formatId, match.ReaderId, match.Volume)]);
        return (selectedImage, []);
    }

    /// <summary>Supprime les interprÃ©tations identiques en conservant leur premiÃ¨re occurrence.</summary>
    private IReadOnlyList<ExploredFileSystem> Deduplicate(IEnumerable<ExploredFileSystem> detected)
    {
        var identities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        return detected.Where(item => identities.Add(FileSystemInterpretationIdentity.Create(item))).ToArray();
    }
}
