namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IReadOnlyList<InputBindingDefinition> WheelAxesDefinitions() =>
    [
        new("Wheel.Left", InputSettingsFunctionsConstants.ResourceSteeringLeft, string.Empty) { OppositeDirectionId = "Wheel.Right" },
        new("Wheel.Right", InputSettingsFunctionsConstants.ResourceSteeringRight, string.Empty) { OppositeDirectionId = "Wheel.Left" },
        new("Wheel.Accelerator", "Controllers.Throttle", string.Empty),
        new("Wheel.Brake", "Controllers.Brake", string.Empty)
    ];

    private static IReadOnlyList<InputBindingDefinition> GtForceDefinitions() =>
    [
        .. WheelAxesDefinitions(),
        new("Wheel.FrontUpperLeft", "Emulation.Sony.Input.Wheel.FrontUpperLeft", string.Empty),
        new("Wheel.FrontLowerLeft", "Emulation.Sony.Input.Wheel.FrontLowerLeft", string.Empty),
        new("Wheel.FrontUpperRight", "Emulation.Sony.Input.Wheel.FrontUpperRight", string.Empty),
        new("Wheel.FrontLowerRight", "Emulation.Sony.Input.Wheel.FrontLowerRight", string.Empty),
        new(EmulationControllerCommandIds.L, string.Empty, string.Empty, "L"),
        new(EmulationControllerCommandIds.R, string.Empty, string.Empty, "R")
    ];

    private static IReadOnlyList<InputBindingDefinition> DrivingForceDefinitions() =>
    [
        .. PlayStationButtonDefinitions(),
        .. WheelAxesDefinitions()
    ];

    private static IReadOnlyList<InputBindingDefinition> DrivingForceProDefinitions() =>
    [
        .. DrivingForceDefinitions(),
        new("Wheel.LeverForward", "Emulation.Sony.Input.Wheel.LeverForward", string.Empty) { OppositeDirectionId = "Wheel.LeverBackward" },
        new("Wheel.LeverBackward", "Emulation.Sony.Input.Wheel.LeverBackward", string.Empty) { OppositeDirectionId = "Wheel.LeverForward" }
    ];

    private static IReadOnlyList<InputBindingDefinition> DrivingForceGtDefinitions() =>
    [
        .. DrivingForceProDefinitions(),
        new("Wheel.PS", string.Empty, string.Empty, "PS"),
        new("Wheel.Horn", "Emulation.Sony.Input.Wheel.Horn", string.Empty),
        new("Wheel.Plus", string.Empty, string.Empty, "+"),
        new("Wheel.Minus", string.Empty, string.Empty, "−"),
        new("Wheel.DialLeft", "Emulation.Sony.Input.Wheel.DialLeft", string.Empty) { OppositeDirectionId = "Wheel.DialRight" },
        new("Wheel.DialRight", "Emulation.Sony.Input.Wheel.DialRight", string.Empty) { OppositeDirectionId = "Wheel.DialLeft" },
        new("Wheel.DialPress", "Emulation.Sony.Input.Wheel.DialPress", string.Empty)
    ];

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string> WheelVisualCommands() => new Dictionary<EmulationControllerVisualControl, string>
    {
        [EmulationControllerVisualControl.RotaryLeft] = "Wheel.Left",
        [EmulationControllerVisualControl.RotaryRight] = "Wheel.Right",
        [EmulationControllerVisualControl.Accelerator] = "Wheel.Accelerator",
        [EmulationControllerVisualControl.BrakePedal] = "Wheel.Brake",
        [EmulationControllerVisualControl.FaceA] = "Wheel.FrontUpperLeft",
        [EmulationControllerVisualControl.FaceB] = "Wheel.FrontLowerLeft",
        [EmulationControllerVisualControl.FaceX] = "Wheel.FrontUpperRight",
        [EmulationControllerVisualControl.FaceY] = "Wheel.FrontLowerRight",
        [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
        [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
        [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
        [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
        [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
        [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A,
        [EmulationControllerVisualControl.TertiaryAction] = EmulationControllerCommandIds.Y,
        [EmulationControllerVisualControl.QuaternaryAction] = EmulationControllerCommandIds.X,
        [EmulationControllerVisualControl.LeftShoulder] = EmulationControllerCommandIds.L,
        [EmulationControllerVisualControl.RightShoulder] = EmulationControllerCommandIds.R,
        [EmulationControllerVisualControl.LeftTrigger] = EmulationControllerCommandIds.L2,
        [EmulationControllerVisualControl.RightTrigger] = EmulationControllerCommandIds.R2,
        [EmulationControllerVisualControl.LeftStick] = EmulationControllerCommandIds.L3,
        [EmulationControllerVisualControl.RightStick] = EmulationControllerCommandIds.R3,
        [EmulationControllerVisualControl.Option] = EmulationControllerCommandIds.Select,
        [EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start,
        [EmulationControllerVisualControl.GearUp] = "Wheel.LeverForward",
        [EmulationControllerVisualControl.GearDown] = "Wheel.LeverBackward",
        [EmulationControllerVisualControl.Guide] = "Wheel.PS",
        [EmulationControllerVisualControl.Buzzer] = "Wheel.Horn",
        [EmulationControllerVisualControl.Key1] = "Wheel.Plus",
        [EmulationControllerVisualControl.Key2] = "Wheel.Minus",
        [EmulationControllerVisualControl.DialLeft] = "Wheel.DialLeft",
        [EmulationControllerVisualControl.DialRight] = "Wheel.DialRight",
        [EmulationControllerVisualControl.DialPress] = "Wheel.DialPress"
    };
}
