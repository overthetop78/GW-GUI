using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Services;

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
