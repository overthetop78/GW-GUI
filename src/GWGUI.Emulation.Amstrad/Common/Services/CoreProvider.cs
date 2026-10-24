using System.IO;
namespace GWGUI.Emulation.Amstrad.Common.Services;
/// <summary>Résout une bibliothèque installée après contrôle de son profil, sans la charger.</summary>
internal sealed class CoreProvider
{
    private readonly string _installationDirectory;
    private readonly string _libraryFile;
    private readonly Action<string>? _verifyInstallation;
    internal CoreProvider(HttpClient client, string installationDirectory, string libraryFile, Action<string>? verifyInstallation)
    {
        ArgumentNullException.ThrowIfNull(client);
        _installationDirectory = installationDirectory;
        _libraryFile = libraryFile;
        _verifyInstallation = verifyInstallation;
    }
    public Task<string?> FindInstalledPathAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var libraryPath = Path.Combine(Path.GetFullPath(_installationDirectory), _libraryFile);
        return Task.FromResult(EmulatorCatalogFunctions.Diagnostics.FindInstalledLibraryPath(libraryPath, _verifyInstallation));
    }
}
