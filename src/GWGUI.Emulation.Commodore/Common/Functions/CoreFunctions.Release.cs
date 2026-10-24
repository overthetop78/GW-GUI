using System.Text.Json;
using System.IO;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation;
namespace GWGUI.Emulation.Commodore.Common.Functions;

/// <summary>Fournit le contrat commun de sélection et installation des versions de cœur.</summary>
internal static class CoreReleaseFunctions
{
    /// <summary>Fournit le contrat commun de sélection et installation des versions de cœur.</summary>
    /// <value>Service emprunté du SDK, disponible pour tout module par son interface publique.</value>
    /// <remarks>Le client HTTP, les chemins, la progression et l’annulation sont empruntés pendant les opérations. Le service ferme ses réponses et flux ; Common conserve les adaptations et erreurs du module. Cet accès ne lance aucun téléchargement et ne possède aucun cœur.</remarks>
    internal static IEmulationCoreReleases Releases => EmulationSdk.CoreReleases;

    /// <summary>Écrit le manifeste de la distribution avec ses empreintes et sa date d’installation.</summary>
    internal static async Task WriteManifestAsync(string version, string source, string libraryPath, string sha256, CancellationToken cancellationToken)
    {
        var manifest = new { version, source, librarySize = new FileInfo(libraryPath).Length, librarySha256 = sha256, architecture = CoreReleaseConstants.X64, installedUtc = DateTimeOffset.UtcNow };
        await CoreReleaseFunctions.Releases.WriteJsonAsync(Path.Combine(Path.GetDirectoryName(libraryPath)!, CoreReleaseConstants.CoreJson), manifest, new JsonSerializerOptions { WriteIndented = true }, cancellationToken).ConfigureAwait(false);
    }
}
