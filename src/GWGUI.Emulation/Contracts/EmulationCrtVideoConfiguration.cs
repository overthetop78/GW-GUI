using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Contracts;

public sealed record EmulationCrtVideoConfiguration(
    EmulationCrtColorMode ColorMode = EmulationCrtColorMode.Color,
    int BeamWidth = EmulationVideoProcessingDefaults.Intensity,
    int BeamIntensity = EmulationVideoProcessingDefaults.Intensity,
    int BeamDiffusion = EmulationVideoProcessingDefaults.Intensity,
    int HaloIntensity = EmulationVideoProcessingDefaults.Intensity,
    EmulationCrtMask Mask = EmulationCrtMask.None,
    EmulationSubpixelLayout MaskSubpixels = EmulationSubpixelLayout.Rgb,
    int MaskIntensity = EmulationVideoProcessingDefaults.Intensity,
    int HorizontalCurvature = 0,
    int VerticalCurvature = 0,
    int Trapezoid = 0,
    int Vignette = EmulationVideoProcessingDefaults.Intensity,
    bool ScanlinesEnabled = false,
    EmulationPatternOrientation ScanlineOrientation = EmulationPatternOrientation.Horizontal,
    int ScanlineIntensity = EmulationVideoProcessingDefaults.Intensity,
    int ScanlineThickness = EmulationVideoProcessingDefaults.Intensity,
    EmulationScanlinePhase ScanlinePhase = EmulationScanlinePhase.Zero,
    int ScanlineCompensation = EmulationVideoProcessingDefaults.Intensity,
    bool PatternEnabled = false,
    EmulationPatternOrientation PatternOrientation = EmulationPatternOrientation.Horizontal,
    int PatternFrequency = EmulationVideoProcessingDefaults.Intensity,
    int PatternPhase = EmulationVideoProcessingDefaults.Intensity,
    int PatternIntensity = EmulationVideoProcessingDefaults.Intensity);
