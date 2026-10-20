using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Interop;
using System.IO;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Services;

internal sealed class ExternalCoreLibrary : IDisposable
{
    private nint _handle;

    internal static HashSet<string> ParseExtensions(nint value)
        => (Marshal.PtrToStringUTF8(value) ?? string.Empty)
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(extension => extension.TrimStart('.'))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    internal static TInfo ReadInfo<TExpected, TInfo>(ExternalCoreApi.SystemInfo nativeInfo, TExpected expected,
        Func<TExpected, string> expectedNameFor, Func<nint, IReadOnlySet<string>> parseExtensions,
        Func<string, string, Exception> identityMismatch,
        Func<TExpected, string, string, IReadOnlySet<string>, bool, bool, TInfo> create)
    {
        var libraryName = Marshal.PtrToStringUTF8(nativeInfo.LibraryName) ?? string.Empty;
        var expectedName = expectedNameFor(expected);
        if (!string.Equals(libraryName, expectedName, StringComparison.OrdinalIgnoreCase))
            throw identityMismatch(expectedName, libraryName);
        return create(expected, libraryName, Marshal.PtrToStringUTF8(nativeInfo.LibraryVersion) ?? string.Empty,
            parseExtensions(nativeInfo.ValidExtensions), nativeInfo.NeedFullPath, nativeInfo.BlockExtract);
    }

    internal static T Inspect<T>(string absolutePath, uint expectedApiVersion,
        Action<ExternalCoreLibrary> validateExports, Func<uint, Exception> unsupportedApi,
        Func<ExternalCoreApi.SystemInfo, T> readInfo,
        Func<EntryPointNotFoundException, Exception> missingExport,
        Func<BadImageFormatException, Exception> invalidLibrary)
    {
        try
        {
            using var library = new ExternalCoreLibrary(absolutePath);
            var getApiVersion = library.Resolve<ExternalCoreApi.GetApiVersion>(ExternalCoreExportNames.ApiVersion);
            var getSystemInfo = library.Resolve<ExternalCoreApi.GetSystemInfo>(ExternalCoreExportNames.GetSystemInfo);
            validateExports(library);
            var apiVersion = getApiVersion();
            if (apiVersion != expectedApiVersion) throw unsupportedApi(apiVersion);
            getSystemInfo(out var nativeInfo);
            return readInfo(nativeInfo);
        }
        catch (EntryPointNotFoundException error)
        {
            throw missingExport(error);
        }
        catch (BadImageFormatException error)
        {
            throw invalidLibrary(error);
        }
    }

    internal static void ValidatePath(string absolutePath, Func<Exception> requireAbsolute,
        Func<string, Exception> missingFile)
    {
        if (!Path.IsPathFullyQualified(absolutePath))
            throw requireAbsolute();
        if (!File.Exists(absolutePath))
            throw missingFile(absolutePath);
    }

    internal ExternalCoreLibrary(string absolutePath)
    {
        ValidatePath(absolutePath,
            static () => new ArgumentException(ExternalCoreErrorMessages.PathMustBeAbsolute, nameof(absolutePath)),
            static path => new FileNotFoundException(ExternalCoreErrorMessages.FileMissing, path));
        _handle = NativeLibrary.Load(absolutePath);
    }

    internal T Resolve<T>(string exportName) where T : Delegate
    {
        ObjectDisposedException.ThrowIf(_handle == 0, this);
        return Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(_handle, exportName));
    }

    public void Dispose()
    {
        if (_handle == 0) return;
        NativeLibrary.Free(_handle);
        _handle = 0;
    }

    internal ExternalCoreExports ResolveExports(bool requireLoadGameSpecial) => new(
        Resolve<ExternalCoreApi.SetEnvironment>(ExternalCoreExportNames.SetEnvironment),
        Resolve<ExternalCoreApi.SetVideo>(ExternalCoreExportNames.SetVideoRefresh),
        Resolve<ExternalCoreApi.SetAudioSample>(ExternalCoreExportNames.SetAudioSample),
        Resolve<ExternalCoreApi.SetAudioBatch>(ExternalCoreExportNames.SetAudioSampleBatch),
        Resolve<ExternalCoreApi.SetInputPoll>(ExternalCoreExportNames.SetInputPoll),
        Resolve<ExternalCoreApi.SetInputState>(ExternalCoreExportNames.SetInputState),
        Resolve<ExternalCoreApi.VoidCall>(ExternalCoreExportNames.Initialize),
        Resolve<ExternalCoreApi.VoidCall>(ExternalCoreExportNames.Deinitialize),
        Resolve<ExternalCoreApi.GetSystemInfo>(ExternalCoreExportNames.GetSystemInfo),
        Resolve<ExternalCoreApi.GetSystemAvInfo>(ExternalCoreExportNames.GetSystemAvInfo),
        Resolve<ExternalCoreApi.SetControllerPortDevice>(ExternalCoreExportNames.SetControllerPortDevice),
        Resolve<ExternalCoreApi.VoidCall>(ExternalCoreExportNames.Reset),
        Resolve<ExternalCoreApi.VoidCall>(ExternalCoreExportNames.Run),
        Resolve<ExternalCoreApi.LoadGame>(ExternalCoreExportNames.LoadGame),
        Resolve<ExternalCoreApi.VoidCall>(ExternalCoreExportNames.UnloadGame),
        Resolve<ExternalCoreApi.GetRegion>(ExternalCoreExportNames.GetRegion),
        Resolve<ExternalCoreApi.GetMemoryData>(ExternalCoreExportNames.GetMemoryData),
        Resolve<ExternalCoreApi.GetMemorySize>(ExternalCoreExportNames.GetMemorySize),
        Resolve<ExternalCoreApi.GetSerializedSize>(ExternalCoreExportNames.GetSerializedSize),
        Resolve<ExternalCoreApi.Serialize>(ExternalCoreExportNames.Serialize),
        Resolve<ExternalCoreApi.Serialize>(ExternalCoreExportNames.Unserialize),
        requireLoadGameSpecial ? Resolve<ExternalCoreApi.LoadGameSpecial>(ExternalCoreExportNames.LoadGameSpecial) : null);
}
