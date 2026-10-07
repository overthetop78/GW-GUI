
namespace GWGUI.Emulation.Commodore.Emulators.Common.Interop.Services;

public sealed class CoreProvider
{
    private readonly CoreDefinition _definition;
    private readonly HttpClient _client;
    private readonly string _installationDirectory;

    internal CoreProvider(HttpClient client, string installationDirectory, CoreDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(client);
        _definition = definition;
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
