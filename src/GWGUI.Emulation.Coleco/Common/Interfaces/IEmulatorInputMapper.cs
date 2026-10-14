namespace GWGUI.Emulation.Coleco.Common.Interfaces;

internal interface IEmulatorInputMapper
{
    IReadOnlyList<PeripheralCategory> GetPeripherals();
    IReadOnlyList<InputBindingDefinition> DescribeInput(PeripheralCategory peripheral, int port);
    EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model);
}
