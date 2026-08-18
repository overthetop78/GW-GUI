using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

public enum AtariPeripheralKind
{
    None,
    Automatic,
    Keyboard,
    Mouse,
    Joystick,
    AnalogJoystick,
    Paddle,
    LightGun,
    NumericKeypad,
    DrivingController,
    ProLineController,
    EnhancedController,
    BoosterGrip,
    GenesisController,
    Joy2BPlus
}

public sealed record AtariControllerBinding(
    int Port,
    AtariPeripheralKind Peripheral,
    string? DeviceId = null,
    IReadOnlyDictionary<string, string>? Mappings = null,
    int DeadZonePercent = AtariControllerConstants.DefaultDeadZonePercent);

public sealed record AtariInputConfiguration(
    IReadOnlyDictionary<string, EmulationKey>? KeyboardMappings = null,
    IReadOnlyList<AtariControllerBinding>? Controllers = null,
    string? MouseDeviceId = null,
    bool CaptureMouse = true,
    EmulationKey ReleaseMouseKey = EmulationKey.Escape);
