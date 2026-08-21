namespace GWGUI.Emulation.Atari;

internal sealed record AtariControllerPort(IReadOnlyList<AtariControllerDevice> Devices);
