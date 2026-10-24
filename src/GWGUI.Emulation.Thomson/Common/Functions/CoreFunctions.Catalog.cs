using System.IO;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation;
namespace GWGUI.Emulation.Thomson.Common.Functions;

/// <summary>Fournit le contrat commun de diagnostic des bibliothèques de cœur.</summary>
internal static class EmulatorCatalogFunctions
{
    /// <summary>Fournit le contrat commun de diagnostic des bibliothèques de cœur.</summary>
    /// <value>Service emprunté du SDK, disponible pour tout module par son interface publique.</value>
    /// <remarks>Le service inspecte les fichiers sans charger ni démarrer le cœur et ferme ses flux de lecture. Les chemins, identités et callbacks de validation restent fournis par le module ; aucune identité de module connue du SDK n’est nécessaire.</remarks>
    internal static IEmulationCoreDiagnostics Diagnostics => EmulationSdk.CoreDiagnostics;

    /// <summary>Construit les chemins d’une installation après validation des noms et de la racine.</summary>
    /// <param name="entry">Description déjà sélectionnée et validée par le catalogue du backend.</param>
    /// <param name="installationRoot">Racine fournie par le gestionnaire, sans création de répertoire.</param>
    /// <param name="version">Composante de version qui doit rester un seul nom de répertoire.</param>
    /// <returns>Chemins gérés de la bibliothèque et de son manifeste ; aucun fichier n’est ouvert.</returns>
    internal static CoreInstallationPaths GetInstallationPaths(EmulatorCatalogEntry entry, string installationRoot, string version)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(installationRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        if (version != Path.GetFileName(version) || version is "." or ".." || version.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new ArgumentException(nameof(version));
        return CoreReleaseFunctions.Releases.CreateInstallationPaths(Path.GetFullPath(installationRoot), entry.Id,
            version, entry.DllName, EmulatorCatalogConstants.ManifestFileName);
    }

    /// <summary>Construit le chemin du marqueur actif pour la description déjà sélectionnée.</summary>
    /// <remarks>La consultation ne lit ni n’écrit le marqueur et conserve le nom de fichier du module.</remarks>
    internal static string GetActiveManifestPath(EmulatorCatalogEntry entry, string installationRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(installationRoot);
        return Path.Combine(Path.GetFullPath(installationRoot), entry.Id, EmulatorCatalogConstants.ActiveManifestFileName);
    }
}
