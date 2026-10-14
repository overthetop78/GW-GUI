namespace GWGUI.Emulation.Sinclair.Common.Contracts;

internal sealed record InputActionDefinition(ControllerCommand Command)
{
    internal string Id => Command.ToString();
    internal InputBindingDefinition Definition => new(Id, "Emulation.Controller.Action." + Id, string.Empty)
    {
        OppositeDirectionId = Command switch
        {
            ControllerCommand.Up => nameof(ControllerCommand.Down),
            ControllerCommand.Down => nameof(ControllerCommand.Up),
            ControllerCommand.Left => nameof(ControllerCommand.Right),
            ControllerCommand.Right => nameof(ControllerCommand.Left),
            _ => null
        }
    };
}
