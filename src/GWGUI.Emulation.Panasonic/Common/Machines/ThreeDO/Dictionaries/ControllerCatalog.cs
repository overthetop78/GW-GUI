namespace GWGUI.Emulation.Panasonic.Common.Machines.ThreeDO.Dictionaries;
internal static class ControllerCatalog
{
    internal static IReadOnlyList<ControllerCommand> Commands(PeripheralCategory peripheral) => peripheral switch
    {
        PeripheralCategory.None => [],
        PeripheralCategory.ThreeDOPad => [
            ControllerCommand.Up,
            ControllerCommand.Down,
            ControllerCommand.Left,
            ControllerCommand.Right,
            ControllerCommand.A,
            ControllerCommand.B,
            ControllerCommand.C,
            ControllerCommand.X,
            ControllerCommand.P,
            ControllerCommand.L,
            ControllerCommand.R
        ],
        PeripheralCategory.Flightstick => [
            ControllerCommand.Up,
            ControllerCommand.Down,
            ControllerCommand.Left,
            ControllerCommand.Right,
            ControllerCommand.A,
            ControllerCommand.B,
            ControllerCommand.C,
            ControllerCommand.X,
            ControllerCommand.P,
            ControllerCommand.L,
            ControllerCommand.R,
            ControllerCommand.Fire,
            ControllerCommand.PitchUp,
            ControllerCommand.PitchDown,
            ControllerCommand.YawLeft,
            ControllerCommand.YawRight,
            ControllerCommand.ThrottleUp,
            ControllerCommand.ThrottleDown
        ],
        PeripheralCategory.Mouse => [
            ControllerCommand.MoveUp,
            ControllerCommand.MoveDown,
            ControllerCommand.MoveLeft,
            ControllerCommand.MoveRight,
            ControllerCommand.LeftButton,
            ControllerCommand.MiddleButton,
            ControllerCommand.RightButton,
            ControllerCommand.Shift
        ],
        PeripheralCategory.Lightgun => [ControllerCommand.AimUp, ControllerCommand.AimDown, ControllerCommand.AimLeft, ControllerCommand.AimRight, ControllerCommand.Trigger, ControllerCommand.Option, ControllerCommand.Reload],
        PeripheralCategory.ArcadeLightgun => [
            ControllerCommand.AimUp,
            ControllerCommand.AimDown,
            ControllerCommand.AimLeft,
            ControllerCommand.AimRight,
            ControllerCommand.Trigger,
            ControllerCommand.Service,
            ControllerCommand.Coin,
            ControllerCommand.Start,
            ControllerCommand.Reload
        ],
        PeripheralCategory.OrbatakTrackball => [
            ControllerCommand.MoveUp,
            ControllerCommand.MoveDown,
            ControllerCommand.MoveLeft,
            ControllerCommand.MoveRight,
            ControllerCommand.StartPlayer1,
            ControllerCommand.StartPlayer2,
            ControllerCommand.CoinPlayer1,
            ControllerCommand.CoinPlayer2,
            ControllerCommand.Service
        ],
        _ => throw new ArgumentOutOfRangeException(nameof(peripheral))
    };
    internal static IReadOnlyList<InputBindingDefinition> Definitions(PeripheralCategory peripheral) => Commands(peripheral).Select(command => (command switch
    {
        ControllerCommand.Up or ControllerCommand.Down or ControllerCommand.Left or ControllerCommand.Right => new InputBindingDefinition(command.ToString(), "Emulation.Controller.Action." + command, string.Empty),
        ControllerCommand.Fire or ControllerCommand.Trigger => new InputBindingDefinition(command.ToString(), "Emulation.Controller.Action.Fire1", string.Empty),
        ControllerCommand.LeftButton => new InputBindingDefinition(command.ToString(), "Emulation.Mouse.Button.Left", string.Empty),
        ControllerCommand.RightButton => new InputBindingDefinition(command.ToString(), "Emulation.Mouse.Button.Right", string.Empty),
        ControllerCommand.MiddleButton => new InputBindingDefinition(command.ToString(), "Emulation.Mouse.Button.Middle", string.Empty),
        _ => new InputBindingDefinition(command.ToString(), string.Empty, string.Empty, Label(command))
    }) with { OppositeDirectionId = OppositeDirection(command) }).ToArray();
    private static string? OppositeDirection(ControllerCommand command) => command switch
    {
        ControllerCommand.Up => ControllerCommand.Down.ToString(),
        ControllerCommand.Down => ControllerCommand.Up.ToString(),
        ControllerCommand.Left => ControllerCommand.Right.ToString(),
        ControllerCommand.Right => ControllerCommand.Left.ToString(),
        ControllerCommand.PitchUp => ControllerCommand.PitchDown.ToString(),
        ControllerCommand.PitchDown => ControllerCommand.PitchUp.ToString(),
        ControllerCommand.YawLeft => ControllerCommand.YawRight.ToString(),
        ControllerCommand.YawRight => ControllerCommand.YawLeft.ToString(),
        ControllerCommand.ThrottleUp => ControllerCommand.ThrottleDown.ToString(),
        ControllerCommand.ThrottleDown => ControllerCommand.ThrottleUp.ToString(),
        ControllerCommand.MoveUp => ControllerCommand.MoveDown.ToString(),
        ControllerCommand.MoveDown => ControllerCommand.MoveUp.ToString(),
        ControllerCommand.MoveLeft => ControllerCommand.MoveRight.ToString(),
        ControllerCommand.MoveRight => ControllerCommand.MoveLeft.ToString(),
        ControllerCommand.AimUp => ControllerCommand.AimDown.ToString(),
        ControllerCommand.AimDown => ControllerCommand.AimUp.ToString(),
        ControllerCommand.AimLeft => ControllerCommand.AimRight.ToString(),
        ControllerCommand.AimRight => ControllerCommand.AimLeft.ToString(),
        _ => null
    };

