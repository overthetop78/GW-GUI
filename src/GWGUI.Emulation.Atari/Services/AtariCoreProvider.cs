using GWGUI.Emulation.Atari.Cores;

namespace GWGUI.Emulation.Atari;

public sealed class AtariCoreProvider
{
    private readonly HttpClient _client;
    private readonly string _installationDirectory;

    public AtariCoreProvider(HttpClient client, string installationDirectory)
    {
        _client = client;
        _installationDirectory = installationDirectory;
    }

    public async Task<string?> FindInstalledPathAsync(AtariEmulator core,
        CancellationToken cancellationToken = default)
    {
        var installation = await new AtariCoreReleaseService(_client, _installationDirectory)
            .GetActiveInstallationAsync(core, cancellationToken).ConfigureAwait(false);
        return installation?.LibraryPath;
    }
}
