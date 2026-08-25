namespace GWGUI.Emulation.Atari.Contracts;

internal sealed record AtariControllerPort(IReadOnlyList<AtariControllerDevice> Devices);
