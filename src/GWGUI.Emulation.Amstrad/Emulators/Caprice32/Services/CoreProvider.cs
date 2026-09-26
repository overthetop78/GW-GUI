using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Constants;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Contracts;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Factories;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Functions;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Services;

namespace GWGUI.Emulation.Amstrad.Emulators.Caprice32.Services;

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

