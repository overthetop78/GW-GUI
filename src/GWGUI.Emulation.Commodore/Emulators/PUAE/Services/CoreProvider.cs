using GWGUI.Emulation.Commodore.Emulators.PUAE.Constants;
using GWGUI.Emulation.Commodore.Emulators.PUAE.Contracts;
using GWGUI.Emulation.Commodore.Emulators.PUAE.Factories;
using GWGUI.Emulation.Commodore.Emulators.PUAE.Functions;
using GWGUI.Emulation.Commodore.Emulators.PUAE.Services;

namespace GWGUI.Emulation.Commodore.Emulators.PUAE.Services;

public sealed class CoreProvider
{
    private readonly HttpClient _client;
    private readonly string _installationDirectory;

    public CoreProvider(HttpClient client, string installationDirectory)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
        _installationDirectory = installationDirectory;
    }

    public Task<string?> FindInstalledPathAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var installer = new ExternalCoreInstaller(_client, _installationDirectory);
        return Task.FromResult<string?>(installer.IsInstalled ? installer.LibraryPath : null);
    }
}
