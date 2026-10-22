using CommonCoreHostFunctions = GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Acorn.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Acorn.Emulators.Common.Interop.Constants;
using System.IO;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Acorn.Emulators.Common.Interop.Functions;


internal static class ExternalCoreProbe
{
    internal static ExternalCoreInfo Inspect(string absolutePath, Emulator expectedEmulator)
    {
        CommonCoreHostFunctions.NativeResources.ValidateLibraryPath(absolutePath,
            static () => new EmulationException(ErrorCategory.Core, ErrorCode.CoreRejected, ErrorMessages.CorePathMustBeAbsolute),
            static missingPath => new EmulationException(ErrorCategory.Core, ErrorCode.CoreNotFound, ErrorMessages.CoreFileMissing, new Dictionary<string, string> { [ErrorContextConstants.Path] = missingPath }));

        return CommonCoreHostFunctions.NativeResources.InspectLibrary(absolutePath, ExternalCoreInteropConstants.ApiVersion, static library =>
            {
                _ = CoreFunctions.ResolveExports(library);
            }, static apiVersion => new EmulationException(ErrorCategory.Core, ErrorCode.CoreRejected, ErrorMessages.CoreApiVersionUnsupported, new Dictionary<string, string> { [ErrorContextConstants.Version] = apiVersion.ToString(System.Globalization.CultureInfo.InvariantCulture) }), nativeInfo => CoreFunctions.ReadInfo(nativeInfo, expectedEmulator),
            static error => new EmulationException(ErrorCategory.Core, ErrorCode.CoreRejected, ErrorMessages.CoreExportMissing, innerException: error),
            static error => new EmulationException(ErrorCategory.Core, ErrorCode.CoreRejected, ErrorMessages.CoreIdentityMismatch, innerException: error));
    }

}

