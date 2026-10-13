namespace GWGUI.Emulation.Sharp.Common.Contracts;

internal sealed record InputActionDefinition(ControllerCommand Command, bool Analog = false)
{
    internal string Id => Command.ToString();
    internal InputBindingDefinition Definition => new(Id, Command is ControllerCommand.Up or ControllerCommand.Down or ControllerCommand.Left or ControllerCommand.Right ? "Emulation.Controller.Action." + Id : InputMappingConstants.ResourcePrefix + Id, string.Empty)
    {
        OppositeDirectionId = Analog ? Command switch
        {
            ControllerCommand.Up => ControllerCommand.Down.ToString(),
            ControllerCommand.Down => ControllerCommand.Up.ToString(),
            ControllerCommand.Left => ControllerCommand.Right.ToString(),
            ControllerCommand.Right => ControllerCommand.Left.ToString(),
            ControllerCommand.ThrottleIncrease => ControllerCommand.ThrottleDecrease.ToString(),
            ControllerCommand.ThrottleDecrease => ControllerCommand.ThrottleIncrease.ToString(),
            _ => null
        } : null
    };
}
