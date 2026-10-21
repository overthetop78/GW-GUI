using GWGUI.Emulation.Coleco.Emulators.Common.Constants;
using GWGUI.Emulation.Coleco.Emulators.Common.Contracts;
using GWGUI.Emulation.Coleco.Emulators.Common.Exceptions;
using System.Net.Http;
using System.IO;
using System.Globalization;
using System.IO.Compression;
using System.Text.Json;

namespace GWGUI.Emulation.Coleco.Emulators.Common.Functions;

internal static class CoreReleaseFunctions
{

    internal static Task<long> DownloadAsync(HttpClient client, Uri source, string destination, IProgress<CoreInstallProgress>? progress, CancellationToken cancellationToken) =>
        global::GWGUI.Emulation.EmulationSdk.CoreReleases.DownloadAsync(client, source, destination,
            progress is null ? null : (written, total) => progress.Report(new CoreInstallProgress(written, total)), cancellationToken);

    internal static void ExtractExpectedLibrary(string archivePath, string expectedName, string destination) =>
        global::GWGUI.Emulation.EmulationSdk.CoreReleases.ExtractExpectedLibrary(archivePath, expectedName, destination,
            name => string.Format(CultureInfo.InvariantCulture, CoreReleaseErrors.MissingExpectedLibraryFormat, name));

    internal static Task WriteManifestAtomicallyAsync(string manifestPath, CoreDiagnosticManifest manifest, CancellationToken cancellationToken) => WriteJsonAtomicallyAsync(manifestPath, manifest, cancellationToken);

    internal static Task WriteActiveInstallationAtomicallyAsync(string manifestPath, CoreActiveInstallation installation, CancellationToken cancellationToken) => WriteJsonAtomicallyAsync(manifestPath, installation, cancellationToken);

    private static Task WriteJsonAtomicallyAsync<T>(string manifestPath, T value, CancellationToken cancellationToken) => global::GWGUI.Emulation.EmulationSdk.CoreReleases.WriteJsonAtomicallyAsync(manifestPath, value, cancellationToken);

    internal static void ReplaceLibraryAtomically(string source, string destination) =>
        global::GWGUI.Emulation.EmulationSdk.CoreReleases.ReplaceLibraryAtomically(source, destination,
            path => string.Format(CultureInfo.InvariantCulture, CoreReleaseErrors.InstalledLibraryLockedFormat, path));

    internal static void DeleteIfExists(string path) => ConfigurationFileAccessFunctions.DeleteIfExists(path);
}
