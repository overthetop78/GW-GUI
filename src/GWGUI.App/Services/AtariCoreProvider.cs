using GWGUI.Emulation.Atari;
using GWGUI.Emulation.Atari.Cores;
using System.Net.Http;

namespace GWGUI.App.Services;

internal static class AtariCoreProvider
{
    internal static async Task<string> GetInstalledPathAsync(AtariCoreKind kind,
        CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient();
        var installation = await new AtariCoreReleaseService(client, StoragePaths.AtariCoreDirectory)
            .GetActiveInstallationAsync(kind, cancellationToken).ConfigureAwait(false);
        if (installation is null)
            throw new AtariEmulationException(AtariErrorKind.Core, AtariErrorCode.CoreNotFound,
                string.Format(System.Globalization.CultureInfo.CurrentCulture,
                    AtariCoreProviderConstants.CoreNotInstalledFormat, AtariCoreCatalog.Get(kind).LibraryName));
        return installation.LibraryPath;
    }
}
