using GWGUI.Emulation.Constants;

namespace GWGUI.Emulation.Contracts;

public sealed record EmulationImageAdjustments(
    int Brightness = EmulationVideoProcessingDefaults.Adjustment,
    int Contrast = EmulationVideoProcessingDefaults.Adjustment,
    int Gamma = EmulationVideoProcessingDefaults.Adjustment,
    int Saturation = EmulationVideoProcessingDefaults.Adjustment,
    int Sharpness = EmulationVideoProcessingDefaults.Adjustment);
