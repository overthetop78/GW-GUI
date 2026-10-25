namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IEnumerable<InputBindingDefinition> SerialWheelButtons() => PlayStationButtonDefinitions()
        .Where(definition => definition.Id is not (EmulationControllerCommandIds.L3 or EmulationControllerCommandIds.R3));

    private static InputBindingDefinition WheelPrintedDefinition(string id, string label) => new(id, string.Empty, string.Empty, label);

    private static IEnumerable<InputBindingDefinition> SerialWheelLeverDefinitions() =>
    [
        new("Wheel.LeverForward", "Emulation.Sony.Input.Wheel.LeverForward", string.Empty) { OppositeDirectionId = "Wheel.LeverBackward" },
        new("Wheel.LeverBackward", "Emulation.Sony.Input.Wheel.LeverBackward", string.Empty) { OppositeDirectionId = "Wheel.LeverForward" }
    ];

    private static IReadOnlyList<InputBindingDefinition> MadCatzWheelDefinitions() =>
    [
        .. SerialWheelButtons().Select(definition => definition.Id switch
        {
            EmulationControllerCommandIds.B => WheelPrintedDefinition(definition.Id, "I / ×"),
            EmulationControllerCommandIds.Y => WheelPrintedDefinition(definition.Id, "II / □"),
            EmulationControllerCommandIds.A => WheelPrintedDefinition(definition.Id, "A / ○"),
            EmulationControllerCommandIds.X => WheelPrintedDefinition(definition.Id, "B / △"),
            EmulationControllerCommandIds.L2 => WheelPrintedDefinition(definition.Id, "L / L2"),
            EmulationControllerCommandIds.R2 => WheelPrintedDefinition(definition.Id, "R / R2"),
            _ => definition
        }),
        .. WheelAxesDefinitions(),
        .. SerialWheelLeverDefinitions(),
        WheelPrintedDefinition("Wheel.Mode", "MODE")
    ];

    private static IReadOnlyList<InputBindingDefinition> InteractV3Definitions() =>
    [
        .. SerialWheelButtons(),
        .. WheelAxesDefinitions(),
        WheelPrintedDefinition("Wheel.Set", "SET")
    ];

    private static IReadOnlyList<InputBindingDefinition> Speedster2Definitions() =>
    [
        .. SerialWheelButtons(),
        .. WheelAxesDefinitions(),
        .. SerialWheelLeverDefinitions(),
        WheelPrintedDefinition("Wheel.Mode", "MODE")
    ];

    private static IReadOnlyList<InputBindingDefinition> Speedster3Definitions() =>
    [
        .. PlayStationButtonDefinitions().Where(definition => definition.Id is not (EmulationControllerCommandIds.Up
            or EmulationControllerCommandIds.Down or EmulationControllerCommandIds.Left or EmulationControllerCommandIds.Right)),
        .. WheelAxesDefinitions(),
        WheelPrintedDefinition("Wheel.Mode", "MODE"),
        WheelPrintedDefinition("Wheel.Tuning", "TUNING")
    ];

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string> SerialWheelVisualCommands() =>
        new Dictionary<EmulationControllerVisualControl, string>(WheelVisualCommands())
        {
            [EmulationControllerVisualControl.AnalogMode] = "Wheel.Mode",
            [EmulationControllerVisualControl.Program] = "Wheel.Set",
            [EmulationControllerVisualControl.Menu] = "Wheel.Tuning",
            // Speedster 3's central horn shares the physical L2 input.
            [EmulationControllerVisualControl.Buzzer] = EmulationControllerCommandIds.L2
        };
}
