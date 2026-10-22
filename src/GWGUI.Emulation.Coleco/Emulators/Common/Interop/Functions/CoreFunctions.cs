using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Coleco.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Dictionaries;
using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Services;
using System.Globalization;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions;

internal static class CoreFunctions
{

    internal static ExternalCoreInfo ReadInfo(GWGUI.Emulation.Interop.ExternalCoreApi.SystemInfo nativeInfo, Emulator expectedEmulator)
        => CommonCoreHostFunctions.NativeResources.ReadInfo(nativeInfo, expectedEmulator, ExpectedLibraryName, CommonCoreHostFunctions.NativeResources.ParseExtensions,
            static (expectedName, libraryName) => new EmulationException(ErrorCategory.Core, ErrorCode.CoreRejected,
                ErrorMessages.CoreIdentityMismatch,
                new Dictionary<string, string>
                {
                    [ErrorContextConstants.Expected] = expectedName,
                    [ErrorContextConstants.Actual] = libraryName
                }),
            static (emulator, libraryName, libraryVersion, extensions, needsFullPath, blocksArchiveExtraction)
                => new ExternalCoreInfo(emulator, libraryName, libraryVersion, extensions, needsFullPath, blocksArchiveExtraction));
    internal static string CreateInvalidOptionValueMessage(string key, string value) => string.Format(CultureInfo.InvariantCulture, ErrorMessages.OptionValueInvalidFormat, key, value);
    internal static string ExpectedLibraryName(Emulator emulator) => CoreCatalog.Get(emulator).LibraryName;

    internal static ExternalCoreExports ResolveExports(IEmulationCoreLibrary library) => library.ResolveExports(requireLoadGameSpecial: true);

    internal static void InstallCallbacks(ExternalCoreExports exports, ExternalHostCallbacks callbacks)
    => CommonCoreHostFunctions.EnvironmentProcessing.InstallCallbacks(exports, callbacks);

    internal static bool WritePointer(nint destination, nint value)
        => CommonCoreHostFunctions.EnvironmentProcessing.WritePointer(destination, value);

    internal static bool WriteInteger(nint destination, int value)
        => CommonCoreHostFunctions.EnvironmentProcessing.WriteInteger(destination, value);
}
