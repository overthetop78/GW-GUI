using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation;
namespace GWGUI.Emulation.Example.Common.Functions;

/// <summary>Catalogue Common des versions simulées et accès au diagnostic public des bibliothèques.</summary>
internal static class EmulatorCatalogFunctions
{
    /// <summary>Fournit le contrat commun de diagnostic des bibliothèques de cœur.</summary>
    /// <value>Service emprunté du SDK, disponible pour tout module par son interface publique.</value>
    /// <remarks>Le service inspecte les fichiers sans charger ni démarrer le cœur et ferme ses flux de lecture. Les chemins, identités et callbacks de validation restent fournis par le module ; aucune identité de module connue du SDK n’est nécessaire.</remarks>
    internal static IEmulationCoreDiagnostics Diagnostics => EmulationSdk.CoreDiagnostics;

    /// <summary>Décrit la version de backend livrée dans l’assembly déjà présent.</summary>
    /// <param name="definition">Description empruntée au catalogue de l’adaptateur ; aucune instance de cœur n’est créée.</param>
    /// <param name="cancellationToken">Annulation vérifiée avant de produire la description.</param>
    /// <returns>Installation intégrée disponible pour cette définition et la version du module.</returns>
    internal static ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(EmulationEmulatorDefinition definition, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(new EmulationEmulatorInstallation(definition, Engine.CoreVersion));
    }

    /// <summary>Présente la version intégrée sous le contrat de publication utilisé par les autres modules.</summary>
    /// <param name="definition">Identifiant et libellé du backend, conservés dans la publication proposée.</param>
    /// <param name="cancellationToken">Annulation vérifiée avant de construire la collection.</param>
    /// <returns>Une seule publication requise, consultable sans requête HTTP ni archive à créer.</returns>
    internal static ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(EmulationEmulatorDefinition definition, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<IReadOnlyList<EmulationEmulatorRelease>>([new(definition.Id, definition.DisplayName + " " + Engine.CoreVersion, Engine.CoreVersion, IsRequired: true)]);
    }

    /// <summary>Valide la publication demandée puis retourne l’implémentation déjà intégrée.</summary>
    /// <param name="definition">Backend auquel la publication doit appartenir.</param>
    /// <param name="release">Publication sélectionnée : son identifiant et sa version sont contrôlés avant tout signal de progression.</param>
    /// <param name="progress">Récepteur emprunté, informé une seule fois du succès avec la fraction 1.</param>
    /// <param name="cancellationToken">Annulation contrôlée avant validation et notification.</param>
    /// <returns>Chemin de l’assembly existant portant les exports simulés. Aucun fichier, processus ou cœur n’est créé.</returns>
    /// <exception cref="ArgumentOutOfRangeException">La publication ne correspond pas au backend ou à la version intégrée.</exception>
    internal static ValueTask<string> InstallAsync(EmulationEmulatorDefinition definition, EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (release.Id != definition.Id || release.Version != Engine.CoreVersion) throw new ArgumentOutOfRangeException(nameof(release));
        progress?.Report(1);
        return ValueTask.FromResult(Engine.CoreImplementationPath);
    }

    /// <summary>Résout l’implémentation locale déjà fournie par le build de l’application.</summary>
    /// <param name="cancellationToken">Annulation vérifiée avant la résolution.</param>
    /// <returns>Chemin de l’assembly emprunté ; ce service ne le charge pas, ne le copie pas et ne le détruit pas.</returns>
    internal static ValueTask<string?> FindInstalledCorePathAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<string?>(Engine.CoreImplementationPath);
    }
}
