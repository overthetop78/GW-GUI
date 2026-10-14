namespace GWGUI.Emulation.Coleco.Common.Interfaces;

internal interface IEmulatorInputMapper
{
    IReadOnlyList<PeripheralCategory> GetPeripherals();
    EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model);
}
