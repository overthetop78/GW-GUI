using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace GWGUI.App.Input;

internal sealed class RelativeMouseCapture
{
    private readonly RelativeMouseCaptureState _state = new();
    internal bool IsCaptured => _state.IsCaptured;

    internal void Capture(FrameworkElement display, FrameworkElement screen, IntPtr nativeHandle)
    {
        _state.Capture();
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
        if (!_state.Release()) return;
        Mouse.Capture(null);
        if (nativeHandle != IntPtr.Zero) ReleaseCapture();
        display.Cursor = null;
    }

    internal void ProcessMovement(FrameworkElement screen, Action<int, int> moved)
    {
        if (!IsCaptured || !GetCursorPos(out var current)) return;
        var center = screen.PointToScreen(RelativeMouseCaptureFunctions.Center(screen.ActualWidth, screen.ActualHeight));
        var (deltaX, deltaY) = RelativeMouseCaptureFunctions.Delta(new Point(current.X, current.Y), center);
        if (!RelativeMouseCaptureFunctions.HasMovement(deltaX, deltaY)) return;
        moved(deltaX, deltaY);
        SetCursorPos((int)Math.Round(center.X), (int)Math.Round(center.Y));
    }

    internal static void FocusNative(IntPtr handle) => SetFocus(handle);
    internal static void HideNativeCursor() => SetCursor(IntPtr.Zero);
    internal static bool IsButtonPressed(int virtualKey) => RelativeMouseCaptureFunctions.IsPressed(GetAsyncKeyState(virtualKey));

    private static void CenterCursor(FrameworkElement screen)
    {
        var center = RelativeMouseCaptureFunctions.Center(screen.ActualWidth, screen.ActualHeight);
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