    private static string Label(ControllerCommand command) => command switch
    {
        ControllerCommand.PitchUp or ControllerCommand.MoveUp or ControllerCommand.AimUp => "Y−",
        ControllerCommand.PitchDown or ControllerCommand.MoveDown or ControllerCommand.AimDown => "Y+",
        ControllerCommand.YawLeft or ControllerCommand.MoveLeft or ControllerCommand.AimLeft => "X−",
        ControllerCommand.YawRight or ControllerCommand.MoveRight or ControllerCommand.AimRight => "X+",
        ControllerCommand.ThrottleUp => "Z+",
        ControllerCommand.ThrottleDown => "Z−",
        ControllerCommand.StartPlayer1 => "START 1",
        ControllerCommand.StartPlayer2 => "START 2",
        ControllerCommand.CoinPlayer1 => "COIN 1",
        ControllerCommand.CoinPlayer2 => "COIN 2",
        _ => command.ToString().ToUpperInvariant()
    };
    internal static IReadOnlyDictionary<EmulationControllerVisualControl, string> VisualCommands(PeripheralCategory peripheral) => peripheral == PeripheralCategory.ThreeDOPad 
        ? new Dictionary<EmulationControllerVisualControl, string>
    {
        [EmulationControllerVisualControl.DirectionUp] = ControllerCommand.Up.ToString(),
        [EmulationControllerVisualControl.DirectionDown] = ControllerCommand.Down.ToString(),
        [EmulationControllerVisualControl.DirectionLeft] = ControllerCommand.Left.ToString(),
        [EmulationControllerVisualControl.DirectionRight] = ControllerCommand.Right.ToString(),
        [EmulationControllerVisualControl.FaceA] = ControllerCommand.A.ToString(),
        [EmulationControllerVisualControl.FaceB] = ControllerCommand.B.ToString(),
        [EmulationControllerVisualControl.FaceC] = ControllerCommand.C.ToString(),
        [EmulationControllerVisualControl.Stop] = ControllerCommand.X.ToString(),
        [EmulationControllerVisualControl.PlayPause] = ControllerCommand.P.ToString(),
        [EmulationControllerVisualControl.LeftShoulder] = ControllerCommand.L.ToString(),
        [EmulationControllerVisualControl.RightShoulder] = ControllerCommand.R.ToString()
        }


        : new Dictionary<EmulationControllerVisualControl, string>();
}
