using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation;
using System.IO;
namespace GWGUI.Emulation.Example.Common.Functions;

/// <summary>Fonctions Common de catalogue et accès au diagnostic public des bibliothèques.</summary>
internal static class EmulatorCatalogFunctions
{
    /// <summary>Fournit le contrat commun de diagnostic des bibliothèques de cœur.</summary>
    /// <value>Service emprunté du SDK, disponible pour tout module par son interface publique.</value>
    /// <remarks>Le service inspecte les fichiers sans charger ni démarrer le cœur et ferme ses flux de lecture. Les chemins, identités et callbacks de validation restent fournis par le module ; aucune identité de module connue du SDK n’est nécessaire.</remarks>
    internal static IEmulationCoreDiagnostics Diagnostics => EmulationSdk.CoreDiagnostics;

    /// <summary>Décrit les emplacements de l'implémentation déjà intégrée au build.</summary>
    /// <returns>Dossier existant du module et chemin de son assembly ; ManifestPath est vide pour ce profil préinstallé.</returns>
    /// <remarks>Les trois simulations partagent le même fichier. Aucun dossier de version ni manifeste d'une
    /// bibliothèque téléchargée n'est créé : le build est propriétaire de cet assembly et de sa version.
    /// InstalledVersion provient de la version du cœur intégré, et non du nom du dossier où l'application est lancée.
    /// Les services de versions et CoreProvider utilisent ce même résultat sans charger le cœur.</remarks>
    internal static CoreInstallationPaths GetInstallationPaths() => new(Path.GetDirectoryName(Engine.CoreImplementationPath)!, Engine.CoreImplementationPath, string.Empty) { InstalledVersion = Engine.CoreVersion };

}
