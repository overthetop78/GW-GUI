namespace GWGUI.Emulation.Fairchild.Common.Interfaces;

internal interface IEmulatorInputMapper
{
    IReadOnlyList<InputActionDefinition> DescribeInput(MachineModel model, int port);
    EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model);
}
