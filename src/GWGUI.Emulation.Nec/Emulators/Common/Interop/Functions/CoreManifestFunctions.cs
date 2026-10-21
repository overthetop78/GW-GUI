using GWGUI.Emulation;
using System.IO;
using System.Text.Json;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;

internal static class CoreManifestFunctions
{
    internal static async Task WriteManifestAsync(string version, string source, string libraryPath, string sha256, CancellationToken cancellationToken)
    {
        var manifest = new { version, source, librarySize = new FileInfo(libraryPath).Length, librarySha256 = sha256, architecture = CoreReleaseConstants.X64, installedUtc = DateTimeOffset.UtcNow };
        await EmulationSdk.CoreReleases.WriteJsonAsync(Path.Combine(Path.GetDirectoryName(libraryPath)!, CoreReleaseConstants.CoreJson), manifest, new JsonSerializerOptions { WriteIndented = true }, cancellationToken).ConfigureAwait(false);
    }

}
