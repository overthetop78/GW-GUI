namespace GWGUI.Emulation.Atari.Emulators.Common.Contracts;

internal sealed record ControllerDevice(string Description, uint Id);

internal sealed record ControllerPort(IReadOnlyList<ControllerDevice> Devices);

internal sealed record InputDescriptor(uint Port, uint Device, uint Index, uint Id, string Description);
