namespace GWGUI.Emulation.Apple.Common.Services;

/// <summary>Résout la bibliothèque active par le contrat Common des installations de cœurs.</summary>
/// <remarks>Le fournisseur emprunte le service reçu ; il ne connaît aucune classe de backend,
/// ne possède pas le client HTTP et ne charge ni ne démarre le cœur.</remarks>
internal sealed class CoreProvider
{
    /// <summary>Service emprunté qui conserve les règles du catalogue, du stockage et de l'annulation.</summary>
    private readonly ICoreReleaseService _releases;

    /// <summary>Construit le fournisseur avec l'implémentation de catalogue choisie par Common.</summary>
    /// <param name="releases">Contrat d'installation ; ses éventuelles ressources gardent leur propriétaire.</param>
    public CoreProvider(ICoreReleaseService releases)
    {
        ArgumentNullException.ThrowIfNull(releases);
        _releases = releases;
    }

    /// <summary>Retourne le chemin de bibliothèque de l'installation active du cœur demandé.</summary>
    /// <param name="core">Identité typée du cœur à résoudre.</param>
    /// <param name="cancellationToken">Annulation transmise à la recherche de l'installation.</param>
    /// <returns>Chemin de l'installation active, ou null si aucune installation n'est disponible.</returns>
    public async Task<string?> FindInstalledPathAsync(Emulator core, CancellationToken cancellationToken = default)
    {
        var installation = await _releases.GetActiveInstallationAsync(core, cancellationToken).ConfigureAwait(false);
        return installation?.LibraryPath;
    }
}