namespace GWGUI.Emulation.Atari;

public sealed record AtariControllerPortView(
    int Port,
    IReadOnlyList<AtariPeripheralKind> Peripherals,
    AtariPeripheralKind Selected,
    int DeadZonePercent,
    string? DeviceId,
    IReadOnlyList<InputBindingDefinition> Definitions,
    IReadOnlyDictionary<string, string> Bindings);

public sealed record AtariInputSettingsView(
    bool HasKeyboard,
    bool HasMouse,
    IReadOnlyList<InputBindingDefinition> KeyboardDefinitions,
    IReadOnlyDictionary<string, string> KeyboardBindings,
    IReadOnlyList<InputBindingDefinition> MouseDefinitions,
    IReadOnlyDictionary<string, string> MouseBindings,
    IReadOnlyList<AtariControllerPortView> Ports,
    int MouseSpeedPercent,
    bool HasEightBitControllerOptions,
    string PaddleMovementSpeed,
    string AutofireMode,
    string ControllerCompatibilityMode,
    string DigitalSensitivity,
    string AnalogSensitivity);
