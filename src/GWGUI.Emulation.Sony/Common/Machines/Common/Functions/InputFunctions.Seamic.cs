namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IReadOnlyList<InputBindingDefinition> SeamicDefinitions() =>
    [
        .. PlayStationButtonDefinitions().Where(definition => definition.Id is EmulationControllerCommandIds.Up
            or EmulationControllerCommandIds.Down or EmulationControllerCommandIds.Left or EmulationControllerCommandIds.Right
            or EmulationControllerCommandIds.Select or EmulationControllerCommandIds.Start),
        Definition(InputSettingsFunctionsConstants.LeftStickUp, InputSettingsFunctionsConstants.LeftStickUpLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.LeftStickDown, InputSettingsFunctionsConstants.LeftStickDownLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.LeftStickLeft, InputSettingsFunctionsConstants.LeftStickLeftLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.LeftStickRight, InputSettingsFunctionsConstants.LeftStickRightLabel, string.Empty),
        .. new[] { "A", "B", "C", "X", "Y", "Z", "L", "R", "MODE" }
            .Select(button => new InputBindingDefinition("Seamic." + button, string.Empty, string.Empty, button))
    ];

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string> SeamicVisualCommands() =>
        new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
            [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
            [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
            [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
            [EmulationControllerVisualControl.StickUp] = InputSettingsFunctionsConstants.LeftStickUp,
            [EmulationControllerVisualControl.StickDown] = InputSettingsFunctionsConstants.LeftStickDown,
            [EmulationControllerVisualControl.StickLeft] = InputSettingsFunctionsConstants.LeftStickLeft,
            [EmulationControllerVisualControl.StickRight] = InputSettingsFunctionsConstants.LeftStickRight,
            [EmulationControllerVisualControl.FaceA] = "Seamic.A",
            [EmulationControllerVisualControl.FaceB] = "Seamic.B",
            [EmulationControllerVisualControl.FaceC] = "Seamic.C",
            [EmulationControllerVisualControl.FaceX] = "Seamic.X",
            [EmulationControllerVisualControl.FaceY] = "Seamic.Y",
            [EmulationControllerVisualControl.FaceZ] = "Seamic.Z",
            [EmulationControllerVisualControl.LeftShoulder] = "Seamic.L",
            [EmulationControllerVisualControl.RightShoulder] = "Seamic.R",
            [EmulationControllerVisualControl.Option] = EmulationControllerCommandIds.Select,
            [EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start,
            [EmulationControllerVisualControl.AnalogMode] = "Seamic.MODE"
        };
}
