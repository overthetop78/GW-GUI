namespace GWGUI.Emulation.Thomson.Common.Machines.Common.Dictionaries;
internal static class ControllerCatalog
{
    internal static IReadOnlyList<ControllerCommand> Commands(PeripheralCategory peripheral) => peripheral switch
    {
        PeripheralCategory.None => [],
        PeripheralCategory.Joystick => Enum.GetValues<ControllerCommand>(),
        _ => throw new ArgumentOutOfRangeException(nameof(peripheral))
    };
    internal static IReadOnlyList<InputBindingDefinition> Definitions(PeripheralCategory peripheral) => Commands(peripheral).Select(command => new InputBindingDefinition(command.ToString(),
        "Emulation.Controller.Action." + (command == ControllerCommand.Fire ? "Fire1" : command.ToString()),
        string.Empty)
    {
        OppositeDirectionId = command switch
        {
            ControllerCommand.Up => nameof(ControllerCommand.Down),
            ControllerCommand.Down => nameof(ControllerCommand.Up),
            ControllerCommand.Left => nameof(ControllerCommand.Right),
            ControllerCommand.Right => nameof(ControllerCommand.Left),
            _ => null
        }
    }).ToArray();
    internal static IReadOnlyDictionary<EmulationControllerVisualControl, string> VisualCommands { get; } = new Dictionary<EmulationControllerVisualControl, string>
    {
        [EmulationControllerVisualControl.DirectionUp] = ControllerCommand.Up.ToString(),
        [EmulationControllerVisualControl.DirectionDown] = ControllerCommand.Down.ToString(),
        [EmulationControllerVisualControl.DirectionLeft] = ControllerCommand.Left.ToString(),
        [EmulationControllerVisualControl.DirectionRight] = ControllerCommand.Right.ToString(),
        [EmulationControllerVisualControl.PrimaryAction] = ControllerCommand.Fire.ToString()
    };
}
