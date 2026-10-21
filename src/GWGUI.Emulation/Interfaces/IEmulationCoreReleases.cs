using System.Net.Http;
using System.Text.Json;

namespace GWGUI.Emulation.Interfaces;

/// <summary>API commune de sélection et d’installation des versions de cœurs pour les modules indépendants.</summary>
/// <remarks>Le service ne possède aucun cœur ni client HTTP. Les opérations ferment leurs réponses et flux ;
/// les callbacks fournis restent sous la responsabilité du module, avec leurs règles de progression et d’annulation.</remarks>
public interface IEmulationCoreReleases
{
    /// <summary>Sélectionne une version connue ou refuse son identifiant.</summary>
    TRelease SelectRelease<TRelease>(IReadOnlyDictionary<string, TRelease> availableReleases,
        EmulationEmulatorRelease release);

    /// <summary>Sélectionne une version puis vérifie son appartenance à l’émulateur attendu.</summary>
    TRelease SelectRelease<TRelease>(IReadOnlyDictionary<string, TRelease> availableReleases,
        EmulationEmulatorRelease release, Func<TRelease, bool> matchesEmulator);

    /// <summary>Construit les chemins de bibliothèque et de manifeste sous la racine fournie.</summary>
    EmulationCoreInstallationPaths CreateInstallationPaths(string fullRoot, string coreId,
        string version, string libraryName, string manifestFileName);

    /// <summary>Adapte la progression typée en fraction ; ne crée rien si aucun récepteur n’est fourni.</summary>
    IProgress<TProgress>? CreateInstallProgress<TProgress>(IProgress<double>? progress,
        Func<TProgress, double?> fraction);

    /// <summary>Enchaîne téléchargement, extraction, manifeste, remplacement, écriture et activation.</summary>
    Task InstallLibraryAsync<TManifest>(Func<Task<long>> downloadArchive,
        Action extractLibrary, Func<long, TManifest> createManifest, Action replaceLibrary,
        Func<TManifest, Task> writeManifest, Func<Task> activateInstallation, Action<long> reportProgress);

    /// <summary>Exécute le même parcours, avec préparation facultative juste avant le remplacement.</summary>
    Task InstallLibraryAsync<TManifest>(Func<Task<long>> downloadArchive,
        Action extractLibrary, Func<long, TManifest> createManifest, Action replaceLibrary,
        Func<TManifest, Task> writeManifest, Func<Task> activateInstallation, Action<long> reportProgress,
        Func<Task>? prepareReplacement);

    /// <summary>Construit la version du catalogue depuis une réponse empruntée ; exige une date HTTP.</summary>
    EmulationCatalogCoreRelease<TEmulator> ParseRelease<TEmulator, TMachineModel>(
        EmulationCoreCatalogEntry<TEmulator, TMachineModel> entry, HttpResponseMessage response,
        string releaseIdPrefix, string releaseVersionFormat, Func<string> missingPublishedDate);

    /// <summary>Lit les métadonnées officielles ; la réponse reste possédée par son appelant.</summary>
    EmulationDownloadCoreRelease ReadOfficialRelease(EmulationCoreReleaseSettings settings,
        HttpResponseMessage response);

    /// <summary>Interroge les métadonnées HTTP ; le lecteur utilise la réponse avant sa libération.</summary>
    Task<T> QueryReleaseAsync<T>(HttpClient client, Uri archiveUri,
        Func<HttpResponseMessage, T> readResponse, CancellationToken cancellationToken);

    /// <summary>Télécharge un fichier en rapportant les octets écrits et le total éventuellement connu.</summary>
    Task<long> DownloadAsync(HttpClient client, Uri source, string destination,
        Action<long, long?>? progress, CancellationToken cancellationToken);

    /// <summary>Extrait la bibliothèque attendue, avec conservation facultative de la date de l’archive.</summary>
    void ExtractExpectedLibrary(string archivePath, string expectedName, string destination,
        Func<string, string> missingExpectedLibrary, bool preserveLastWriteTime = false);

    /// <summary>Lit la version installée ou conserve le repli en cas de document absent ou illisible.</summary>
    string ReadInstalledVersion(string manifestPath, string versionProperty, string unknownValue);

    /// <summary>Résout le marqueur actif puis vérifie la présence de la bibliothèque et du manifeste.</summary>
    Task<EmulationCoreInstallationPaths?> GetActiveInstallationAsync(string markerPath,
        Func<string, EmulationCoreInstallationPaths> pathsForVersion, CancellationToken cancellationToken);

    /// <summary>Écrit le manifeste temporaire puis remplace sa destination selon la politique commune.</summary>
    Task WriteJsonAtomicallyAsync<T>(string manifestPath, T value, CancellationToken cancellationToken);

    /// <summary>Remplace la bibliothèque ; traduit le refus d’accès avec le message différé fourni.</summary>
    void ReplaceLibraryAtomically(string source, string destination, Func<string, string> installedLibraryLocked);

    /// <summary>Écrit le document avec les options JSON du module et ferme son fichier.</summary>
    Task WriteJsonAsync<T>(string path, T value, JsonSerializerOptions options, CancellationToken cancellationToken);
}
