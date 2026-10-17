namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IReadOnlyList<InputBindingDefinition> TrainButtons() =>
    [
        new("Train.A", string.Empty, string.Empty, "A"),
        new("Train.B", string.Empty, string.Empty, "B"),
        new("Train.C", string.Empty, string.Empty, "C"),
        Definition(EmulationControllerCommandIds.Select, InputSettingsFunctionsConstants.ControllerSelect, string.Empty),
        Definition(EmulationControllerCommandIds.Start, InputSettingsFunctionsConstants.ControllerStart, string.Empty)
    ];

    private static IReadOnlyList<InputBindingDefinition> TrainTwoHandleDefinitions() =>
    [
        .. TrainButtons(),
        new("Train.PowerIncrease", "Emulation.Sony.Input.Train.PowerIncrease", string.Empty) { OppositeDirectionId = "Train.PowerDecrease" },
        new("Train.PowerDecrease", "Emulation.Sony.Input.Train.PowerDecrease", string.Empty) { OppositeDirectionId = "Train.PowerIncrease" },
        new("Train.BrakeIncrease", "Emulation.Sony.Input.Train.BrakeIncrease", string.Empty) { OppositeDirectionId = "Train.BrakeDecrease" },
        new("Train.BrakeDecrease", "Emulation.Sony.Input.Train.BrakeDecrease", string.Empty) { OppositeDirectionId = "Train.BrakeIncrease" }
    ];

    private static IReadOnlyList<InputBindingDefinition> TrainOneHandleDefinitions() =>
    [
        .. TrainButtons(),
        new("Train.CombinedForward", "Emulation.Sony.Input.Train.CombinedForward", string.Empty) { OppositeDirectionId = "Train.CombinedBackward" },
        new("Train.CombinedBackward", "Emulation.Sony.Input.Train.CombinedBackward", string.Empty) { OppositeDirectionId = "Train.CombinedForward" }
    ];

    private static IReadOnlyList<InputBindingDefinition> JetDefinitions() =>
    [
        .. PlayStationButtonDefinitions().Where(definition => definition.Id is EmulationControllerCommandIds.A
            or EmulationControllerCommandIds.B or EmulationControllerCommandIds.X or EmulationControllerCommandIds.Y
            or EmulationControllerCommandIds.Select or EmulationControllerCommandIds.Start
            or EmulationControllerCommandIds.L or EmulationControllerCommandIds.R),
        new("Jet.RollLeft", "Emulation.Sony.Input.Jet.RollLeft", string.Empty) { OppositeDirectionId = "Jet.RollRight" },
        new("Jet.RollRight", "Emulation.Sony.Input.Jet.RollRight", string.Empty) { OppositeDirectionId = "Jet.RollLeft" },
        new("Jet.PitchUp", "Emulation.Sony.Input.Jet.PitchUp", string.Empty) { OppositeDirectionId = "Jet.PitchDown" },
        new("Jet.PitchDown", "Emulation.Sony.Input.Jet.PitchDown", string.Empty) { OppositeDirectionId = "Jet.PitchUp" },
        new("Jet.ThrottleIncrease", "Emulation.Sony.Input.Jet.ThrottleIncrease", string.Empty) { OppositeDirectionId = "Jet.ThrottleDecrease" },
        new("Jet.ThrottleDecrease", "Emulation.Sony.Input.Jet.ThrottleDecrease", string.Empty) { OppositeDirectionId = "Jet.ThrottleIncrease" }
    ];

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>? TrainAndJetVisualCommands(ControllerType type, Model model)
    {
        if (type is ControllerType.TrainController or ControllerType.UsbTrainController) return new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.FaceA] = "Train.A",
            [EmulationControllerVisualControl.FaceB] = "Train.B",
            [EmulationControllerVisualControl.FaceC] = "Train.C",
            [EmulationControllerVisualControl.FaceD] = "Train.D",
            [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
            [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
            [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
            [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
            [EmulationControllerVisualControl.Horn] = "Train.Horn",
            [EmulationControllerVisualControl.HornPedal] = "Train.HornPedal",
            [EmulationControllerVisualControl.Announcement] = "Train.Announce",
            [EmulationControllerVisualControl.Camera] = "Train.Camera",
            [EmulationControllerVisualControl.LeftDoor] = "Train.LeftDoor",
            [EmulationControllerVisualControl.RightDoor] = "Train.RightDoor",
            [EmulationControllerVisualControl.ReverserForward] = "Train.ReverserForward",
            [EmulationControllerVisualControl.ReverserNeutral] = "Train.ReverserNeutral",
            [EmulationControllerVisualControl.ReverserBackward] = "Train.ReverserBackward",
            [EmulationControllerVisualControl.Ats] = "Train.Ats",
            [EmulationControllerVisualControl.CloseDoors] = "Train.Close",
            [EmulationControllerVisualControl.Option] = EmulationControllerCommandIds.Select,
            [EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start,
            [EmulationControllerVisualControl.PowerIncrease] = "Train.PowerIncrease",
            [EmulationControllerVisualControl.PowerDecrease] = "Train.PowerDecrease",
            [EmulationControllerVisualControl.BrakeIncrease] = "Train.BrakeIncrease",
            [EmulationControllerVisualControl.BrakeDecrease] = "Train.BrakeDecrease",
            [EmulationControllerVisualControl.CombinedLeverForward] = "Train.CombinedForward",
            [EmulationControllerVisualControl.CombinedLeverBackward] = "Train.CombinedBackward"
        };
        if (type != ControllerType.JetController) return null;
        return new Dictionary<EmulationControllerVisualControl, string>(VisualCommandIds(ControllerType.Joystick, model)!)
        {
            [EmulationControllerVisualControl.StickLeft] = "Jet.RollLeft",
            [EmulationControllerVisualControl.StickRight] = "Jet.RollRight",
            [EmulationControllerVisualControl.StickUp] = "Jet.PitchDown",
            [EmulationControllerVisualControl.StickDown] = "Jet.PitchUp",
            [EmulationControllerVisualControl.PowerIncrease] = "Jet.ThrottleIncrease",
            [EmulationControllerVisualControl.PowerDecrease] = "Jet.ThrottleDecrease"
        };
    }
}
