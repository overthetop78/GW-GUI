using System.IO;
namespace GWGUI.Emulation.Commodore.Common.Services;
/// <summary>Résout une bibliothèque installée après contrôle de son profil, sans la charger.</summary>
internal sealed class CoreProvider
{
    private readonly string _installationDirectory;
    private readonly string _libraryFile;
    internal CoreProvider(HttpClient client, string installationDirectory, string libraryFile)
    {
        ArgumentNullException.ThrowIfNull(client);
        _installationDirectory = installationDirectory;
        _libraryFile = libraryFile;
    }
    public Task<string?> FindInstalledPathAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var libraryPath = Path.Combine(Path.GetFullPath(_installationDirectory), _libraryFile);
        return Task.FromResult(EmulatorCatalogFunctions.Diagnostics.FindInstalledLibraryPath(libraryPath, CoreReleaseService.VerifyWindowsX64Library));
    }
}
