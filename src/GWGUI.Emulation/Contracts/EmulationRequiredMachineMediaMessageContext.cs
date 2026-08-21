namespace GWGUI.Emulation;

public sealed record EmulationRequiredMachineMediaMessageContext(
    string MachineId,
    IReadOnlyList<EmulationMediaCategory> RequiredMedia)
    : IEmulationMachineMessageContext, IEmulationRequiredMediaMessageContext;
