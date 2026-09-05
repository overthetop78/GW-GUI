using GWGUI.VideoPresentation.Constants;
using GWGUI.VideoPresentation.Enums;

namespace GWGUI.VideoPresentation.Contracts;

public sealed record EmulationFixedPixelVideoConfiguration(
    EmulationFixedPixelTechnology Technology = EmulationFixedPixelTechnology.Lcd,
    EmulationSubpixelLayout Subpixels = EmulationSubpixelLayout.Rgb,
    uint? MonochromeColorArgb = null,
    int GridIntensity = EmulationVideoProcessingDefaults.Intensity,
    int PixelGap = EmulationVideoProcessingDefaults.Intensity,
    int ResponseTimeMilliseconds = EmulationVideoProcessingDefaults.DurationMilliseconds,
    int PersistenceIntensity = EmulationVideoProcessingDefaults.Intensity,
    int? BacklightIntensity = null,
    int? BlackDepth = null,
    EmulationMonochromePalette MonochromePalette = EmulationMonochromePalette.Green,
    int BacklightBleedIntensity = 25);
