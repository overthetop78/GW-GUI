using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Atari.Common.Functions;


internal static class ExternalCoreProbe
{
    internal static ExternalCoreInfo Inspect(string absolutePath, Emulator expectedEmulator)
    {
        if (!Path.IsPathFullyQualified(absolutePath))
            throw new EmulationException(ErrorCategory.Core, ErrorCode.CoreRejected,
                ErrorMessages.CorePathMustBeAbsolute);
        if (!File.Exists(absolutePath))
            throw new EmulationException(ErrorCategory.Core, ErrorCode.CoreNotFound,
                ErrorMessages.CoreFileMissing,
                new Dictionary<string, string> { [CommonConstants.PathContextKey] = absolutePath });

        try
        {
            using var library = new ExternalCoreLibrary(absolutePath);
            var getApiVersion = library.Resolve<ExternalCoreApi.GetApiVersion>(ExternalCoreExportNames.ApiVersion);
            var getSystemInfo = library.Resolve<ExternalCoreApi.GetSystemInfo>(ExternalCoreExportNames.GetSystemInfo);
            _ = CoreFunctions.ResolveExports(library);

            var apiVersion = getApiVersion();
            if (apiVersion != CommonConstants.ExternalCoreApiVersion)
                throw new EmulationException(ErrorCategory.Core, ErrorCode.CoreRejected,
                    ErrorMessages.CoreApiVersionUnsupported,
                    new Dictionary<string, string>
                    {
                        [CommonConstants.VersionContextKey] = apiVersion.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    });

            getSystemInfo(out var nativeInfo);
            var libraryName = Marshal.PtrToStringUTF8(nativeInfo.LibraryName) ?? string.Empty;
            var expectedName = CoreFunctions.ExpectedLibraryName(expectedEmulator);
            if (!string.Equals(libraryName, expectedName, StringComparison.OrdinalIgnoreCase))
                throw new EmulationException(ErrorCategory.Core, ErrorCode.CoreRejected,
                    ErrorMessages.CoreIdentityMismatch,
                    new Dictionary<string, string>
                    {
                        [CommonConstants.ExpectedContextKey] = expectedName,
                        [CommonConstants.ActualContextKey] = libraryName
                    });

            return new ExternalCoreInfo(expectedEmulator, libraryName,
                Marshal.PtrToStringUTF8(nativeInfo.LibraryVersion) ?? string.Empty,
                CoreFunctions.ParseExtensions(nativeInfo.ValidExtensions), nativeInfo.NeedFullPath, nativeInfo.BlockExtract);
        }
        catch (EmulationException)
        {
            throw;
        }
        catch (EntryPointNotFoundException error)
        {
            throw new EmulationException(ErrorCategory.Core, ErrorCode.CoreRejected,
                ErrorMessages.CoreExportMissing, innerException: error);
        }
        catch (BadImageFormatException error)
        {
            throw new EmulationException(ErrorCategory.Core, ErrorCode.CoreRejected,
                ErrorMessages.CoreIdentityMismatch, innerException: error);
        }
    }

}
