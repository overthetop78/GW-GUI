using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Contracts;

public sealed record EmulationEPaperVideoConfiguration(
    EmulationEPaperColorMode ColorMode = EmulationEPaperColorMode.Monochrome,
    int Contrast = 70,
    int Dithering = 35,
    int RefreshTimeMilliseconds = 500,
    int Ghosting = 20);
