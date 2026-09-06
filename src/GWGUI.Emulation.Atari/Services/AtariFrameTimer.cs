using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace GWGUI.Emulation.Atari.Services;

internal sealed class AtariFrameTimer : IDisposable
{
    private const uint TimerAllAccess = 0x001F0003;
    private const uint HighResolution = 0x00000002;
    private readonly EventWaitHandle _timer = new(false, EventResetMode.AutoReset);
    private readonly WaitHandle[] _waitHandles;

    internal AtariFrameTimer(CancellationToken cancellationToken)
    {
        var handle = CreateWaitableTimerEx(nint.Zero, null, HighResolution, TimerAllAccess);
        if (handle == nint.Zero)
            handle = CreateWaitableTimerEx(nint.Zero, null, 0, TimerAllAccess);
        if (handle == nint.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
        _timer.SafeWaitHandle = new SafeWaitHandle(handle, ownsHandle: true);
        _waitHandles = [_timer, cancellationToken.WaitHandle];
    }

    internal void WaitUntil(long target, CancellationToken cancellationToken)
    {
        var remaining = target - Stopwatch.GetTimestamp();
        if (remaining <= 0) return;
        var dueTime = -Math.Max(1L, (long)Math.Ceiling(
            remaining * 10_000_000d / Stopwatch.Frequency));
        if (!SetWaitableTimer(_timer.SafeWaitHandle, ref dueTime, 0, nint.Zero, nint.Zero, false))
            throw new Win32Exception(Marshal.GetLastWin32Error());
        if (WaitHandle.WaitAny(_waitHandles) == 1) cancellationToken.ThrowIfCancellationRequested();
    }

    public void Dispose() => _timer.Dispose();

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint CreateWaitableTimerEx(nint attributes, string? name, uint flags, uint access);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWaitableTimer(SafeWaitHandle timer, ref long dueTime,
        int period, nint completionRoutine, nint argument, [MarshalAs(UnmanagedType.Bool)] bool resume);
}
