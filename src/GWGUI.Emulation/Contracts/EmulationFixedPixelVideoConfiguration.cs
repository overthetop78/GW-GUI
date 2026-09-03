using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Contracts;

public sealed record EmulationFixedPixelVideoConfiguration(
    EmulationFixedPixelTechnology Technology = EmulationFixedPixelTechnology.Lcd,
    EmulationSubpixelLayout Subpixels = EmulationSubpixelLayout.Rgb,
    uint? MonochromeColorArgb = null,
    int GridIntensity = EmulationVideoProcessingDefaults.Intensity,
    int PixelGap = EmulationVideoProcessingDefaults.Intensity,
    int ResponseTimeMilliseconds = EmulationVideoProcessingDefaults.DurationMilliseconds,
    int PersistenceIntensity = EmulationVideoProcessingDefaults.Intensity,
    int? BacklightIntensity = null,
    int? BlackDepth = null);
