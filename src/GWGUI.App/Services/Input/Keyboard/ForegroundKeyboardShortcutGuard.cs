using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;
using GWGUI.App.Functions.Input.Bindings;

namespace GWGUI.App.Services.Input.Keyboard;

internal sealed class ForegroundKeyboardShortcutGuard : IDisposable
{
    private const int LowLevelKeyboardHook = 13;
    private const uint KeyDown = 0x100;
    private const uint KeyUp = 0x101;
    private const uint SystemKeyDown = 0x104;
    private const uint SystemKeyUp = 0x105;
    private const uint ForwardedMessageMarker = 1u << 25;
    private readonly HookCallback _callback;
    private readonly HashSet<Key> _pressed = [];
    private readonly Dictionary<Key, IntPtr> _redirected = [];
    private readonly ManualResetEventSlim _ready = new();
    private readonly Thread _thread;
    private Exception? _failure;
    private uint _threadId;
    private volatile bool _stopping;
    private bool _disposed;
    private bool _filterAttached;
    private IntPtr _hook;

    internal ForegroundKeyboardShortcutGuard()
    {
        _callback = ProcessKeyboard;
        _thread = new Thread(Run) { IsBackground = true, Name = "GW GUI keyboard shortcut guard" };
        _thread.Start();
        try
        {
            if (!_ready.Wait(TimeSpan.FromSeconds(5))) throw new TimeoutException("Keyboard hook initialization timed out.");
            if (_failure is not null) throw _failure;
            ComponentDispatcher.ThreadFilterMessage += UpdateForwardedKeyboardState;
            _filterAttached = true;
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    private void Run()
    {
        try
        {
            _threadId = GetCurrentThreadId();
            PeekMessageW(out _, IntPtr.Zero, 0, 0, 0);
            _hook = SetWindowsHookExW(LowLevelKeyboardHook, _callback, GetModuleHandleW(null), 0);
            if (_hook == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
            _ready.Set();
            while (!_stopping)
            {
                var result = GetMessageW(out var message, IntPtr.Zero, 0, 0);
                if (result < 0) throw new Win32Exception(Marshal.GetLastWin32Error());
                if (result == 0) break;
                TranslateMessage(ref message);
                DispatchMessageW(ref message);
            }
        }
        catch (Exception error) { _failure = error; }
        finally
        {
            try
            {
                if (_hook != IntPtr.Zero && !UnhookWindowsHookEx(_hook)) _failure ??= new Win32Exception(Marshal.GetLastWin32Error());
            }
            finally
            {
                _hook = IntPtr.Zero;
                _ready.Set();
            }
        }
    }

    internal static bool ShouldIntercept(Key key, ModifierKeys modifiers) => !KeyboardChordFunctions.IsModifierKey(key)
        && modifiers != ModifierKeys.None && !KeyboardChordFunctions.IsWindowsReserved(new GWGUI.App.Contracts.Input.KeyboardChord(modifiers, [key]));

    private IntPtr ProcessKeyboard(int code, IntPtr message, IntPtr pointer)
    {
        if (code < 0) return CallNextHookEx(_hook, code, message, pointer);
        var data = Marshal.PtrToStructure<KeyboardData>(pointer);
        var key = KeyInterop.KeyFromVirtualKey((int)data.VirtualKey);
        var down = (uint)message.ToInt64() is KeyDown or SystemKeyDown;
        var up = (uint)message.ToInt64() is KeyUp or SystemKeyUp;
        if (!down && !up) return CallNextHookEx(_hook, code, message, pointer);
        var repeated = !_pressed.Add(key);
        if (up) _pressed.Remove(key);
        var foreground = GetForegroundWindow();
        var foregroundThread = GetWindowThreadProcessId(foreground, out var process);
        var ownForeground = process == (uint)Environment.ProcessId;
        if (_redirected.TryGetValue(key, out var destination))
        {
            if (up) _redirected.Remove(key);
            if (ownForeground) Forward(destination, data, up, repeated);
            return new IntPtr(1);
        }
        var modifiers = _pressed.Aggregate(ModifierKeys.None, (value, pressed) => value | KeyboardChordFunctions.ModifierForKey(pressed));
        if (!down || !ownForeground || !ShouldIntercept(key, modifiers)) return CallNextHookEx(_hook, code, message, pointer);
        var info = new GuiThreadInfo { Size = (uint)Marshal.SizeOf<GuiThreadInfo>() };
        var focus = GetGUIThreadInfo(foregroundThread, ref info) ? info.Focus : IntPtr.Zero;
        if (focus == IntPtr.Zero) focus = foreground;
        // Post only to this process, never inject a second global keyboard event.
        if (!Forward(focus, data, false, repeated)) return CallNextHookEx(_hook, code, message, pointer);
        _redirected[key] = focus;
        return new IntPtr(1);
    }

    private static bool Forward(IntPtr window, KeyboardData data, bool up, bool repeated)
    {
        uint flags = 1 | (data.ScanCode << 16) | ForwardedMessageMarker;
        if ((data.Flags & 1) != 0) flags |= 1u << 24;
        if (up || repeated) flags |= 1u << 30;
        if (up) flags |= 1u << 31;
        return PostMessageW(window, up ? KeyUp : KeyDown, (IntPtr)data.VirtualKey, new IntPtr(unchecked((int)flags)));
    }

    public void Dispose()
    {
        if (_disposed) return;
        if (_filterAttached)
        {
            ComponentDispatcher.ThreadFilterMessage -= UpdateForwardedKeyboardState;
            _filterAttached = false;
        }
        _stopping = true;
        if (_threadId != 0) PostThreadMessageW(_threadId, 0x12, IntPtr.Zero, IntPtr.Zero);
        if (!_thread.Join(TimeSpan.FromSeconds(5))) throw new TimeoutException("Keyboard hook thread did not terminate.");
        _disposed = true;
        _ready.Dispose();
        _pressed.Clear();
        _redirected.Clear();
        GC.KeepAlive(_callback);
    }

    private static void UpdateForwardedKeyboardState(ref MSG message, ref bool handled)
    {
        if (message.message is not (int)KeyDown and not (int)KeyUp || (message.lParam.ToInt64() & ForwardedMessageMarker) == 0) return;
        var key = message.wParam.ToInt64();
        if (key is < 0 or > 255) return;
        var state = new byte[256];
        if (GetKeyboardState(state))
        {
            state[key] = (byte)((state[key] & 0x7f) | (message.message == KeyDown ? 0x80 : 0));
            SetKeyboardState(state);
        }
        message.lParam = new IntPtr(message.lParam.ToInt64() & ~((long)ForwardedMessageMarker));
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeMessage
    {
        internal IntPtr Window;
        internal uint Message;
        internal UIntPtr WParam;
        internal IntPtr LParam;
        internal uint Time;
        internal int X;
        internal int Y;
        internal uint Private;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct GuiThreadInfo
    {
        internal uint Size;
        internal uint Flags;
        internal IntPtr Active;
        internal IntPtr Focus;
        internal IntPtr Capture;
        internal IntPtr MenuOwner;
        internal IntPtr MoveSize;
        internal IntPtr Caret;
        internal int Left;
        internal int Top;
        internal int Right;
        internal int Bottom;
    }

    private delegate IntPtr HookCallback(int code, IntPtr message, IntPtr data);
    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardData
    {
        internal uint VirtualKey;
        internal uint ScanCode;
        internal uint Flags;
        internal uint Time;
        internal UIntPtr ExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)] private static extern IntPtr SetWindowsHookExW(int id, HookCallback callback, IntPtr module, uint thread);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool UnhookWindowsHookEx(IntPtr hook);
    [DllImport("user32.dll")] private static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr message, IntPtr data);
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetGUIThreadInfo(uint thread, ref GuiThreadInfo info);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr window, out uint process);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool PostMessageW(IntPtr window, uint message, IntPtr key, IntPtr flags);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern IntPtr GetModuleHandleW(string? name);
    [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
    [DllImport("user32.dll", SetLastError = true)] private static extern int GetMessageW(out NativeMessage message, IntPtr window, uint minimum, uint maximum);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool PeekMessageW(out NativeMessage message, IntPtr window, uint minimum, uint maximum, uint flags);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool TranslateMessage(ref NativeMessage message);
    [DllImport("user32.dll")] private static extern IntPtr DispatchMessageW(ref NativeMessage message);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool PostThreadMessageW(uint thread, uint message, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetKeyboardState([Out] byte[] state);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SetKeyboardState(byte[] state);
}
