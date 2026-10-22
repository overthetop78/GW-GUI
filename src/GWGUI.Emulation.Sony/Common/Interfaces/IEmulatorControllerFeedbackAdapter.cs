namespace GWGUI.Emulation.Sony.Common.Interfaces;

internal interface IEmulatorControllerFeedbackAdapter
{
    IReadOnlyList<EmulationControllerOutputDefinition> GetControllerOutputs(MachineConfiguration configuration, PeripheralCategory type);
}
