using GWGUI.Emulation.Sega.Emulators.Yabause.Constants;
using GWGUI.Emulation.Sega.Emulators.Yabause.Contracts;
using GWGUI.Emulation.Sega.Emulators.Yabause.Factories;
using GWGUI.Emulation.Sega.Emulators.Yabause.Functions;
using GWGUI.Emulation.Sega.Emulators.Yabause.Services;

namespace GWGUI.Emulation.Sega.Emulators.Yabause.Services;

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

