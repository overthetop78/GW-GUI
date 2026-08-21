using System.Globalization;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Common;

namespace GWGUI.Emulation.Atari.Cores;

internal static class AtariCoreFunctions
{
    internal static AtariExternalCoreInfo ReadInitializedInfo(AtariExternalCoreExports exports,
        AtariEmulator expectedEmulator)
    {
        exports.GetSystemInfo(out var nativeInfo);
        var libraryName = Marshal.PtrToStringUTF8(nativeInfo.LibraryName) ?? string.Empty;
        var expectedName = ExpectedLibraryName(expectedEmulator);
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
            ParseExtensions(nativeInfo.ValidExtensions), nativeInfo.NeedFullPath, nativeInfo.BlockExtract);
    }
    internal static string CreateInvalidOptionValueMessage(string key, string value) =>
        string.Format(CultureInfo.InvariantCulture,
            AtariErrorMessages.OptionValueInvalidFormat, key, value);
    internal static string ExpectedLibraryName(AtariEmulator emulator) => emulator switch
    {
        AtariEmulator.Hatari => AtariCoreIdentityConstants.Hatari,
        AtariEmulator.Atari800 => AtariCoreIdentityConstants.Atari800,
        AtariEmulator.Stella => AtariCoreIdentityConstants.Stella,
        AtariEmulator.ProSystem => AtariCoreIdentityConstants.ProSystem,
        AtariEmulator.BeetleLynx => AtariCoreIdentityConstants.BeetleLynx,
        AtariEmulator.VirtualJaguar => AtariCoreIdentityConstants.VirtualJaguar,
        _ => throw new ArgumentOutOfRangeException(nameof(emulator), emulator, null)
    };

    internal static IReadOnlySet<string> ParseExtensions(nint value)
    {
        var extensions = Marshal.PtrToStringUTF8(value);
        return string.IsNullOrWhiteSpace(extensions)
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : extensions.Split(AtariConstants.SupportedExtensionSeparator,
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(extension => extension.TrimStart(AtariConstants.ExtensionPrefix))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    internal static AtariExternalCoreExports ResolveExports(ExternalCoreLibrary library) => new(
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

    internal static void InstallCallbacks(AtariExternalCoreExports exports, AtariExternalHostCallbacks callbacks)
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
            value ? AtariConstants.NativeBooleanTrue : AtariConstants.NativeBooleanFalse);
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
