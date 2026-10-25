using CommonCoreHostFunctions = GWGUI.Emulation.Coleco.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Contracts;
using System.IO;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions;


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
