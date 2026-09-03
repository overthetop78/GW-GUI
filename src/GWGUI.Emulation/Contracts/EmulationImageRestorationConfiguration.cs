using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Contracts;

public sealed record EmulationImageRestorationConfiguration(
    int Dedithering = EmulationVideoProcessingDefaults.Intensity,
    int Denoising = EmulationVideoProcessingDefaults.Intensity,
    int Debanding = EmulationVideoProcessingDefaults.Intensity,
    int DetailRecovery = EmulationVideoProcessingDefaults.Intensity,
    EmulationDeinterlacingMode Deinterlacing = EmulationDeinterlacingMode.Off);
