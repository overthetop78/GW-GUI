namespace GWGUI.Emulation;

public interface IEmulationMachineMessageContext : IEmulationMessageContext
{
    string MachineId { get; }
}
