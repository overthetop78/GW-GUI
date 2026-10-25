namespace GWGUI.Emulation.Snk.Common.Interfaces;

internal interface IEmulatorInputMapper
{
    EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration);
}
