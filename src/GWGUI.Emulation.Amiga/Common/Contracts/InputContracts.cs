namespace GWGUI.Emulation.Amiga.Common.Contracts;

public sealed record InputConfiguration(
    IReadOnlyDictionary<string, EmulationKey>? KeyboardMappings = null,
    string? MouseDeviceId = null,
    bool CaptureMouse = true,
    IReadOnlyList<ControllerBinding>? ControllerBindings = null,
    IReadOnlyDictionary<string, MouseAction>? MouseButtonMappings = null,
    EmulationKey ReleaseMouseKey = EmulationKey.Escape,
    IReadOnlyDictionary<string, string>? KeyboardBindings = null,
    string? ReleaseMouseBinding = null,
    bool ParallelJoystickAdapterEnabled = false);
