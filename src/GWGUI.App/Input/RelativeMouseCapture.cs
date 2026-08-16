using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace GWGUI.App.Input;

internal sealed class RelativeMouseCapture
{
    internal bool IsCaptured { get; private set; }

    internal void Capture(FrameworkElement display, FrameworkElement screen, IntPtr nativeHandle)
    {
        IsCaptured = true;
        display.Cursor = Cursors.None;
        Mouse.Capture(display);
        if (nativeHandle != IntPtr.Zero)
        {
            SetCapture(nativeHandle);
            SetFocus(nativeHandle);
            HideNativeCursor();
        }
        else display.Focus();
        CenterCursor(screen);
    }

    internal void Release(FrameworkElement display, IntPtr nativeHandle)
    {
        if (!IsCaptured) return;
        IsCaptured = false;
        Mouse.Capture(null);
        if (nativeHandle != IntPtr.Zero) ReleaseCapture();
        display.Cursor = null;
    }

    internal void ProcessMovement(FrameworkElement screen, Action<int, int> moved)
    {
        if (!IsCaptured || !GetCursorPos(out var current)) return;
        var center = screen.PointToScreen(new Point(screen.ActualWidth / 2, screen.ActualHeight / 2));
        var deltaX = current.X - (int)Math.Round(center.X);
        var deltaY = current.Y - (int)Math.Round(center.Y);
        if (deltaX == 0 && deltaY == 0) return;
        moved(deltaX, deltaY);
        SetCursorPos((int)Math.Round(center.X), (int)Math.Round(center.Y));
    }

    internal static void FocusNative(IntPtr handle) => SetFocus(handle);
    internal static void HideNativeCursor() => SetCursor(IntPtr.Zero);
    internal static bool IsButtonPressed(int virtualKey) => (GetAsyncKeyState(virtualKey) & 0x8000) != 0;

    private static void CenterCursor(FrameworkElement screen)
    {
        var center = new Point(screen.ActualWidth / 2, screen.ActualHeight / 2);
        var position = screen.PointToScreen(center);
        SetCursorPos((int)Math.Round(position.X), (int)Math.Round(position.Y));
    }

    [DllImport("user32.dll")] private static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] private static extern bool GetCursorPos(out NativePoint point);
    [DllImport("user32.dll")] private static extern IntPtr SetCapture(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern IntPtr SetFocus(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int virtualKey);
    [DllImport("user32.dll")] private static extern bool ReleaseCapture();
    [DllImport("user32.dll")] private static extern IntPtr SetCursor(IntPtr cursor);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint { public int X; public int Y; }
}
