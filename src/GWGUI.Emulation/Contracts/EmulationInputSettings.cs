namespace GWGUI.Emulation.Contracts;

public sealed record EmulationInputSettings(
    EmulationInputBindingSet? Keyboard,
    EmulationInputBindingSet? Mouse,
    IReadOnlyList<EmulationControllerPort> ControllerPorts);
