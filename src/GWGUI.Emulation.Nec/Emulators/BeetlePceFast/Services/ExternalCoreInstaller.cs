using GWGUI.Emulation.Nec.Emulators.Common.Interop.Exceptions;
using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Factories;
using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Functions;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Services;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Services;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;


namespace GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Services;

public sealed class ExternalCoreInstaller
{
    public const string CoreRevision = CoreReleaseConstants.Latest;
    public const string DownloadUrl = ReleaseConstants.OfficialArchiveUrl;
    private readonly HttpClient _httpClient;
    private readonly string _directory;

    public ExternalCoreInstaller(HttpClient httpClient, string directory)
    {
        _httpClient = httpClient;
        _directory = Path.GetFullPath(directory);
    }

    public string LibraryPath => Path.Combine(_directory, ReleaseConstants.InstalledLibraryName);

    public bool IsInstalled
    {
        get
        {
            if (!File.Exists(LibraryPath)) return false;
            try { CoreReleaseService.VerifyWindowsX64Library(LibraryPath); return true; }
            catch (IOException) { return false; }
            catch (InvalidDataException) { return false; }
        }
    }

    public async Task<string> InstallAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_directory);
        var package = LibraryPath + CoreReleaseConstants.Download;
        var extracted = LibraryPath + CoreReleaseConstants.Extract;
        try
        {
            using var response = await _httpClient.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            await using (var source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
            await using (var destination = new FileStream(package, FileMode.Create, FileAccess.Write, FileShare.None,
                             81920, FileOptions.Asynchronous))
                await source.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);

            using (var archive = ZipFile.OpenRead(package))
            {
                var entry = archive.Entries.FirstOrDefault(item =>
                    Path.GetFileName(item.FullName).Equals(ReleaseConstants.ArchiveLibraryName, StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidDataException(CoreExceptions.ArchiveMissingLibrary());
                entry.ExtractToFile(extracted, true);
            }
            CoreReleaseService.VerifyWindowsX64Library(extracted);
            var sha256 = Hash(extracted);
            File.Move(extracted, LibraryPath, true);
            await CoreManifestFunctions.WriteManifestAsync(CoreRevision, DownloadUrl, LibraryPath, sha256,
                cancellationToken).ConfigureAwait(false);
            return LibraryPath;
        }
        finally
        {
            if (File.Exists(package)) File.Delete(package);
            if (File.Exists(extracted)) File.Delete(extracted);
        }
    }

    internal static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }
}

