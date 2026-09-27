
namespace GWGUI.Emulation.Atari.Common.Services;

public sealed class CoreProvider
{
    private readonly HttpClient _client;
    private readonly string _installationDirectory;

    public CoreProvider(HttpClient client, string installationDirectory)
    {
        _client = client;
        _installationDirectory = installationDirectory;
    }

    public async Task<string?> FindInstalledPathAsync(Emulator core,
        CancellationToken cancellationToken = default)
    {
        var installation = await new CoreReleaseService(_client, _installationDirectory)
            .GetActiveInstallationAsync(core, cancellationToken).ConfigureAwait(false);
        return installation?.LibraryPath;
    }
}
