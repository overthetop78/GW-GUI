using System.Globalization;
using System.IO.Compression;
using System.Text.Json;

namespace GWGUI.Emulation.Atari.Emulators.Libretro.Functions;

internal static class CoreReleaseFunctions
{
    private static readonly JsonSerializerOptions ManifestJsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    internal static CoreRelease ParseRelease(EmulatorCatalogEntry entry, HttpResponseMessage response)
    {
        var published = response.Content.Headers.LastModified ?? response.Headers.Date
            ?? throw new InvalidDataException(CoreReleaseErrors.MissingPublishedDate);
        var version = published.UtcDateTime.ToString(CoreReleaseConstants.ReleaseVersionFormat,
            CultureInfo.InvariantCulture);
        return new CoreRelease(entry.Emulator, CoreReleaseConstants.ReleaseIdPrefix + version,
            version, entry.ArchiveUri, published, response.Content.Headers.ContentLength);
    }

    internal static async Task<long> DownloadAsync(HttpClient client, Uri source, string destination,
        IProgress<CoreInstallProgress>? progress, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(source, HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var total = response.Content.Headers.ContentLength;
        long written = BufferConstants.FirstBufferIndex;
        await using var input = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using var output = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None,
            CoreReleaseConstants.DownloadBufferSize, FileOptions.Asynchronous);
        var buffer = new byte[CoreReleaseConstants.DownloadBufferSize];
        int read;
        while ((read = await input.ReadAsync(buffer, cancellationToken).ConfigureAwait(false))
               > BufferConstants.FirstBufferIndex)
        {
            await output.WriteAsync(buffer.AsMemory(BufferConstants.FirstBufferIndex, read),
                cancellationToken).ConfigureAwait(false);
            written += read;
            progress?.Report(new CoreInstallProgress(written, total));
        }
        return written;
    }

    internal static void ExtractExpectedLibrary(string archivePath, string expectedName, string destination)
    {
        using var archive = ZipFile.OpenRead(archivePath);
        var entry = archive.Entries.FirstOrDefault(item =>
            Path.GetFileName(item.FullName).Equals(expectedName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture,
                CoreReleaseErrors.MissingExpectedLibraryFormat, expectedName));
        using var input = entry.Open();
        using var output = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None);
        input.CopyTo(output);
    }

    internal static Task WriteManifestAtomicallyAsync(string manifestPath,
        CoreDiagnosticManifest manifest, CancellationToken cancellationToken) =>
        WriteJsonAtomicallyAsync(manifestPath, manifest, cancellationToken);

    internal static Task WriteActiveInstallationAtomicallyAsync(string manifestPath,
        CoreActiveInstallation installation, CancellationToken cancellationToken) =>
        WriteJsonAtomicallyAsync(manifestPath, installation, cancellationToken);

    internal static async Task<T?> ReadJsonAsync<T>(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path)) return default;
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(stream, ManifestJsonOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task WriteJsonAtomicallyAsync<T>(string manifestPath,
        T value, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(manifestPath)!);
        var temporary = manifestPath + CoreReleaseConstants.TemporaryManifestExtension;
        try
        {
            await File.WriteAllTextAsync(temporary,
                JsonSerializer.Serialize(value, ManifestJsonOptions),
                cancellationToken).ConfigureAwait(false);
            File.Move(temporary, manifestPath, overwrite: true);
        }
        finally
        {
            DeleteIfExists(temporary);
        }
    }

    internal static void ReplaceLibraryAtomically(string source, string destination)
    {
        try
        {
            File.Move(source, destination, overwrite: true);
        }
        catch (UnauthorizedAccessException error)
        {
            throw new IOException(string.Format(CultureInfo.InvariantCulture,
                CoreReleaseErrors.InstalledLibraryLockedFormat, destination), error);
        }
    }

    internal static void DeleteIfExists(string path)
    {
        if (File.Exists(path)) File.Delete(path);
    }
}
