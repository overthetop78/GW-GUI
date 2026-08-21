namespace GWGUI.Emulation;

public sealed record EmulationInputSettings(
    EmulationInputBindingSet? Keyboard,
    EmulationInputBindingSet? Mouse,
    IReadOnlyList<EmulationControllerPort> ControllerPorts);
