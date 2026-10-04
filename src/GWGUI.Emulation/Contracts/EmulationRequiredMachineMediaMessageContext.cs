namespace GWGUI.Emulation.Contracts;

public sealed record EmulationRequiredMachineMediaMessageContext(
    string MachineId,
    IReadOnlyList<EmulationMediaCategory> RequiredMedia,
    IReadOnlyList<string>? RequiredMediaNames = null)
    : IEmulationMachineMessageContext, IEmulationRequiredMediaMessageContext;
