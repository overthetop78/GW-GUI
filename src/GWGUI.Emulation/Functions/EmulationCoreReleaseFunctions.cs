using System.IO;
using System.Globalization;
using System.IO.Compression;
using System.Text.Json;

namespace GWGUI.Emulation.Functions;

/// <summary>Opérations communes de sélection, téléchargement et installation des cœurs.</summary>
/// <remarks>Le client HTTP est emprunté. Chaque opération conserve sa politique de progression, d’erreur et de remplacement ; les requêtes, réponses et flux créés sont libérés par leur propriétaire.</remarks>
internal sealed class EmulationCoreReleaseFunctions : IEmulationCoreReleases
{
    /// <summary>Sélectionne une release connue et applique le contrôle de correspondance lorsqu’il est fourni.</summary>
    public TRelease SelectRelease<TRelease>(IReadOnlyDictionary<string, TRelease> availableReleases,
        EmulationEmulatorRelease release)
    {
        if (!availableReleases.TryGetValue(release.Id, out var selected))
            throw new ArgumentException(nameof(release));
        return selected;
    }

    /// <summary>Sélectionne une release connue et applique le contrôle de correspondance lorsqu’il est fourni.</summary>
    public TRelease SelectRelease<TRelease>(IReadOnlyDictionary<string, TRelease> availableReleases,
        EmulationEmulatorRelease release, Func<TRelease, bool> matchesEmulator)
    {
        var selected = SelectRelease(availableReleases, release);
        if (!matchesEmulator(selected))
            throw new ArgumentException(nameof(release));
        return selected;
    }

    /// <summary>Construit les chemins de version, bibliothèque et manifeste dans la racine fournie.</summary>
    public EmulationCoreInstallationPaths CreateInstallationPaths(string fullRoot, string coreId,
        string version, string libraryName, string manifestFileName)
    {
        var versionDirectory = Path.Combine(fullRoot, coreId, version);
        return new EmulationCoreInstallationPaths(versionDirectory,
            Path.Combine(versionDirectory, libraryName),
            Path.Combine(versionDirectory, manifestFileName));
    }


    /// <summary>Adapte la progression typée vers sa fraction, sans créer d’adaptateur lorsque la progression est absente.</summary>
    public IProgress<TProgress>? CreateInstallProgress<TProgress>(IProgress<double>? progress,
        Func<TProgress, double?> fraction)
        => progress is null ? null : new Progress<TProgress>(value => progress.Report(fraction(value) ?? 0));

    /// <summary>Exécute téléchargement, extraction, manifeste, remplacement et activation dans leur ordre commun ; la préparation facultative précède le remplacement.</summary>
    public Task InstallLibraryAsync<TManifest>(Func<Task<long>> downloadArchive,
        Action extractLibrary, Func<long, TManifest> createManifest, Action replaceLibrary,
        Func<TManifest, Task> writeManifest, Func<Task> activateInstallation, Action<long> reportProgress)
        => InstallLibraryAsync(downloadArchive, extractLibrary, createManifest, replaceLibrary,
            writeManifest, activateInstallation, reportProgress, prepareReplacement: null);

    /// <summary>Exécute téléchargement, extraction, manifeste, remplacement et activation dans leur ordre commun ; la préparation facultative précède le remplacement.</summary>
    public async Task InstallLibraryAsync<TManifest>(Func<Task<long>> downloadArchive,
        Action extractLibrary, Func<long, TManifest> createManifest, Action replaceLibrary,
        Func<TManifest, Task> writeManifest, Func<Task> activateInstallation, Action<long> reportProgress,
        Func<Task>? prepareReplacement)
    {
        var archiveSize = await downloadArchive().ConfigureAwait(false);
        extractLibrary();
        var manifest = createManifest(archiveSize);
        if (prepareReplacement is not null)
            await prepareReplacement().ConfigureAwait(false);
        replaceLibrary();
        await writeManifest(manifest).ConfigureAwait(false);
        await activateInstallation().ConfigureAwait(false);
        reportProgress(archiveSize);
    }

    /// <summary>Construit une release de catalogue avec date obligatoire et format invariant.</summary>
    public EmulationCatalogCoreRelease<TEmulator> ParseRelease<TEmulator, TMachineModel>(
        EmulationCoreCatalogEntry<TEmulator, TMachineModel> entry, HttpResponseMessage response,
        string releaseIdPrefix, string releaseVersionFormat, Func<string> missingPublishedDate)
    {
        var published = response.Content.Headers.LastModified ?? response.Headers.Date
            ?? throw new InvalidDataException(missingPublishedDate());
        var version = published.UtcDateTime.ToString(releaseVersionFormat,
            CultureInfo.InvariantCulture);
        return new EmulationCatalogCoreRelease<TEmulator>(entry.Emulator, releaseIdPrefix + version,
            version, entry.ArchiveUri, published, response.Content.Headers.ContentLength);
    }

