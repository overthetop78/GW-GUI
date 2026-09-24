using System.Runtime.InteropServices;

namespace GWGUI.App.Services.Input.GameInput;

internal static class GameInputResourceReleaser
{
    internal static bool SafeUnregister(IGameInput? gameInput, ulong token)
    {
        if (gameInput is null || token == 0) return true;
        try { return gameInput.UnregisterCallback(token); }
        catch (Exception exception) when (IsInteropFailure(exception)) { return false; }
    }

    internal static void ReleaseEntryResources(GameInputDeviceEntry entry)
    {
        if (entry.RawReadingContext != IntPtr.Zero)
        {
            var handle = GCHandle.FromIntPtr(entry.RawReadingContext);
            if (handle.IsAllocated) handle.Free();
        }
        if (entry.DevicePointer != IntPtr.Zero) Marshal.Release(entry.DevicePointer);
        Release(entry.Device);
        Release(entry.Mapper);
        entry.HidDecoder?.Dispose();
    }

    internal static bool IsInteropFailure(Exception exception) => exception is
        COMException or InvalidComObjectException or InvalidCastException or InvalidOperationException or
        ArgumentException or OverflowException;

    internal static void Release(object? value)
    {
        if (value is not null && Marshal.IsComObject(value)) Marshal.ReleaseComObject(value);
    }

    internal static void FinalRelease(object? value)
    {
        if (value is not null && Marshal.IsComObject(value)) Marshal.FinalReleaseComObject(value);
    }
}
