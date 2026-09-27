using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Atari.Common.Services;

internal sealed class ContentPath : IDisposable
{
    internal ContentPath(string value, bool useWindowsAnsi)
    {
        Pointer = useWindowsAnsi && OperatingSystem.IsWindows()
            ? Marshal.StringToCoTaskMemAnsi(value)
            : Marshal.StringToCoTaskMemUTF8(value);
    }

    internal nint Pointer { get; private set; }

    public void Dispose()
    {
        if (Pointer == nint.Zero) return;
        Marshal.FreeCoTaskMem(Pointer);
        Pointer = nint.Zero;
    }
}
