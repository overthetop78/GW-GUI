using System.Globalization;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Atari.Common.Functions;

internal static class CoreFunctions
{
    internal static ExternalCoreInfo ReadInitializedInfo(ExternalCoreExports exports,
        Emulator expectedEmulator)
    {
        exports.GetSystemInfo(out var nativeInfo);
        var libraryName = Marshal.PtrToStringUTF8(nativeInfo.LibraryName) ?? string.Empty;
        var expectedName = ExpectedLibraryName(expectedEmulator);
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
            ParseExtensions(nativeInfo.ValidExtensions), nativeInfo.NeedFullPath, nativeInfo.BlockExtract);
    }
    internal static string CreateInvalidOptionValueMessage(string key, string value) =>
        string.Format(CultureInfo.InvariantCulture,
            ErrorMessages.OptionValueInvalidFormat, key, value);
    internal static string ExpectedLibraryName(Emulator emulator) => emulator switch
    {
        Emulator.Hatari => CoreIdentityConstants.Hatari,
        Emulator.Atari800 => CoreIdentityConstants.Atari800,
        Emulator.Stella => CoreIdentityConstants.Stella,
        Emulator.ProSystem => CoreIdentityConstants.ProSystem,
        Emulator.BeetleLynx => CoreIdentityConstants.BeetleLynx,
        Emulator.VirtualJaguar => CoreIdentityConstants.VirtualJaguar,
        _ => throw new ArgumentOutOfRangeException(nameof(emulator), emulator, null)
    };

    internal static IReadOnlySet<string> ParseExtensions(nint value)
    {
        var extensions = Marshal.PtrToStringUTF8(value);
        return string.IsNullOrWhiteSpace(extensions)
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : extensions.Split(CommonConstants.SupportedExtensionSeparator,
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(extension => extension.TrimStart(CommonConstants.ExtensionPrefix))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    internal static ExternalCoreExports ResolveExports(ExternalCoreLibrary library) => new(
        library.Resolve<ExternalCoreApi.SetEnvironment>(ExternalCoreExportNames.SetEnvironment),
        library.Resolve<ExternalCoreApi.SetVideo>(ExternalCoreExportNames.SetVideoRefresh),
        library.Resolve<ExternalCoreApi.SetAudioSample>(ExternalCoreExportNames.SetAudioSample),
        library.Resolve<ExternalCoreApi.SetAudioBatch>(ExternalCoreExportNames.SetAudioSampleBatch),
        library.Resolve<ExternalCoreApi.SetInputPoll>(ExternalCoreExportNames.SetInputPoll),
        library.Resolve<ExternalCoreApi.SetInputState>(ExternalCoreExportNames.SetInputState),
        library.Resolve<ExternalCoreApi.VoidCall>(ExternalCoreExportNames.Initialize),
        library.Resolve<ExternalCoreApi.VoidCall>(ExternalCoreExportNames.Deinitialize),
        library.Resolve<ExternalCoreApi.GetSystemInfo>(ExternalCoreExportNames.GetSystemInfo),
        library.Resolve<ExternalCoreApi.GetSystemAvInfo>(ExternalCoreExportNames.GetSystemAvInfo),
        library.Resolve<ExternalCoreApi.SetControllerPortDevice>(ExternalCoreExportNames.SetControllerPortDevice),
        library.Resolve<ExternalCoreApi.VoidCall>(ExternalCoreExportNames.Reset),
        library.Resolve<ExternalCoreApi.VoidCall>(ExternalCoreExportNames.Run),
        library.Resolve<ExternalCoreApi.LoadGame>(ExternalCoreExportNames.LoadGame),
        library.Resolve<ExternalCoreApi.VoidCall>(ExternalCoreExportNames.UnloadGame),
        library.Resolve<ExternalCoreApi.GetRegion>(ExternalCoreExportNames.GetRegion),
        library.Resolve<ExternalCoreApi.GetMemoryData>(ExternalCoreExportNames.GetMemoryData),
        library.Resolve<ExternalCoreApi.GetMemorySize>(ExternalCoreExportNames.GetMemorySize),
        library.Resolve<ExternalCoreApi.GetSerializedSize>(ExternalCoreExportNames.GetSerializedSize),
        library.Resolve<ExternalCoreApi.Serialize>(ExternalCoreExportNames.Serialize),
        library.Resolve<ExternalCoreApi.Serialize>(ExternalCoreExportNames.Unserialize));

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
        Marshal.WriteByte(destination,
            value ? CommonConstants.NativeBooleanTrue : CommonConstants.NativeBooleanFalse);
        return true;
    }

    internal static bool WriteInteger(nint destination, int value)
    {
        if (destination == nint.Zero) return false;
        Marshal.WriteInt32(destination, value);
        return true;
    }

    internal static bool WriteInteger(nint destination, uint value) =>
        WriteInteger(destination, checked((int)value));

    internal static bool WriteUnsignedLong(nint destination, ulong value)
    {
        if (destination == nint.Zero) return false;
        Marshal.WriteInt64(destination, unchecked((long)value));
        return true;
    }
}
