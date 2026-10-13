namespace GWGUI.Emulation.Sinclair.Common.Contracts;

internal sealed record InputActionDefinition(ControllerCommand Command)
{
    internal string Id => Command.ToString();
    internal InputBindingDefinition Definition => new(Id, "Emulation.Controller.Action." + Id, string.Empty);
}
