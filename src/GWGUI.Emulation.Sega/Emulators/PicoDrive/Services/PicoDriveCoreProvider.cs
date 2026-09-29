using System.IO;
using GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Services;

namespace GWGUI.Emulation.Sega.Emulators.PicoDrive.Services;

internal sealed class PicoDriveCoreProvider
{
    private readonly string _installationDirectory;

    internal PicoDriveCoreProvider(string installationDirectory) =>
        _installationDirectory = Path.GetFullPath(installationDirectory);

    internal Task<string?> FindInstalledPathAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = Path.Combine(_installationDirectory, Constants.PicoDriveConstants.LibraryFileName);
        if (!File.Exists(path)) return Task.FromResult<string?>(null);
        try
        {
            CoreReleaseService.VerifyWindowsX64Library(path);
            return Task.FromResult<string?>(path);
        }
        catch (IOException) { return Task.FromResult<string?>(null); }
        catch (InvalidDataException) { return Task.FromResult<string?>(null); }
    }
}
