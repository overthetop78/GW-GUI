using System.IO;
using System.Net.Http;
using GWGUI.Emulation.Amiga;

namespace GWGUI.App.Services;

internal static class AmigaCoreProvider
{
    private static readonly HttpClient Client = new();
    private static readonly SemaphoreSlim Gate = new(1, 1);

    internal static async Task<string> EnsureAvailableAsync(CancellationToken cancellationToken = default)
    {
        var bundled = Path.Combine(AppContext.BaseDirectory, "Emulation", "puae_libretro.dll");
        if (File.Exists(bundled)) return bundled;
        await Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var installer = new AmigaExternalCoreInstaller(Client, StoragePaths.AmigaCoreDirectory);
            return installer.IsInstalled ? installer.LibraryPath : await installer.InstallAsync(cancellationToken).ConfigureAwait(false);
        }
        finally { Gate.Release(); }
    }
}
