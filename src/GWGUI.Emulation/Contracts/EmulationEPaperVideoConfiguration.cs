using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Contracts;

public sealed record EmulationEPaperVideoConfiguration(
    EmulationEPaperColorMode ColorMode = EmulationEPaperColorMode.Monochrome,
    int Contrast = 70,
    int Dithering = 35,
    int RefreshTimeMilliseconds = 500,
    int Ghosting = 20,
    int InkDensity = 90,
    int PaperBrightness = 90,
    int PaperWarmth = 35,
    int ColorSaturation = 55,
    int SurfaceTexture = 10,
    int EdgeSoftness = 10);
