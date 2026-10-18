using GWGUI.Emulation.Mattel.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Mattel.Emulators.Common.Interop.Contracts;
using System.IO;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Mattel.Emulators.Common.Interop.Functions;


internal static class ExternalCoreProbe
{
    internal static ExternalCoreInfo Inspect(string absolutePath, Emulator expectedEmulator)
    {
        ExternalCoreLibrary.ValidatePath(absolutePath,
            static () => new EmulationException(ErrorCategory.Core, ErrorCode.CoreRejected, ErrorMessages.CorePathMustBeAbsolute),
            static missingPath => new EmulationException(ErrorCategory.Core, ErrorCode.CoreNotFound, ErrorMessages.CoreFileMissing, new Dictionary<string, string> { [ErrorContextConstants.Path] = missingPath }));

        try
        {
            return ExternalCoreLibrary.Inspect(absolutePath, ExternalCoreInteropConstants.ApiVersion, static library =>
            {
                _ = CoreFunctions.ResolveExports(library);
            }, static apiVersion => new EmulationException(ErrorCategory.Core, ErrorCode.CoreRejected, ErrorMessages.CoreApiVersionUnsupported, new Dictionary<string, string> { [ErrorContextConstants.Version] = apiVersion.ToString(System.Globalization.CultureInfo.InvariantCulture) }), nativeInfo => CoreFunctions.ReadInfo(nativeInfo, expectedEmulator));
        }
        catch (EmulationException)
        {
            throw;
        }
        catch (EntryPointNotFoundException error)
        {
            throw new EmulationException(ErrorCategory.Core, ErrorCode.CoreRejected, ErrorMessages.CoreExportMissing, innerException: error);
        }
        catch (BadImageFormatException error)
        {
            throw new EmulationException(ErrorCategory.Core, ErrorCode.CoreRejected, ErrorMessages.CoreIdentityMismatch, innerException: error);
        }
    }

}
