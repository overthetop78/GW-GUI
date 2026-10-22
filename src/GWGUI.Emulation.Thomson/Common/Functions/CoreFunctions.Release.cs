using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation;
using GWGUI.Emulation.Thomson.Emulators.Theodore.Constants;
using GWGUI.Emulation.Thomson.Emulators.Theodore.Exceptions;
using System.Net.Http;
using System.IO;
using System.Globalization;
using System.IO.Compression;
using System.Text.Json;

namespace GWGUI.Emulation.Thomson.Common.Functions;

internal static class CoreReleaseFunctions
{
    /// <summary>Fournit le contrat commun de sélection et installation des versions de cœur.</summary>
    /// <value>Service emprunté du SDK, disponible pour tout module par son interface publique.</value>
    /// <remarks>Le client HTTP, les chemins, la progression et l’annulation sont empruntés pendant les opérations. Le service ferme ses réponses et flux ; Common conserve les adaptations et erreurs du module. Cet accès ne lance aucun téléchargement et ne possède aucun cœur.</remarks>
    internal static IEmulationCoreReleases Releases => EmulationSdk.CoreReleases;


    internal static Task<long> DownloadAsync(HttpClient client, Uri source, string destination, IProgress<CoreInstallProgress>? progress, CancellationToken cancellationToken) =>
        CoreReleaseFunctions.Releases.DownloadAsync(client, source, destination,
            progress is null ? null : (written, total) => progress.Report(new CoreInstallProgress(written, total)), cancellationToken);

    internal static void ExtractExpectedLibrary(string archivePath, string expectedName, string destination) =>
        CoreReleaseFunctions.Releases.ExtractExpectedLibrary(archivePath, expectedName, destination,
            name => string.Format(CultureInfo.InvariantCulture, CoreReleaseErrors.MissingExpectedLibraryFormat, name));

    internal static Task WriteManifestAtomicallyAsync(string manifestPath, CoreDiagnosticManifest manifest, CancellationToken cancellationToken) => WriteJsonAtomicallyAsync(manifestPath, manifest, cancellationToken);

    internal static Task WriteActiveInstallationAtomicallyAsync(string manifestPath, CoreActiveInstallation installation, CancellationToken cancellationToken) => WriteJsonAtomicallyAsync(manifestPath, installation, cancellationToken);

    private static Task WriteJsonAtomicallyAsync<T>(string manifestPath, T value, CancellationToken cancellationToken) => CoreReleaseFunctions.Releases.WriteJsonAtomicallyAsync(manifestPath, value, cancellationToken);

    internal static void ReplaceLibraryAtomically(string source, string destination) =>
        CoreReleaseFunctions.Releases.ReplaceLibraryAtomically(source, destination,
            path => string.Format(CultureInfo.InvariantCulture, CoreReleaseErrors.InstalledLibraryLockedFormat, path));

    internal static void DeleteIfExists(string path) => ConfigurationFileAccessFunctions.DeleteIfExists(path);
}
