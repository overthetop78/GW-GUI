namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IReadOnlyList<InputBindingDefinition> SpecialControllerDefinitions(ControllerType type) =>
    [
        .. PlayStationButtonDefinitions(),
        .. AnalogStickDefinitions(),
        new(InputSettingsFunctionsConstants.Analog, string.Empty, string.Empty, "ANALOG"),
        type == ControllerType.Katana
            ? new("Katana.Swing", "Emulation.Sony.Input.Katana.Swing", string.Empty)
            : new("Chainsaw.Raise", "Emulation.Sony.Input.Chainsaw.Raise", string.Empty)
    ];

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>? SpecialControllerVisualCommands(ControllerType type, Model model) =>
        type is ControllerType.Katana or ControllerType.Chainsaw
            ? VisualCommandIds(ControllerType.Joystick, model) : null;
}
