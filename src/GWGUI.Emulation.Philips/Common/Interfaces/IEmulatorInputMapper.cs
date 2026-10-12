namespace GWGUI.Emulation.Philips.Common.Interfaces;

internal interface IEmulatorInputMapper
{
    IReadOnlyList<PeripheralCategory> GetPeripherals(MachineModel model);
    IReadOnlyList<InputActionDefinition> DescribeInput(MachineModel model, PeripheralCategory peripheral);
    EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model);
}
