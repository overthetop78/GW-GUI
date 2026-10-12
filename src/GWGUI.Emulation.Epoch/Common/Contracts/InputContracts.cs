namespace GWGUI.Emulation.Epoch.Common.Contracts;

internal sealed record InputActionDefinition(InputBindingDefinition Definition, EmulationKey? Key = null)
{
    internal string Id => Definition.Id;
    internal string DefaultBinding => Definition.DefaultBinding;
}
