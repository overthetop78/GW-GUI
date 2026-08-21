namespace GWGUI.Emulation.Amiga;

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

    public Task<string?> FindInstalledPathAsync(string? bundledPath = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!string.IsNullOrWhiteSpace(bundledPath) && File.Exists(bundledPath))
            return Task.FromResult<string?>(bundledPath);
        var installer = new AmigaExternalCoreInstaller(_client, _installationDirectory);
        return Task.FromResult<string?>(installer.IsInstalled ? installer.LibraryPath : null);
    }
}
