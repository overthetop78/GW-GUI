namespace GWGUI.Emulation.Example.Common.Interfaces;

/// <summary>Contrat Common de recherche, d'installation et de résolution des versions d'un cœur.</summary>
/// <remarks>Les noms, paramètres et résultats suivent le contrat utilisé par les autres catalogues.
/// Les types CoreRelease, CoreInstallProgress et CoreInstallationPaths sont des alias de contrats publics du SDK.
/// Un backend consomme cette interface ; il ne connaît ni le stockage ni l'implémentation du service.</remarks>
public interface ICoreReleaseService
{
    /// <summary>Recherche les versions proposées pour l'identité de cœur demandée.</summary>
    /// <param name="emulator">Identité typée du cœur dans le catalogue du module.</param>
    /// <param name="cancellationToken">Annulation à respecter avant la recherche et pendant les opérations éventuelles.</param>
    /// <returns>Versions disponibles avec leur identité, numéro et origine.</returns>
    Task<IReadOnlyList<CoreRelease>> GetAvailableAsync(Emulator emulator, CancellationToken cancellationToken = default);

    /// <summary>Installe la version demandée et retourne ses emplacements.</summary>
    /// <param name="release">Version du catalogue choisie par l'appelant.</param>
    /// <param name="progress">Récepteur emprunté de progression typée ; null désactive les notifications.</param>
    /// <param name="cancellationToken">Annulation de l'opération, sans transfert de propriété des services empruntés.</param>
    /// <returns>Chemins de l'installation utilisable par le fournisseur de cœur.</returns>
    Task<CoreInstallationPaths> InstallAsync(CoreRelease release, IProgress<CoreInstallProgress>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>Résout l'installation active du cœur sans le charger ni démarrer une machine.</summary>
    /// <param name="emulator">Identité typée dont il faut rechercher l'installation.</param>
    /// <param name="cancellationToken">Annulation de la résolution.</param>
    /// <returns>Emplacements actifs, ou null lorsqu'aucune installation n'est disponible.</returns>
    Task<CoreInstallationPaths?> GetActiveInstallationAsync(Emulator emulator, CancellationToken cancellationToken = default);
}
