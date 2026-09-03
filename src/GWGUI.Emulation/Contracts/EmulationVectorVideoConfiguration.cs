using GWGUI.Emulation.Constants;

namespace GWGUI.Emulation.Contracts;

public sealed record EmulationVectorVideoConfiguration(
    int LineThreshold = EmulationVideoProcessingDefaults.Intensity,
    int LineIntensity = EmulationVideoProcessingDefaults.Intensity,
    int HaloIntensity = EmulationVideoProcessingDefaults.Intensity,
    int PersistenceIntensity = EmulationVideoProcessingDefaults.Intensity);
