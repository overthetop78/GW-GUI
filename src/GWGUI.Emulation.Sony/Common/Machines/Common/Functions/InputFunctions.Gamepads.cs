namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IReadOnlyList<InputBindingDefinition> LogitechCordlessDefinitions() =>
    [
        .. PlayStationButtonDefinitions(),
        .. AnalogStickDefinitions(),
        new(InputSettingsFunctionsConstants.Analog, string.Empty, string.Empty, "MODE"),
        new("Logitech.Vibration", string.Empty, string.Empty, "VIBRATION")
    ];

    private static IReadOnlyList<InputBindingDefinition> SegaSaturnPs2Definitions() => PlayStationButtonDefinitions()
        .Where(definition => definition.Id is not (EmulationControllerCommandIds.L3 or EmulationControllerCommandIds.R3))
        .Select(definition => definition with
        {
            InvariantDisplayValue = definition.Id switch
            {
                EmulationControllerCommandIds.B => "A (×)",
                EmulationControllerCommandIds.A => "B (○)",
                EmulationControllerCommandIds.R => "C (R1)",
                EmulationControllerCommandIds.Y => "X (□)",
                EmulationControllerCommandIds.X => "Y (△)",
                EmulationControllerCommandIds.L => "Z (L1)",
                EmulationControllerCommandIds.L2 => "L (L2)",
                EmulationControllerCommandIds.R2 => "R (R2)",
                _ => definition.InvariantDisplayValue
            }
        }).ToArray();

    private static IReadOnlyList<InputBindingDefinition> HoriFightingStick2Definitions() =>
    [
        .. PlayStationButtonDefinitions(),
        new(InputSettingsFunctionsConstants.Analog, string.Empty, string.Empty, "MODE"),
        new("Turbo", string.Empty, string.Empty, "TURBO"),
        new("Hori.Speed", "Emulation.Sony.Input.Hori.Speed", string.Empty),
        new("Hori.Stick", "Emulation.Sony.Input.Hori.Stick", string.Empty)
    ];

    private static IReadOnlyList<InputBindingDefinition> HoriRealArcadeProDefinitions() =>
    [
        .. PlayStationButtonDefinitions().Where(definition => definition.Id is not (EmulationControllerCommandIds.L3 or EmulationControllerCommandIds.R3)),
        .. Enumerable.Range(1, 8).Select(index => new InputBindingDefinition("Arcade.Turbo" + index, "Emulation.Sony.Input.Hori.Turbo" + index, string.Empty))
    ];

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string> ArcadeVisualCommands(Model model)
    {
        var commands = new Dictionary<EmulationControllerVisualControl, string>(VisualCommandIds(PeripheralCategory.Joystick, model)!)
        {
            [EmulationControllerVisualControl.Turbo] = "Turbo",
            [EmulationControllerVisualControl.RepeatRate] = "Hori.Speed",
            [EmulationControllerVisualControl.StickMode] = "Hori.Stick"
        };
        foreach (var index in Enumerable.Range(1, 8))
            commands[Enum.Parse<EmulationControllerVisualControl>("Key" + index)] = "Arcade.Turbo" + index;
        return commands;
    }
}
