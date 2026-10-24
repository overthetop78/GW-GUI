using EmulatorCatalogFunctions = GWGUI.Emulation.Sinclair.Common.Functions.EmulatorCatalogFunctions;

namespace GWGUI.Emulation.Sinclair.Common.Services;

/// <summary>Résout la bibliothèque active par le contrat Common d'installation des cœurs.</summary>
/// <remarks>Le service détermine le stockage et la version ; le contrôle facultatif valide le format de la bibliothèque.
/// Le fournisseur ne charge ni ne démarre de cœur. Le service et le callback sont empruntés à leur propriétaire.</remarks>
internal sealed class CoreProvider
{
    /// <summary>Service emprunté qui conserve les règles de recherche, de stockage et d'annulation.</summary>
    private readonly ICoreReleaseService _releases;

    /// <summary>Contrôle emprunté du format de bibliothèque ; null convient aux profils qui n'en demandent aucun.</summary>
    private readonly Action<string>? _verifyInstallation;

    /// <summary>Construit le fournisseur avec le service et le contrôle choisis par le module.</summary>
    /// <param name="releases">Contrat d'installation dont le fournisseur ne possède pas les ressources.</param>
    /// <param name="verifyInstallation">Contrôle du fichier avant utilisation, ou null pour vérifier seulement sa présence.</param>
    /// <remarks>Un backend natif peut fournir son contrôle PE et d'architecture. Un cœur intégré conserve null.
    /// Le callback ne transfère aucune propriété de bibliothèque et n'est appelé qu'après résolution de l'installation.</remarks>
    public CoreProvider(ICoreReleaseService releases, Action<string>? verifyInstallation = null)
    {
        ArgumentNullException.ThrowIfNull(releases);
        _releases = releases;
        _verifyInstallation = verifyInstallation;
    }

    /// <summary>Retourne la bibliothèque présente et validée de l'installation active du cœur demandé.</summary>
    /// <param name="core">Identité typée transmise au service d'installation.</param>
    /// <param name="cancellationToken">Annulation de la résolution avant toute validation du fichier.</param>
    /// <returns>Chemin de bibliothèque, ou null si l'installation est absente ou le contrôle refuse le fichier.</returns>
    /// <remarks>Le diagnostic commun traduit les erreurs d'entrée-sortie et de format du contrôle en absence de bibliothèque.
    /// Les autres erreurs conservent leur propagation. Aucun flux, cœur ou processus n'est conservé par ce fournisseur.</remarks>
    public async Task<string?> FindInstalledPathAsync(Emulator core, CancellationToken cancellationToken = default)
    {
        var installation = await _releases.GetActiveInstallationAsync(core, cancellationToken).ConfigureAwait(false);
        return installation is null ? null : EmulatorCatalogFunctions.Diagnostics.FindInstalledLibraryPath(installation.LibraryPath, _verifyInstallation);
    }
}
