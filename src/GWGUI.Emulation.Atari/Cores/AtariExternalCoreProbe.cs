using System.Runtime.InteropServices;
using GWGUI.Emulation.Common;

namespace GWGUI.Emulation.Atari.Cores;

internal sealed record AtariExternalCoreInfo(
    AtariEmulator Emulator,
    string LibraryName,
    string LibraryVersion,
    IReadOnlySet<string> Extensions,
    bool NeedsFullPath,
    bool BlocksArchiveExtraction);

internal static class AtariExternalCoreProbe
{
    internal static AtariExternalCoreInfo Inspect(string absolutePath, AtariEmulator expectedEmulator)
    {
        if (!Path.IsPathFullyQualified(absolutePath))
            throw new AtariEmulationException(AtariErrorCategory.Core, AtariErrorCode.CoreRejected,
                AtariErrorMessages.CorePathMustBeAbsolute);
        if (!File.Exists(absolutePath))
            throw new AtariEmulationException(AtariErrorCategory.Core, AtariErrorCode.CoreNotFound,
                AtariErrorMessages.CoreFileMissing,
                new Dictionary<string, string> { [AtariConstants.PathContextKey] = absolutePath });

        try
        {
            using var library = new ExternalCoreLibrary(absolutePath);
            var getApiVersion = library.Resolve<ExternalCoreApi.GetApiVersion>(ExternalCoreExportNames.ApiVersion);
            var getSystemInfo = library.Resolve<ExternalCoreApi.GetSystemInfo>(ExternalCoreExportNames.GetSystemInfo);
            _ = AtariCoreFunctions.ResolveExports(library);

            var apiVersion = getApiVersion();
            if (apiVersion != AtariConstants.ExternalCoreApiVersion)
                throw new AtariEmulationException(AtariErrorCategory.Core, AtariErrorCode.CoreRejected,
                    AtariErrorMessages.CoreApiVersionUnsupported,
                    new Dictionary<string, string>
                    {
                        [AtariConstants.VersionContextKey] = apiVersion.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    });

            getSystemInfo(out var nativeInfo);
            var libraryName = Marshal.PtrToStringUTF8(nativeInfo.LibraryName) ?? string.Empty;
            var expectedName = AtariCoreFunctions.ExpectedLibraryName(expectedEmulator);
            if (!string.Equals(libraryName, expectedName, StringComparison.OrdinalIgnoreCase))
                throw new AtariEmulationException(AtariErrorCategory.Core, AtariErrorCode.CoreRejected,
                    AtariErrorMessages.CoreIdentityMismatch,
                    new Dictionary<string, string>
                    {
                        [AtariConstants.ExpectedContextKey] = expectedName,
                        [AtariConstants.ActualContextKey] = libraryName
                    });

            return new AtariExternalCoreInfo(expectedEmulator, libraryName,
                Marshal.PtrToStringUTF8(nativeInfo.LibraryVersion) ?? string.Empty,
                AtariCoreFunctions.ParseExtensions(nativeInfo.ValidExtensions), nativeInfo.NeedFullPath, nativeInfo.BlockExtract);
        }
        catch (AtariEmulationException)
        {
            throw;
        }
        catch (EntryPointNotFoundException error)
        {
            throw new AtariEmulationException(AtariErrorCategory.Core, AtariErrorCode.CoreRejected,
                AtariErrorMessages.CoreExportMissing, innerException: error);
        }
        catch (BadImageFormatException error)
        {
            throw new AtariEmulationException(AtariErrorCategory.Core, AtariErrorCode.CoreRejected,
                AtariErrorMessages.CoreIdentityMismatch, innerException: error);
        }
    }

}
