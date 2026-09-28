using GWGUI.Emulation.Nintendo.Emulators.Gambatte.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Gambatte.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Gambatte.Factories;
using GWGUI.Emulation.Nintendo.Emulators.Gambatte.Functions;
using GWGUI.Emulation.Nintendo.Emulators.Gambatte.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.Gambatte.Services;

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



