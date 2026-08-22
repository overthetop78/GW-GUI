using GWGUI.App.Constants.Rendering.Emulation;
using System.Runtime.InteropServices;

namespace GWGUI.App.Functions.Rendering.Emulation;

internal static class NativeVideoWindowFunctions
{
    internal static IntPtr Create(IntPtr parent, int width, int height) => CreateWindowEx(
        0,
        "STATIC",
        string.Empty,
        NativeChildWindowStyleConstants.Value,
        0,
        0,
        Math.Max(1, width),
        Math.Max(1, height),
        parent,
        IntPtr.Zero,
        ModuleHandle,
        IntPtr.Zero);

    internal static void Destroy(IntPtr window) => DestroyWindow(window);

    internal static NativeVideoWindowSize GetClientSize(IntPtr window)
    {
        if (!GetClientRect(window, out var rectangle))
            return new NativeVideoWindowSize(1, 1);

        return new NativeVideoWindowSize(
            Math.Max(1, rectangle.Right - rectangle.Left),
            Math.Max(1, rectangle.Bottom - rectangle.Top));
    }

    internal static IntPtr ModuleHandle => GetModuleHandle(null);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateWindowEx(
        int extendedStyle,
        string className,
        string windowName,
        int style,
        int x,
        int y,
        int width,
        int height,
        IntPtr parent,
        IntPtr menu,
        IntPtr instance,
        IntPtr parameter);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr window);

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(IntPtr window, out NativeVideoWindowRectangle rectangle);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetModuleHandle(string? name);

    [StructLayout(LayoutKind.Sequential)]
    private readonly record struct NativeVideoWindowRectangle(int Left, int Top, int Right, int Bottom);
}

internal readonly record struct NativeVideoWindowSize(int Width, int Height);
