using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Contracts;

public sealed record EmulationVectorVideoConfiguration(
    int LineThreshold = EmulationVideoProcessingDefaults.Intensity,
    int LineIntensity = EmulationVideoProcessingDefaults.Intensity,
    int BeamWidth = EmulationVideoProcessingDefaults.Intensity,
    int BeamFocus = EmulationVideoProcessingLimits.IntensityMaximum,
    EmulationCrtColorMode PhosphorColor = EmulationCrtColorMode.Color,
    int HaloIntensity = EmulationVideoProcessingDefaults.Intensity,
    int HaloRadius = EmulationVideoProcessingDefaults.Intensity,
    int PersistenceIntensity = EmulationVideoProcessingDefaults.Intensity);
