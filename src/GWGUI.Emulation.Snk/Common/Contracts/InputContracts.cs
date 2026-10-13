namespace GWGUI.Emulation.Snk.Common.Contracts;

internal sealed record InputActionDefinition(ControllerCommand Command)
{
    internal string Id => Command.ToString();
    internal InputBindingDefinition Definition => new(Id, ResourceKey, string.Empty);
    private string ResourceKey => Command is ControllerCommand.Up or ControllerCommand.Down or ControllerCommand.Left or ControllerCommand.Right ? "Emulation.Controller.Action." + Id : InputMappingConstants.ResourcePrefix + Id;
}
