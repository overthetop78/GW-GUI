using System.IO;
using System.IO.Compression;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Constants;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Exceptions;

namespace GWGUI.Emulation.Sony.Emulators.Ppsspp.Functions;

internal static class SystemFilesFunctions
{
    internal static bool IsInstalled(string directory) => SystemFilesConstants.RequiredFiles.All(file => File.Exists(Path.Combine(directory, SystemFilesConstants.DirectoryName, file)));

    internal static void Validate(string directory)
    {
        foreach (var file in SystemFilesConstants.RequiredFiles)
        {
            var path = Path.Combine(directory, SystemFilesConstants.DirectoryName, file);
            if (!File.Exists(path)) throw new FileNotFoundException(PpssppExceptions.SystemFilesMissing(), path);
        }
    }

    internal static async Task InstallAsync(HttpClient client, string directory, CancellationToken cancellationToken)
    {
        var root = Path.GetFullPath(directory);
        var staging = Path.Combine(root, SystemFilesConstants.StagingPrefix + Guid.NewGuid().ToString("N"));
        var previous = staging + SystemFilesConstants.PreviousSuffix;
        var installed = Path.Combine(root, SystemFilesConstants.DirectoryName);
        var replaced = false;
        Directory.CreateDirectory(staging);
        try
        {
            using var response = await client.GetAsync(SystemFilesConstants.PackageUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            await using (var source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
            using (var package = new MemoryStream())
            {
                await source.CopyToAsync(package, cancellationToken).ConfigureAwait(false);
                package.Position = 0;
                using var archive = new ZipArchive(package, ZipArchiveMode.Read);
                archive.ExtractToDirectory(staging);
            }
            Validate(staging);
            cancellationToken.ThrowIfCancellationRequested();
            if (Directory.Exists(installed)) Directory.Move(installed, previous);
            try
            {
                Directory.Move(Path.Combine(staging, SystemFilesConstants.DirectoryName), installed);
                replaced = true;
            }
            catch
            {
                if (Directory.Exists(previous)) Directory.Move(previous, installed);
                throw;
            }
        }
        finally
        {
            try { if (Directory.Exists(staging)) Directory.Delete(staging, true); }
            finally { if (replaced && Directory.Exists(previous)) Directory.Delete(previous, true); }
        }
    }
}
