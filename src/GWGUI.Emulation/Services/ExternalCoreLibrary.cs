using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Interop;
using System.IO;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Services;

internal sealed class ExternalCoreLibrary : IDisposable
{
    private nint _handle;

    internal static T Inspect<T>(string absolutePath, uint expectedApiVersion,
        Action<ExternalCoreLibrary> validateExports, Func<uint, Exception> unsupportedApi,
        Func<ExternalCoreApi.SystemInfo, T> readInfo)
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

    internal ExternalCoreLibrary(string absolutePath)
    {
        if (!Path.IsPathFullyQualified(absolutePath))
            throw new ArgumentException(ExternalCoreErrorMessages.PathMustBeAbsolute, nameof(absolutePath));
        if (!File.Exists(absolutePath))
            throw new FileNotFoundException(ExternalCoreErrorMessages.FileMissing, absolutePath);
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
