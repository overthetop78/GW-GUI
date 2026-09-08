using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Atari.Services;

internal sealed class AtariContentPath : IDisposable
{
    internal AtariContentPath(string value, bool useWindowsAnsi)
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
