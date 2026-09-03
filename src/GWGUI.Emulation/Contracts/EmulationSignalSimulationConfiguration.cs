namespace GWGUI.Emulation.Contracts;

public sealed record EmulationSignalSimulationConfiguration(
    int Composite = 0,
    int SVideo = 0,
    int Rf = 0,
    int Pal = 0,
    int Ntsc = 0);
