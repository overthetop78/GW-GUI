namespace GWGUI.Emulation;

public interface IEmulationConfiguration
{
    string ModuleId { get; }
    Guid Id { get; }
    string MachineId { get; }
    EmulationVideoRenderer VideoRenderer { get; }
}
