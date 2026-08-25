namespace GWGUI.Emulation.Atari.Contracts;

internal sealed record AtariInputDescriptor(uint Port, uint Device, uint Index, uint Id, string Description);
