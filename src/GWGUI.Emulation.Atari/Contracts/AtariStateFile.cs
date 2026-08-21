namespace GWGUI.Emulation.Atari;

internal sealed record AtariStateFile(AtariSavedStateHeader Header, byte[] State);
