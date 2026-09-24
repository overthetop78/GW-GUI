using GWGUI.App.Contracts.Services.Input;
using GWGUI.Emulation;

namespace GWGUI.App.Services.Input.GameInput;

internal static class GameInputControllerReader
{
    internal static IReadOnlyList<GameInputKind> RegisteredDeviceCallbackFilters =>
        GameInputDeviceMonitor.RegisteredDeviceCallbackFilters;
    internal static IReadOnlyList<GameInputKind> RegisteredControllerRefreshFilters =>
        GameInputDeviceMonitor.RegisteredControllerRefreshFilters;
    internal static bool InitializationFailed => GameInputDeviceMonitor.InitializationFailed;
    internal static string LastReadDiagnostic => GameInputDeviceMonitor.LastReadDiagnostic;
    internal static string LastDetailedReadDiagnostic => GameInputDeviceMonitor.LastDetailedReadDiagnostic;
    internal static string LastCallbackDiagnostic => GameInputDeviceMonitor.LastCallbackDiagnostic;
    internal static string LastEnumerationDiagnostic => GameInputDeviceMonitor.LastEnumerationDiagnostic;
    internal static string RawEnumerationDiagnostic => GameInputDeviceMonitor.RawEnumerationDiagnostic;
    internal static string DeviceCallbackTrace => GameInputDeviceMonitor.DeviceCallbackTrace;

    internal static void StartMonitoring() => GameInputDeviceMonitor.StartMonitoring();
    internal static void StopMonitoring() => GameInputDeviceMonitor.StopMonitoring();
    internal static IReadOnlyList<GameInputDeviceDescriptor> GetConnectedControllerDetailsCached() =>
        GameInputDeviceMonitor.GetConnectedControllerDetailsCached();
    internal static IReadOnlyList<EmulationControllerState> ReadAll() => GameInputDeviceMonitor.ReadAll();
    internal static IReadOnlyList<GameControllerDevice> GetConnectedDevices() =>
        GameInputDeviceMonitor.GetConnectedDevices();
    internal static IReadOnlyList<GameInputDeviceDescriptor> GetConnectedControllerDetails() =>
        GameInputDeviceMonitor.GetConnectedControllerDetails();
    internal static GameInputLiveState ReadDetailedState(string deviceId) =>
        GameInputDeviceMonitor.ReadDetailedState(deviceId);
    internal static IReadOnlyList<GameInputLiveState> ReadAllDetailedStates() =>
        GameInputDeviceMonitor.ReadAllDetailedStates();
    internal static string? GetControllerName(string deviceId) =>
        GameInputDeviceMonitor.GetControllerName(deviceId);
    internal static void RefreshConnectedDevices() => GameInputDeviceMonitor.RefreshConnectedDevices();
    internal static void SetFocusPolicyForDiagnostics(GameInputFocusPolicy policy) =>
        GameInputDeviceMonitor.SetFocusPolicyForDiagnostics(policy);
    internal static bool SetRumble(
        string deviceId,
        float lowFrequency,
        float highFrequency,
        float leftTrigger,
        float rightTrigger) => GameInputDeviceMonitor.SetRumble(
            deviceId, lowFrequency, highFrequency, leftTrigger, rightTrigger);
    internal static GameInputPhysicalState ReadPhysicalInput() =>
        GameInputDeviceMonitor.ReadPhysicalInput();
    internal static EmulationControllerState MapGamepad(
        string deviceId,
        GameInputGamepadState gamepad) =>
        GameInputDeviceMonitor.MapGamepad(deviceId, gamepad);
}
