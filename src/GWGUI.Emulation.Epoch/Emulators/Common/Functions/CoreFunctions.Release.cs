using GWGUI.Emulation.Epoch.Emulators.Common.Constants;
using GWGUI.Emulation.Epoch.Emulators.Common.Contracts;
using GWGUI.Emulation.Epoch.Emulators.Common.Exceptions;
using System.Net.Http;
using System.IO;
using System.Globalization;
using System.IO.Compression;
using System.Text.Json;

namespace GWGUI.Emulation.Epoch.Emulators.Common.Functions;

internal static class CoreReleaseFunctions
{


    internal static CoreRelease ParseRelease(EmulatorCatalogEntry entry, HttpResponseMessage response)
    {
        var published = response.Content.Headers.LastModified ?? response.Headers.Date ?? throw new InvalidDataException(CoreReleaseErrors.MissingPublishedDate);
        var version = published.UtcDateTime.ToString(CoreReleaseConstants.ReleaseVersionFormat, CultureInfo.InvariantCulture);
        return new CoreRelease(entry.Emulator, CoreReleaseConstants.ReleaseIdPrefix + version, version, entry.ArchiveUri, published, response.Content.Headers.ContentLength);
    }

    internal static Task<long> DownloadAsync(HttpClient client, Uri source, string destination, IProgress<CoreInstallProgress>? progress, CancellationToken cancellationToken) =>
        EmulationCoreReleaseFunctions.DownloadAsync(client, source, destination,
            progress is null ? null : (written, total) => progress.Report(new CoreInstallProgress(written, total)), cancellationToken);

    internal static void ExtractExpectedLibrary(string archivePath, string expectedName, string destination) =>
        EmulationCoreReleaseFunctions.ExtractExpectedLibrary(archivePath, expectedName, destination,
            name => string.Format(CultureInfo.InvariantCulture, CoreReleaseErrors.MissingExpectedLibraryFormat, name));

    internal static Task WriteManifestAtomicallyAsync(string manifestPath, CoreDiagnosticManifest manifest, CancellationToken cancellationToken) => WriteJsonAtomicallyAsync(manifestPath, manifest, cancellationToken);

    internal static Task WriteActiveInstallationAtomicallyAsync(string manifestPath, CoreActiveInstallation installation, CancellationToken cancellationToken) => WriteJsonAtomicallyAsync(manifestPath, installation, cancellationToken);

    internal static Task<T?> ReadJsonAsync<T>(string path, CancellationToken cancellationToken) => EmulationCoreReleaseFunctions.ReadJsonAsync<T>(path, cancellationToken);

    private static Task WriteJsonAtomicallyAsync<T>(string manifestPath, T value, CancellationToken cancellationToken) => EmulationCoreReleaseFunctions.WriteJsonAtomicallyAsync(manifestPath, value, cancellationToken);

    internal static void ReplaceLibraryAtomically(string source, string destination) =>
        EmulationCoreReleaseFunctions.ReplaceLibraryAtomically(source, destination,
            path => string.Format(CultureInfo.InvariantCulture, CoreReleaseErrors.InstalledLibraryLockedFormat, path));

    internal static void DeleteIfExists(string path) => ConfigurationFileAccessFunctions.DeleteIfExists(path);
}
