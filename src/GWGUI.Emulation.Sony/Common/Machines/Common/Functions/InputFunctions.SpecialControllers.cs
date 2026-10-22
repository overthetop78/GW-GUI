namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IReadOnlyList<InputBindingDefinition> SpecialControllerDefinitions(PeripheralCategory type) =>
    [
        .. PlayStationButtonDefinitions(),
        .. AnalogStickDefinitions(),
        new(InputSettingsFunctionsConstants.Analog, string.Empty, string.Empty, "ANALOG"),
        type == PeripheralCategory.Katana
            ? new("Katana.Swing", "Emulation.Sony.Input.Katana.Swing", string.Empty)
            : new("Chainsaw.Raise", "Emulation.Sony.Input.Chainsaw.Raise", string.Empty)
    ];

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>? SpecialControllerVisualCommands(PeripheralCategory type, Model model) =>
        type is PeripheralCategory.Katana or PeripheralCategory.Chainsaw
            ? VisualCommandIds(PeripheralCategory.Joystick, model) : null;
}
