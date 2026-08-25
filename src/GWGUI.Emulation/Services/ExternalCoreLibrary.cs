using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Services;

internal sealed class ExternalCoreLibrary : IDisposable
{
    private nint _handle;

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
}
