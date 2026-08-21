namespace GWGUI.Emulation;

public interface IEmulationRequiredMediaMessageContext : IEmulationMessageContext
{
    IReadOnlyList<EmulationMediaCategory> RequiredMedia { get; }
}
