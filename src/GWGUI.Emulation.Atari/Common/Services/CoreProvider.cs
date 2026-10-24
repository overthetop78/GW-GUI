
namespace GWGUI.Emulation.Atari.Common.Services;

internal sealed class CoreProvider
{
    private readonly HttpClient _client;
    private readonly string _installationDirectory;
    private readonly Func<Emulator, EmulatorCatalogEntry> _getEntry;

    public CoreProvider(HttpClient client, string installationDirectory, Func<Emulator, EmulatorCatalogEntry> getEntry)
    {
        _client = client;
        _getEntry = getEntry;
        _installationDirectory = installationDirectory;
    }

    public async Task<string?> FindInstalledPathAsync(Emulator core, CancellationToken cancellationToken = default)
    {
        var installation = await new CoreReleaseService(_client, _installationDirectory, _getEntry).GetActiveInstallationAsync(core, cancellationToken).ConfigureAwait(false);
        return installation?.LibraryPath;
    }
}
