namespace GWGUI.Emulation.Amiga;

public sealed record AmigaInputConfiguration(
    IReadOnlyDictionary<string, EmulationKey>? KeyboardMappings = null,
    string? MouseDeviceId = null,
    bool CaptureMouse = true,
    IReadOnlyList<AmigaControllerBinding>? ControllerBindings = null,
    IReadOnlyDictionary<string, AmigaMouseAction>? MouseButtonMappings = null,
    EmulationKey ReleaseMouseKey = EmulationKey.Escape,
    IReadOnlyDictionary<string, string>? KeyboardBindings = null,
    string? ReleaseMouseBinding = null,
    bool ParallelJoystickAdapterEnabled = false);
