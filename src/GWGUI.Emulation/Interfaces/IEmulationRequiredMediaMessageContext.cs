namespace GWGUI.Emulation.Interfaces;

public interface IEmulationRequiredMediaMessageContext : IEmulationMessageContext
{
    IReadOnlyList<EmulationMediaCategory> RequiredMedia { get; }
}
