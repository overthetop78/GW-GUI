using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace GWGUI.App.Functions.Services.Windows;

internal static class MonitorWorkAreaFunctions
{
    private const uint MonitorDefaultToNearest = 2;

    public static Rect Get(Window window)
    {
        var handle = new WindowInteropHelper(window).Handle;
        var monitor = MonitorFromWindow(handle, MonitorDefaultToNearest);
        var info = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };
        if (monitor == IntPtr.Zero || !GetMonitorInfo(monitor, ref info)) return SystemParameters.WorkArea;

        var topLeft = window.PointFromScreen(new Point(info.Work.Left, info.Work.Top));
        var bottomRight = window.PointFromScreen(new Point(info.Work.Right, info.Work.Bottom));
        return new Rect(window.Left + topLeft.X, window.Top + topLeft.Y,
            bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect { public int Left; public int Top; public int Right; public int Bottom; }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MonitorInfo
    {
        public int Size;
        public NativeRect Monitor;
        public NativeRect Work;
        public uint Flags;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr window, uint flags);

    [DllImport("user32.dll", EntryPoint = "GetMonitorInfoW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo info);
}
