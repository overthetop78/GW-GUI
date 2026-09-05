using GWGUI.VideoPresentation.Constants;
using GWGUI.VideoPresentation.Enums;

namespace GWGUI.VideoPresentation.Contracts;

public sealed record EmulationImageRestorationConfiguration(
    int Dedithering = EmulationVideoProcessingDefaults.Intensity,
    int Denoising = EmulationVideoProcessingDefaults.Intensity,
    int Debanding = EmulationVideoProcessingDefaults.Intensity,
    int DetailRecovery = EmulationVideoProcessingDefaults.Intensity,
    EmulationDeinterlacingMode Deinterlacing = EmulationDeinterlacingMode.Off);
