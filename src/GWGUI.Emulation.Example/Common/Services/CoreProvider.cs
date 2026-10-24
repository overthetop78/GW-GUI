namespace GWGUI.Emulation.Example.Common.Services;

/// <summary>Résout le chemin d'un cœur en passant par le contrat Common des installations.</summary>
/// <remarks>Le fournisseur reçoit son service ; il ne dépend pas d'une classe de backend et ne charge pas la bibliothèque.
/// Dans cet exemple, les trois simulations sont déjà livrées dans l'assembly du module.
/// La résolution ne crée donc ni archive, ni copie de DLL, ni processus.</remarks>
internal sealed class CoreProvider
{
    /// <summary>Service emprunté : son propriétaire conserve sa durée de vie et ses éventuelles ressources.</summary>
    private readonly ICoreReleaseService _releases;

    /// <summary>Construit le fournisseur avec le service de versions choisi par Common.</summary>
    /// <param name="releases">Contrat d'installation ; le fournisseur ne le remplace pas et ne le détruit pas.</param>
    public CoreProvider(ICoreReleaseService releases)
    {
        ArgumentNullException.ThrowIfNull(releases);
        _releases = releases;
    }

    /// <summary>Retourne la bibliothèque de l'installation active du cœur demandé.</summary>
    /// <param name="core">Identité typée du cœur à résoudre.</param>
    /// <param name="cancellationToken">Annulation transmise à la résolution de l'installation.</param>
    /// <returns>Chemin emprunté de la bibliothèque, ou null si le service ne trouve aucune installation.</returns>
    /// <remarks>La même opération sert aux cœurs natifs et simulés ; seul le service d'installation varie.</remarks>
    public async Task<string?> FindInstalledPathAsync(Emulator core, CancellationToken cancellationToken = default)
    {
        var installation = await _releases.GetActiveInstallationAsync(core, cancellationToken).ConfigureAwait(false);
        return installation?.LibraryPath;
    }
}
