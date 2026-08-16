using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal sealed record AtariControllerPortView(int Port,
    IReadOnlyList<AtariPeripheralKind> Peripherals, AtariPeripheralKind Selected, int DeadZonePercent,
    string? DeviceId, IReadOnlyList<InputBindingDefinition> Definitions,
    IReadOnlyDictionary<string, string> Bindings);

internal sealed record AtariInputSettingsView(
    bool HasKeyboard,
    bool HasMouse,
    IReadOnlyList<InputBindingDefinition> KeyboardDefinitions,
    IReadOnlyDictionary<string, string> KeyboardBindings,
    IReadOnlyList<InputBindingDefinition> MouseDefinitions,
    IReadOnlyDictionary<string, string> MouseBindings,
    IReadOnlyList<AtariControllerPortView> Ports,
    int MouseSpeedPercent);
