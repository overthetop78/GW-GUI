using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Common;

internal sealed class ExternalCoreLibrary : IDisposable
{
    private nint _handle;

    internal ExternalCoreLibrary(string absolutePath)
    {
        if (!Path.IsPathFullyQualified(absolutePath))
            throw new ArgumentException("The external core path must be absolute.", nameof(absolutePath));
        if (!File.Exists(absolutePath))
            throw new FileNotFoundException("The configured external core was not found.", absolutePath);
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

internal sealed class ExternalCoreUtf8String : IDisposable
{
    internal nint Pointer { get; private set; }

    internal ExternalCoreUtf8String(string value) => Pointer = Marshal.StringToCoTaskMemUTF8(value);

    public void Dispose()
    {
        if (Pointer == 0) return;
        Marshal.FreeCoTaskMem(Pointer);
        Pointer = 0;
    }
}
