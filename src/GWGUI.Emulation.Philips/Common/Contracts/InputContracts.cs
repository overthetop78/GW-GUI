namespace GWGUI.Emulation.Philips.Common.Contracts;

internal sealed record InputActionDefinition(InputBindingDefinition Definition)
{
    internal string Id => Definition.Id;
}
