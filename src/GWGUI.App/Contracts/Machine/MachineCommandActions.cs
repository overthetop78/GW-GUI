namespace GWGUI.App.Contracts.Machine;

internal sealed record MachineCommandActions(
    Func<Task> TogglePower,
    Func<Task> TogglePause,
    Func<Task> SoftReset,
    Func<Task> HardReset,
    Func<Task> QuickSave,
    Func<Task> QuickLoad,
    Func<Task> CaptureScreen,
    Func<Task> ToggleFullscreen,
    Func<Task> ToggleAudio,
    Func<Task>? SwitchControllerPointer = null);
