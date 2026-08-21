namespace GWGUI.Emulation.Atari;

internal sealed record AtariInputDescriptor(uint Port, uint Device, uint Index, uint Id, string Description);
