namespace GWGUI.Emulation.Sinclair.Common.Interfaces;

internal interface IEmulatorInputMapper
{
    EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model);
}
