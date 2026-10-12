namespace GWGUI.Emulation.Sharp.Common.Interfaces;

internal interface IEmulatorInputMapper
{
    IReadOnlyList<PeripheralCategory> GetPeripherals(MachineModel model);
    EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model);
}
