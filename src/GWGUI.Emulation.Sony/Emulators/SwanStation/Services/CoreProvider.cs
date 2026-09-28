using GWGUI.Emulation.Sony.Emulators.SwanStation.Constants;
using GWGUI.Emulation.Sony.Emulators.SwanStation.Contracts;
using GWGUI.Emulation.Sony.Emulators.SwanStation.Factories;
using GWGUI.Emulation.Sony.Emulators.SwanStation.Functions;
using GWGUI.Emulation.Sony.Emulators.SwanStation.Services;

namespace GWGUI.Emulation.Sony.Emulators.SwanStation.Services;

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



