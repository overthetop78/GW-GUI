using GWGUI.VideoPresentation.Constants;
using GWGUI.VideoPresentation.Enums;

namespace GWGUI.VideoPresentation.Contracts;

public sealed record EmulationVectorVideoConfiguration(
    int LineThreshold = EmulationVideoProcessingDefaults.Intensity,
    int LineIntensity = EmulationVideoProcessingDefaults.Intensity,
    int BeamWidth = EmulationVideoProcessingDefaults.Intensity,
    int BeamFocus = EmulationVideoProcessingLimits.IntensityMaximum,
    EmulationCrtColorMode PhosphorColor = EmulationCrtColorMode.Color,
    int HaloIntensity = EmulationVideoProcessingDefaults.Intensity,
    int HaloRadius = EmulationVideoProcessingDefaults.Intensity,
    int PersistenceIntensity = EmulationVideoProcessingDefaults.Intensity);
