namespace GWGUI.Emulation.Atari.Contracts;

internal sealed record AtariStateFile(AtariSavedStateHeader Header, byte[] State);
