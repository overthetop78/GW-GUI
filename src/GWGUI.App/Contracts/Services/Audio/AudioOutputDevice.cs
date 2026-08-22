namespace GWGUI.App.Contracts.Services.Audio;

public sealed record AudioOutputDevice(string Id, string Name)
{
    public override string ToString() => Name;
}
