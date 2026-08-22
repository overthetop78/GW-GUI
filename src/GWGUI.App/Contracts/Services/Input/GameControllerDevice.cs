namespace GWGUI.App.Contracts.Services.Input;

internal sealed record GameControllerDevice(string Id, string Name)
{
    public override string ToString() => Name;
}
