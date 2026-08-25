namespace GWGUI.Emulation.Amiga.Services;

public sealed class AmigaCoreProvider
{
    private readonly HttpClient _client;
    private readonly string _installationDirectory;

    public AmigaCoreProvider(HttpClient client, string installationDirectory)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
        _installationDirectory = installationDirectory;
    }

    public Task<string?> FindInstalledPathAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var installer = new AmigaExternalCoreInstaller(_client, _installationDirectory);
        return Task.FromResult<string?>(installer.IsInstalled ? installer.LibraryPath : null);
    }
}
