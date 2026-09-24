namespace GWGUI.App.Services.Input.GameInput;

internal sealed record PendingGameInputDeviceChange(
    ulong Token,
    IntPtr Context,
    IGameInputDevice Device,
    IntPtr Lifetime,
    ulong Timestamp,
    GameInputDeviceStatus CurrentStatus,
    GameInputDeviceStatus PreviousStatus);
