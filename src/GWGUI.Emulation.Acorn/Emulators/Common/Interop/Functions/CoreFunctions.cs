using GWGUI.Emulation.Acorn.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Acorn.Emulators.Common.Interop.Dictionaries;
using GWGUI.Emulation.Acorn.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Acorn.Emulators.Common.Interop.Constants;
using System.Globalization;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Acorn.Emulators.Common.Interop.Functions;

internal static class CoreFunctions
{
    internal static ExternalCoreInfo ReadInitializedInfo(ExternalCoreExports exports, Emulator expectedEmulator)
    {
        exports.GetSystemInfo(out var nativeInfo);
        return ReadInfo(nativeInfo, expectedEmulator);
    }

    internal static ExternalCoreInfo ReadInfo(GWGUI.Emulation.Interop.ExternalCoreApi.SystemInfo nativeInfo, Emulator expectedEmulator)
        => ExternalCoreLibrary.ReadInfo(nativeInfo, expectedEmulator, ExpectedLibraryName, ExternalCoreLibrary.ParseExtensions,
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

    internal static ExternalCoreExports ResolveExports(ExternalCoreLibrary library) => library.ResolveExports(requireLoadGameSpecial: false);

    internal static void InstallCallbacks(ExternalCoreExports exports, ExternalHostCallbacks callbacks)
    => ExternalCoreEnvironmentFunctions.InstallCallbacks(exports, callbacks);

    internal static bool WritePointer(nint destination, nint value)
        => ExternalCoreEnvironmentFunctions.WritePointer(destination, value);

    internal static bool WriteBoolean(nint destination, bool value)
        => ExternalCoreEnvironmentFunctions.WriteBoolean(destination, value);

    internal static bool WriteInteger(nint destination, int value)
        => ExternalCoreEnvironmentFunctions.WriteInteger(destination, value);

    internal static bool WriteInteger(nint destination, uint value) => ExternalCoreEnvironmentFunctions.WriteInteger(destination, value);

    internal static bool WriteUnsignedLong(nint destination, ulong value)
        => ExternalCoreEnvironmentFunctions.WriteUnsignedLong(destination, value);
}

