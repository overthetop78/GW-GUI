using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;

internal sealed class ContentPath : IDisposable
{
    internal ContentPath(string value, bool useWindowsAnsi)
    {
        Value = value;
        Pointer = useWindowsAnsi && OperatingSystem.IsWindows() ? Marshal.StringToCoTaskMemAnsi(value) : Marshal.StringToCoTaskMemUTF8(value);
    }

    internal nint Pointer { get; private set; }
    internal string Value { get; }

    public void Dispose()
    {
        if (Pointer == nint.Zero) return;
        Marshal.FreeCoTaskMem(Pointer);
        Pointer = nint.Zero;
    }
}
