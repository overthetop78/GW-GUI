namespace GWGUI.Emulation.Watara.Common.Machines.Common.Dictionaries;

internal static class ControllerCatalog
{
    internal static IReadOnlyList<ControllerCommand> Commands { get; } = Enum.GetValues<ControllerCommand>();
    internal static IReadOnlyList<InputBindingDefinition> Definitions { get; } = Commands.Select(command => command switch
    {
        ControllerCommand.Up or ControllerCommand.Down or ControllerCommand.Left or ControllerCommand.Right => new InputBindingDefinition(command.ToString(), "Emulation.Controller.Action." + command, string.Empty),
        _ => new InputBindingDefinition(command.ToString(), string.Empty, string.Empty, command.ToString().ToUpperInvariant())}).ToArray();
    internal static IReadOnlyDictionary<EmulationControllerVisualControl, string> VisualCommands { get; } = new Dictionary<EmulationControllerVisualControl, string>
    {
        [EmulationControllerVisualControl.DirectionUp] = ControllerCommand.Up.ToString(),
        [EmulationControllerVisualControl.DirectionDown] = ControllerCommand.Down.ToString(),
        [EmulationControllerVisualControl.DirectionLeft] = ControllerCommand.Left.ToString(),
        [EmulationControllerVisualControl.DirectionRight] = ControllerCommand.Right.ToString(),
        [EmulationControllerVisualControl.FaceA] = ControllerCommand.A.ToString(),
        [EmulationControllerVisualControl.FaceB] = ControllerCommand.B.ToString(),
        [EmulationControllerVisualControl.Start] = ControllerCommand.Start.ToString(),
        [EmulationControllerVisualControl.Option] = ControllerCommand.Select.ToString()
    };
}
