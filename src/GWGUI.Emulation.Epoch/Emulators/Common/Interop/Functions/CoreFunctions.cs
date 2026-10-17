using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Dictionaries;
using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Services;
using System.Globalization;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Epoch.Emulators.Common.Interop.Functions;

internal static class CoreFunctions
{
    internal static ExternalCoreInfo ReadInitializedInfo(ExternalCoreExports exports, Emulator expectedEmulator)
    {
        exports.GetSystemInfo(out var nativeInfo);
        var libraryName = Marshal.PtrToStringUTF8(nativeInfo.LibraryName) ?? string.Empty;
        var expectedName = ExpectedLibraryName(expectedEmulator);
        if (!string.Equals(libraryName, expectedName, StringComparison.OrdinalIgnoreCase))
            throw new EmulationException(ErrorCategory.Core, ErrorCode.CoreRejected,
                ErrorMessages.CoreIdentityMismatch,
                new Dictionary<string, string>
                {
                    [ErrorContextConstants.Expected] = expectedName,
                    [ErrorContextConstants.Actual] = libraryName
                });
        return new ExternalCoreInfo(expectedEmulator, libraryName, Marshal.PtrToStringUTF8(nativeInfo.LibraryVersion) ?? string.Empty, ParseExtensions(nativeInfo.ValidExtensions), nativeInfo.NeedFullPath, nativeInfo.BlockExtract);
    }
    internal static string CreateInvalidOptionValueMessage(string key, string value) => string.Format(CultureInfo.InvariantCulture, ErrorMessages.OptionValueInvalidFormat, key, value);
    internal static string ExpectedLibraryName(Emulator emulator) => CoreCatalog.Get(emulator).LibraryName;

    internal static IReadOnlySet<string> ParseExtensions(nint value)
    {
        var extensions = Marshal.PtrToStringUTF8(value);
        return string.IsNullOrWhiteSpace(extensions)
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : extensions.Split(MediaConstants.SupportedExtensionSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(extension => extension.TrimStart(MediaConstants.ExtensionPrefix))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    internal static ExternalCoreExports ResolveExports(ExternalCoreLibrary library) => library.ResolveExports(requireLoadGameSpecial: true);

    internal static void InstallCallbacks(ExternalCoreExports exports, ExternalHostCallbacks callbacks)
    {
        exports.SetEnvironment(callbacks.Environment);
        exports.SetVideo(callbacks.Video);
        exports.SetAudioSample(callbacks.AudioSample);
        exports.SetAudioBatch(callbacks.AudioBatch);
        exports.SetInputPoll(callbacks.InputPoll);
        exports.SetInputState(callbacks.InputState);
    }

    internal static bool WritePointer(nint destination, nint value)
    {
        if (destination == nint.Zero) return false;
        Marshal.WriteIntPtr(destination, value);
        return true;
    }

    internal static bool WriteBoolean(nint destination, bool value)
    {
        if (destination == nint.Zero) return false;
        Marshal.WriteByte(destination, value ? ExternalCoreInteropConstants.NativeBooleanTrue : ExternalCoreInteropConstants.NativeBooleanFalse);
        return true;
    }

    internal static bool WriteInteger(nint destination, int value)
    {
        if (destination == nint.Zero) return false;
        Marshal.WriteInt32(destination, value);
        return true;
    }

    internal static bool WriteInteger(nint destination, uint value) => WriteInteger(destination, checked((int)value));

    internal static bool WriteUnsignedLong(nint destination, ulong value)
    {
        if (destination == nint.Zero) return false;
        Marshal.WriteInt64(destination, unchecked((long)value));
        return true;
    }
}
