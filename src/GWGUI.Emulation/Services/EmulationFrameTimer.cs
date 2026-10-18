using System.ComponentModel;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace GWGUI.Emulation.Services;

internal sealed class EmulationFrameTimer : IDisposable
{
    internal static long WaitForNextFrame(long nextFrame, double framesPerSecond,
        TimeProvider clock, Action<TimeSpan> sleep)
    {
        var frameDuration = TimeSpan.FromSeconds(1 / Math.Clamp(framesPerSecond, 1, 1000));
        nextFrame += (long)(frameDuration.TotalSeconds * clock.TimestampFrequency);
        var remaining = clock.GetElapsedTime(clock.GetTimestamp(), nextFrame);
        if (remaining > TimeSpan.Zero) sleep(remaining);
        else nextFrame = clock.GetTimestamp();
        return nextFrame;
    }

    internal static void RunLoop(long nextFrame, object gate, ConcurrentQueue<EmulationMachineCommand> commands,
        Func<bool> isPaused, Action runFrame, Func<double> framesPerSecond,
        Func<long, double, long> nextFrameTimestamp, Action<long, CancellationToken> waitUntil,
        CancellationToken cancellationToken, int pauseWaitMilliseconds)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            lock (gate)
                while (isPaused() && commands.IsEmpty && !cancellationToken.IsCancellationRequested)
                    Monitor.Wait(gate, TimeSpan.FromMilliseconds(pauseWaitMilliseconds));
            if (cancellationToken.IsCancellationRequested) break;
            while (commands.TryDequeue(out var command)) command.Execute(rethrow: true);
            lock (gate)
                if (isPaused()) continue;
            runFrame();
            nextFrame = nextFrameTimestamp(nextFrame, framesPerSecond());
            waitUntil(nextFrame, cancellationToken);
            if (nextFrame < Stopwatch.GetTimestamp()) nextFrame = Stopwatch.GetTimestamp();
        }
    }

    private const uint TimerAllAccess = 0x001F0003;
    private const uint HighResolution = 0x00000002;
    private readonly EventWaitHandle _timer = new(false, EventResetMode.AutoReset);
    private readonly WaitHandle[] _waitHandles;

    internal EmulationFrameTimer(CancellationToken cancellationToken)
    {
        var completed = false;
        SafeWaitHandle? nativeHandle = null;
        try
        {
            var handle = CreateWaitableTimerEx(nint.Zero, null, HighResolution, TimerAllAccess);
            if (handle == nint.Zero) handle = CreateWaitableTimerEx(nint.Zero, null, 0, TimerAllAccess);
            if (handle == nint.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
            nativeHandle = new SafeWaitHandle(handle, ownsHandle: true);
            var previousHandle = _timer.SafeWaitHandle;
            _timer.SafeWaitHandle = nativeHandle;
            nativeHandle = null;
            previousHandle.Dispose();
            _waitHandles = [_timer, cancellationToken.WaitHandle];
            completed = true;
        }
        finally
        {
            try { nativeHandle?.Dispose(); }
            finally { if (!completed) _timer.Dispose(); }
        }
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
