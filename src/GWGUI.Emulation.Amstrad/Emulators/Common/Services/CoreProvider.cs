using GWGUI.Emulation.Amstrad.Emulators.Common.Contracts;
using GWGUI.Emulation.Amstrad.Emulators.Common.Services;

namespace GWGUI.Emulation.Amstrad.Emulators.Common.Services;

public sealed class CoreProvider
{
    private readonly HttpClient _client;
    private readonly string _installationDirectory;
    private readonly CoreDefinition _definition;

    internal CoreProvider(HttpClient client, string installationDirectory, CoreDefinition definition)
    {
        _definition = definition;
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
        _installationDirectory = installationDirectory;
    }

    public Task<string?> FindInstalledPathAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var installer = new ExternalCoreInstaller(_client, _installationDirectory, _definition);
        return Task.FromResult<string?>(installer.IsInstalled ? installer.LibraryPath : null);
    }
}

