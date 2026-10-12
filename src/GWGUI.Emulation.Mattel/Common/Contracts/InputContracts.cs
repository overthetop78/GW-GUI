namespace GWGUI.Emulation.Mattel.Common.Contracts;

internal sealed record InputActionDefinition(InputBindingDefinition Definition)
{
    internal string Id => Definition.Id;
}