    /// <summary>Lit une release officielle avec date facultative et les règles de formatage prévues.</summary>
    public EmulationDownloadCoreRelease ReadOfficialRelease(EmulationCoreReleaseSettings settings,
        HttpResponseMessage response)
    {
        const string latest = "latest";
        var published = response.Content.Headers.LastModified ?? response.Headers.Date;
        var suffix = published?.UtcDateTime.ToString("yyyyMMdd-HHmm") ?? latest;
        return new EmulationDownloadCoreRelease($"official-{suffix}",
            published is null ? $"{settings.DisplayName} · {latest}"
                : $"{published.Value.LocalDateTime:dd/MM/yyyy HH:mm} · {settings.DisplayName}",
            settings.OfficialArchiveUri, published, true, true);
    }

    /// <summary>Interroge les métadonnées HTTP et transmet la réponse empruntée au lecteur avant sa libération.</summary>
    public async Task<T> QueryReleaseAsync<T>(HttpClient client, Uri archiveUri,
        Func<HttpResponseMessage, T> readResponse, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, archiveUri);
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return readResponse(response);
    }

    private static readonly JsonSerializerOptions ManifestJsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };



    /// <summary>Télécharge vers le fichier possédé par cette opération et rapporte les octets écrits avec le total éventuellement connu.</summary>
    public async Task<long> DownloadAsync(HttpClient client, Uri source, string destination,
        Action<long, long?>? progress, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(source, HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var total = response.Content.Headers.ContentLength;
        long written = 0;
        await using var input = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using var output = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None,
            81_920, FileOptions.Asynchronous);
        var buffer = new byte[81_920];
        int read;
        while ((read = await input.ReadAsync(buffer, cancellationToken).ConfigureAwait(false))
               > 0)
        {
            await output.WriteAsync(buffer.AsMemory(0, read),
                cancellationToken).ConfigureAwait(false);
            written += read;
            progress?.Invoke(written, total);
        }
        return written;
    }

    /// <summary>Extrait la bibliothèque attendue selon la politique de remplacement et de date fournie.</summary>
    public void ExtractExpectedLibrary(string archivePath, string expectedName, string destination,
        Func<string, string> missingExpectedLibrary, bool preserveLastWriteTime = false)
    {
        using var archive = ZipFile.OpenRead(archivePath);
        var entry = archive.Entries.FirstOrDefault(item =>
            Path.GetFileName(item.FullName).Equals(expectedName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidDataException(missingExpectedLibrary(expectedName));
        if (preserveLastWriteTime)
        {
            entry.ExtractToFile(destination, overwrite: true);
            return;
        }
        using var input = entry.Open();
        using var output = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None);
        input.CopyTo(output);
    }





    /// <summary>Lit un document JSON existant et libère son flux ; renvoie la valeur par défaut si le fichier manque.</summary>
    internal static async Task<T?> ReadJsonAsync<T>(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path)) return default;
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(stream, ManifestJsonOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Lit la propriété de version du manifeste ou conserve la valeur de repli prévue.</summary>
    public string ReadInstalledVersion(string manifestPath, string versionProperty, string unknownValue)
    {
        if (!File.Exists(manifestPath)) return unknownValue;
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
            return document.RootElement.TryGetProperty(versionProperty, out var version)
                ? version.GetString() ?? unknownValue
                : unknownValue;
        }
        catch (JsonException) { return unknownValue; }
        catch (IOException) { return unknownValue; }
    }

    /// <summary>Résout le marqueur actif avant de vérifier la bibliothèque et son manifeste.</summary>
    public async Task<EmulationCoreInstallationPaths?> GetActiveInstallationAsync(string markerPath,
        Func<string, EmulationCoreInstallationPaths> pathsForVersion, CancellationToken cancellationToken)
    {
        var marker = await ReadJsonAsync<EmulationCoreActiveInstallation>(
            markerPath, cancellationToken)
            .ConfigureAwait(false);
        if (marker is null) return null;
        var paths = pathsForVersion(marker.ReleaseVersion);
        return File.Exists(paths.LibraryPath) && File.Exists(paths.ManifestPath) ? paths : null;
    }

    /// <summary>Écrit le manifeste temporaire puis remplace sa destination avec le propriétaire d’écriture commun.</summary>
    public Task WriteJsonAtomicallyAsync<T>(string manifestPath, T value, CancellationToken cancellationToken) =>
        ConfigurationFileAccessFunctions.WriteAtomicallyAsync(manifestPath, manifestPath + ".temporary",
            temporary => WriteJsonAsync(temporary, value, ManifestJsonOptions, cancellationToken),
            static (source, target) => File.Move(source, target, overwrite: true));

    /// <summary>Remplace la bibliothèque installée et traduit le refus d’accès avec le message différé du module.</summary>
    public void ReplaceLibraryAtomically(string source, string destination, Func<string, string> installedLibraryLocked)
    {
        try
        {
            File.Move(source, destination, overwrite: true);
        }
        catch (UnauthorizedAccessException error)
        {
            throw new IOException(installedLibraryLocked(destination), error);
        }
    }

    /// <summary>Écrit le document avec les options de sérialisation explicitement fournies.</summary>
    public Task WriteJsonAsync<T>(string path, T value, JsonSerializerOptions options, CancellationToken cancellationToken) =>
        File.WriteAllTextAsync(path, JsonSerializer.Serialize(value, options), cancellationToken);
}
