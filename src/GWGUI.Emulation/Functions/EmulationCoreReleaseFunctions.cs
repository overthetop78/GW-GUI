using System.IO;
using System.Globalization;
using System.IO.Compression;
using System.Text.Json;

namespace GWGUI.Emulation.Functions;

internal static class EmulationCoreReleaseFunctions
{
    internal static TRelease SelectRelease<TRelease>(IReadOnlyDictionary<string, TRelease> availableReleases,
        EmulationEmulatorRelease release)
    {
        if (!availableReleases.TryGetValue(release.Id, out var selected))
            throw new ArgumentException(nameof(release));
        return selected;
    }

    internal static TRelease SelectRelease<TRelease>(IReadOnlyDictionary<string, TRelease> availableReleases,
        EmulationEmulatorRelease release, Func<TRelease, bool> matchesEmulator)
    {
        var selected = SelectRelease(availableReleases, release);
        if (!matchesEmulator(selected))
            throw new ArgumentException(nameof(release));
        return selected;
    }

    internal static EmulationCoreInstallationPaths CreateInstallationPaths(string fullRoot, string coreId,
        string version, string libraryName, string manifestFileName)
    {
        var versionDirectory = Path.Combine(fullRoot, coreId, version);
        return new EmulationCoreInstallationPaths(versionDirectory,
            Path.Combine(versionDirectory, libraryName),
            Path.Combine(versionDirectory, manifestFileName));
    }


    internal static IProgress<TProgress>? CreateInstallProgress<TProgress>(IProgress<double>? progress,
        Func<TProgress, double?> fraction)
        => progress is null ? null : new Progress<TProgress>(value => progress.Report(fraction(value) ?? 0));

    internal static Task InstallLibraryAsync<TManifest>(Func<Task<long>> downloadArchive,
        Action extractLibrary, Func<long, TManifest> createManifest, Action replaceLibrary,
        Func<TManifest, Task> writeManifest, Func<Task> activateInstallation, Action<long> reportProgress)
        => InstallLibraryAsync(downloadArchive, extractLibrary, createManifest, replaceLibrary,
            writeManifest, activateInstallation, reportProgress, prepareReplacement: null);

    internal static async Task InstallLibraryAsync<TManifest>(Func<Task<long>> downloadArchive,
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

    internal static async Task<T> QueryReleaseAsync<T>(HttpClient client, Uri archiveUri,
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



    internal static async Task<long> DownloadAsync(HttpClient client, Uri source, string destination,
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

    internal static void ExtractExpectedLibrary(string archivePath, string expectedName, string destination,
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





    internal static async Task<T?> ReadJsonAsync<T>(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path)) return default;
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(stream, ManifestJsonOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    internal static Task WriteJsonAtomicallyAsync<T>(string manifestPath, T value, CancellationToken cancellationToken) =>
        ConfigurationFileAccessFunctions.WriteAtomicallyAsync(manifestPath, manifestPath + ".temporary",
            temporary => WriteJsonAsync(temporary, value, ManifestJsonOptions, cancellationToken),
            static (source, target) => File.Move(source, target, overwrite: true));

    internal static void ReplaceLibraryAtomically(string source, string destination, Func<string, string> installedLibraryLocked)
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

    internal static Task WriteJsonAsync<T>(string path, T value, JsonSerializerOptions options, CancellationToken cancellationToken) =>
        File.WriteAllTextAsync(path, JsonSerializer.Serialize(value, options), cancellationToken);
}
