using EmulatorCatalogFunctions = GWGUI.Emulation.Example.Common.Functions.EmulatorCatalogFunctions;

namespace GWGUI.Emulation.Example.Common.Services;

/// <summary>Implémentation simulée du même contrat de versions que les autres catalogues Common.</summary>
/// <remarks>Les trois cœurs sont livrés dans l'assembly du module et partagent sa version.
/// Leur recherche et leur installation sont donc simulées en mémoire, sans requête HTTP, archive ou copie de DLL.
/// Le service reçoit les descriptions par une fonction de catalogue et ne connaît aucune classe de backend.
/// Les adaptateurs passent par ICoreReleaseService ; CoreProvider résout ensuite l'installation active.</remarks>
internal sealed class CoreReleaseService : ICoreReleaseService
{
    /// <summary>Résolveur emprunté des descriptions ; le catalogue valide les identités de cœur acceptées.</summary>
    private readonly Func<Emulator, EmulationEmulatorDefinition> _getDefinition;

    /// <summary>Construit le service à partir du catalogue de descriptions existant.</summary>
    /// <param name="getDefinition">Résolveur des métadonnées de Libretro, External et Internal ; aucun cœur n'est créé ici.</param>
    public CoreReleaseService(Func<Emulator, EmulationEmulatorDefinition> getDefinition)
    {
        ArgumentNullException.ThrowIfNull(getDefinition);
        _getDefinition = getDefinition;
    }

    /// <summary>Propose l'unique version déjà intégrée pour l'identité demandée.</summary>
    /// <param name="emulator">Identité à rechercher dans le catalogue reçu.</param>
    /// <param name="cancellationToken">Annulation contrôlée avant toute consultation du catalogue.</param>
    /// <returns>Publication typée du SDK portant l'identité et la version du module.</returns>
    /// <remarks>DownloadUri désigne l'assembly existant par une URI de fichier ; aucune lecture ni copie n'est effectuée.
    /// La date UnixEpoch est une donnée déterministe du mock, pas une date de publication réseau.
    /// ExpectedArchiveSize reste null puisqu'aucune archive n'est proposée.</remarks>
    public Task<IReadOnlyList<CoreRelease>> GetAvailableAsync(Emulator emulator, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var definition = _getDefinition(emulator);
        return Task.FromResult<IReadOnlyList<CoreRelease>>([new(emulator, definition.Id, Engine.CoreVersion, new Uri(Engine.CoreImplementationPath, UriKind.Absolute), DateTimeOffset.UnixEpoch, null)]);
    }

    /// <summary>Valide la version sélectionnée puis rend l'installation intégrée disponible.</summary>
    /// <param name="release">Publication du cœur : son identité et sa version doivent correspondre au catalogue.</param>
    /// <param name="progress">Récepteur emprunté, notifié une seule fois avec une progression complète de 1 sur 1.</param>
    /// <param name="cancellationToken">Annulation contrôlée avant validation et notification.</param>
    /// <returns>Emplacements existants du module ; aucun fichier ni dossier d'installation n'est créé.</returns>
    /// <exception cref="ArgumentOutOfRangeException">L'identité ou la version ne correspond pas au cœur intégré.</exception>
    /// <remarks>La progression utilise le contrat typé commun, même si cette simulation termine immédiatement.
    /// Un vrai service peut télécharger et activer une bibliothèque derrière cette même interface.</remarks>
    public Task<CoreInstallationPaths> InstallAsync(CoreRelease release, IProgress<CoreInstallProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(release);
        var definition = _getDefinition(release.Emulator);
        if (release.Id != definition.Id || release.DeclaredVersion != Engine.CoreVersion) throw new ArgumentOutOfRangeException(nameof(release));
        progress?.Report(new CoreInstallProgress(1, 1));
        return Task.FromResult(EmulatorCatalogFunctions.GetInstallationPaths());
    }

    /// <summary>Résout l'installation déjà fournie par le build après validation de l'identité du cœur.</summary>
    /// <param name="emulator">Identité à vérifier dans le catalogue.</param>
    /// <param name="cancellationToken">Annulation contrôlée avant la résolution.</param>
    /// <returns>Installation intégrée commune aux trois simulations ; ce profil est toujours préinstallé.</returns>
    /// <remarks>Le contrat permet null pour les services dont aucun cœur n'est installé.
    /// Cet exemple utilise l'installation existante et ne crée aucun manifeste ni bibliothèque de remplacement.</remarks>
    public Task<CoreInstallationPaths?> GetActiveInstallationAsync(Emulator emulator, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = _getDefinition(emulator);
        return Task.FromResult<CoreInstallationPaths?>(EmulatorCatalogFunctions.GetInstallationPaths());
    }
}
