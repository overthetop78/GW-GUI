using GWGUI.Emulation.Constants;

namespace GWGUI.Emulation.Contracts;

public sealed record EmulationPlasmaVideoConfiguration(
    int CellStructure = EmulationVideoProcessingDefaults.Intensity,
    int Diffusion = EmulationVideoProcessingDefaults.Intensity,
    int TemporalDithering = EmulationVideoProcessingDefaults.Intensity,
    int PersistenceIntensity = EmulationVideoProcessingDefaults.Intensity,
    int BlackDepth = EmulationVideoProcessingDefaults.Intensity,
    int PhosphorIntensity = EmulationVideoProcessingDefaults.Intensity,
    int GammaResponse = EmulationVideoProcessingDefaults.Intensity,
    int AutomaticBrightnessLimiter = EmulationVideoProcessingDefaults.Intensity);
