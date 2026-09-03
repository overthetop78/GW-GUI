using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Contracts;

public sealed record EmulationCrtVideoConfiguration(
    EmulationCrtColorMode ColorMode = EmulationCrtColorMode.Color,
    uint? CustomColorArgb = null,
    int BeamWidth = EmulationVideoProcessingDefaults.Intensity,
    int BeamIntensity = EmulationVideoProcessingDefaults.Intensity,
    int BeamDiffusion = EmulationVideoProcessingDefaults.Intensity,
    int HaloIntensity = EmulationVideoProcessingDefaults.Intensity,
    EmulationCrtMask Mask = EmulationCrtMask.None,
    EmulationSubpixelLayout MaskSubpixels = EmulationSubpixelLayout.Rgb,
    int MaskIntensity = EmulationVideoProcessingDefaults.Intensity,
    int Curvature = EmulationVideoProcessingDefaults.Intensity,
    int Vignette = EmulationVideoProcessingDefaults.Intensity,
    bool ScanlinesEnabled = false,
    EmulationPatternOrientation ScanlineOrientation = EmulationPatternOrientation.Horizontal,
    int ScanlineIntensity = EmulationVideoProcessingDefaults.Intensity,
    int ScanlineThickness = EmulationVideoProcessingDefaults.Intensity,
    int ScanlinePhase = EmulationVideoProcessingDefaults.Intensity,
    int ScanlineCompensation = EmulationVideoProcessingDefaults.Intensity,
    bool PatternEnabled = false,
    EmulationPatternOrientation PatternOrientation = EmulationPatternOrientation.Horizontal,
    int PatternFrequency = EmulationVideoProcessingDefaults.Intensity,
    int PatternPhase = EmulationVideoProcessingDefaults.Intensity,
    int PatternIntensity = EmulationVideoProcessingDefaults.Intensity);
