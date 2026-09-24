using GWGUI.VideoPresentation.Contracts;
using GWGUI.VideoPresentation.Enums;

namespace GWGUI.VideoPresentation.Dictionaries;

public static partial class EmulationVideoProcessingCatalog
{
    public static IReadOnlyDictionary<EmulationVideoRenderer, string> RendererResourceKeys { get; } =
        new Dictionary<EmulationVideoRenderer, string>
        {
            [EmulationVideoRenderer.Direct3D11] = "Emulation.Video.Renderer.Direct3D11",
            [EmulationVideoRenderer.Vulkan] = "Emulation.Video.Renderer.Vulkan",
            [EmulationVideoRenderer.OpenGL] = "Emulation.Video.Renderer.OpenGL",
            [EmulationVideoRenderer.Wpf] = "Emulation.Video.Renderer.Wpf"
        };
    public const string Brightness = nameof(Brightness);
    public const string Contrast = nameof(Contrast);
    public const string Gamma = nameof(Gamma);
    public const string Saturation = nameof(Saturation);
    public const string Sharpness = nameof(Sharpness);
    public const string Dedithering = nameof(Dedithering);
    public const string Denoising = nameof(Denoising);
    public const string Debanding = nameof(Debanding);
    public const string DetailRecovery = nameof(DetailRecovery);
    public const string Deinterlacing = nameof(Deinterlacing);
    public const string GeneralPersistence = nameof(GeneralPersistence);
    public const string MotionBlur = nameof(MotionBlur);
    public const string Flicker = nameof(Flicker);
    public const string Interlacing = nameof(Interlacing);
    public const string InterlacingVisibility = nameof(InterlacingVisibility);
    public const string BlackFrameInsertion = nameof(BlackFrameInsertion);
    public const string SignalConnection = nameof(SignalConnection);
    public const string SignalConnectionIntensity = nameof(SignalConnectionIntensity);
    public const string SignalStandard = nameof(SignalStandard);
    public const string SignalStandardIntensity = nameof(SignalStandardIntensity);
    public const string Grain = nameof(Grain);
    public const string Vhs = nameof(Vhs);
    public const string ChromaticAberration = nameof(ChromaticAberration);
    public const string Bloom = nameof(Bloom);
    public const string Sepia = nameof(Sepia);
    public const string CrtColorMode = nameof(CrtColorMode);
    public const string CrtBeamWidth = nameof(CrtBeamWidth);
    public const string CrtBeamIntensity = nameof(CrtBeamIntensity);
    public const string CrtBeamDiffusion = nameof(CrtBeamDiffusion);
    public const string CrtHaloIntensity = nameof(CrtHaloIntensity);
    public const string CrtMask = nameof(CrtMask);
    public const string CrtMaskSubpixels = nameof(CrtMaskSubpixels);
    public const string CrtMaskIntensity = nameof(CrtMaskIntensity);
    public const string CrtHorizontalCurvature = nameof(CrtHorizontalCurvature);
    public const string CrtVerticalCurvature = nameof(CrtVerticalCurvature);
    public const string CrtTrapezoid = nameof(CrtTrapezoid);
    public const string CrtVignette = nameof(CrtVignette);
    public const string CrtScanlinesEnabled = nameof(CrtScanlinesEnabled);
    public const string CrtScanlineOrientation = nameof(CrtScanlineOrientation);
    public const string CrtScanlineIntensity = nameof(CrtScanlineIntensity);
    public const string CrtScanlineThickness = nameof(CrtScanlineThickness);
    public const string CrtScanlinePhase = nameof(CrtScanlinePhase);
    public const string CrtScanlineCompensation = nameof(CrtScanlineCompensation);
    public const string CrtPatternEnabled = nameof(CrtPatternEnabled);
    public const string CrtPatternOrientation = nameof(CrtPatternOrientation);
    public const string CrtPatternFrequency = nameof(CrtPatternFrequency);
    public const string CrtPatternPhase = nameof(CrtPatternPhase);
    public const string CrtPatternIntensity = nameof(CrtPatternIntensity);
    public const string FixedPixelTechnology = nameof(FixedPixelTechnology);
    public const string FixedPixelSubpixels = nameof(FixedPixelSubpixels);
    public const string FixedPixelMonochromeColor = nameof(FixedPixelMonochromeColor);
    public const string FixedPixelGridIntensity = nameof(FixedPixelGridIntensity);
    public const string FixedPixelPixelGap = nameof(FixedPixelPixelGap);
    public const string FixedPixelResponseTime = nameof(FixedPixelResponseTime);
    public const string FixedPixelPersistence = nameof(FixedPixelPersistence);
    public const string FixedPixelBacklight = nameof(FixedPixelBacklight);
    public const string FixedPixelBacklightBleed = nameof(FixedPixelBacklightBleed);
    public const string FixedPixelBlackDepth = nameof(FixedPixelBlackDepth);
    public const string PlasmaCellStructure = nameof(PlasmaCellStructure);
    public const string PlasmaDiffusion = nameof(PlasmaDiffusion);
    public const string PlasmaTemporalDithering = nameof(PlasmaTemporalDithering);
    public const string PlasmaPersistence = nameof(PlasmaPersistence);
    public const string PlasmaBlackDepth = nameof(PlasmaBlackDepth);
    public const string PlasmaPhosphorIntensity = nameof(PlasmaPhosphorIntensity);
    public const string PlasmaGammaResponse = nameof(PlasmaGammaResponse);
    public const string PlasmaAutomaticBrightnessLimiter = nameof(PlasmaAutomaticBrightnessLimiter);
    public const string VectorLineThreshold = nameof(VectorLineThreshold);
    public const string VectorLineIntensity = nameof(VectorLineIntensity);
    public const string VectorBeamWidth = nameof(VectorBeamWidth);
    public const string VectorBeamFocus = nameof(VectorBeamFocus);
    public const string VectorPhosphorColor = nameof(VectorPhosphorColor);
    public const string VectorHaloIntensity = nameof(VectorHaloIntensity);
    public const string VectorHaloRadius = nameof(VectorHaloRadius);
    public const string VectorPersistence = nameof(VectorPersistence);
    public const string VfdColor = nameof(VfdColor);
    public const string VfdPhosphorIntensity = nameof(VfdPhosphorIntensity);
    public const string VfdEmissionThreshold = nameof(VfdEmissionThreshold);
    public const string VfdGlassDarkening = nameof(VfdGlassDarkening);
    public const string VfdStructure = nameof(VfdStructure);
    public const string VfdCellSize = nameof(VfdCellSize);
    public const string VfdCellGap = nameof(VfdCellGap);
    public const string VfdHaloIntensity = nameof(VfdHaloIntensity);
    public const string VfdHaloRadius = nameof(VfdHaloRadius);
    public const string VfdPersistence = nameof(VfdPersistence);
    public const string LedMatrixColor = nameof(LedMatrixColor);
    public const string LedMatrixCellSize = nameof(LedMatrixCellSize);
    public const string LedMatrixCellGap = nameof(LedMatrixCellGap);
    public const string LedMatrixDiffusion = nameof(LedMatrixDiffusion);
    public const string LedMatrixBrightness = nameof(LedMatrixBrightness);
    public const string LedMatrixShape = nameof(LedMatrixShape);
    public const string LedMatrixHaloRadius = nameof(LedMatrixHaloRadius);
    public const string LedMatrixBlackDepth = nameof(LedMatrixBlackDepth);
    public const string DotMatrixPalette = nameof(DotMatrixPalette);
    public const string DotMatrixShape = nameof(DotMatrixShape);
    public const string DotMatrixDotSize = nameof(DotMatrixDotSize);
    public const string DotMatrixCellSize = nameof(DotMatrixCellSize);
    public const string DotMatrixCellGap = nameof(DotMatrixCellGap);
    public const string DotMatrixContrast = nameof(DotMatrixContrast);
    public const string DotMatrixBrightness = nameof(DotMatrixBrightness);
    public const string DotMatrixHaloIntensity = nameof(DotMatrixHaloIntensity);
    public const string DotMatrixResponseTime = nameof(DotMatrixResponseTime);
    public const string DotMatrixPersistence = nameof(DotMatrixPersistence);
    public const string SegmentDisplayLayout = nameof(SegmentDisplayLayout);
    public const string SegmentDisplayColor = nameof(SegmentDisplayColor);
    public const string SegmentDisplayCellSize = nameof(SegmentDisplayCellSize);
    public const string SegmentDisplayHorizontalGap = nameof(SegmentDisplayHorizontalGap);
    public const string SegmentDisplayVerticalGap = nameof(SegmentDisplayVerticalGap);
    public const string SegmentDisplayThickness = nameof(SegmentDisplayThickness);
    public const string SegmentDisplaySegmentGap = nameof(SegmentDisplaySegmentGap);
    public const string SegmentDisplayEndShape = nameof(SegmentDisplayEndShape);
    public const string SegmentDisplayDecimalPoint = nameof(SegmentDisplayDecimalPoint);
    public const string SegmentDisplayColon = nameof(SegmentDisplayColon);
    public const string SegmentDisplayBrightness = nameof(SegmentDisplayBrightness);
    public const string SegmentDisplayActivationThreshold = nameof(SegmentDisplayActivationThreshold);
    public const string SegmentDisplayContrast = nameof(SegmentDisplayContrast);
    public const string SegmentDisplayOffSegmentVisibility = nameof(SegmentDisplayOffSegmentVisibility);
    public const string SegmentDisplayBlackDepth = nameof(SegmentDisplayBlackDepth);
    public const string SegmentDisplayGlow = nameof(SegmentDisplayGlow);
    public const string SegmentDisplayHaloRadius = nameof(SegmentDisplayHaloRadius);
    public const string SegmentDisplayResponseTime = nameof(SegmentDisplayResponseTime);
    public const string SegmentDisplayPersistence = nameof(SegmentDisplayPersistence);
    public const string EPaperColorMode = nameof(EPaperColorMode);
    public const string EPaperContrast = nameof(EPaperContrast);
    public const string EPaperDithering = nameof(EPaperDithering);
    public const string EPaperRefreshTime = nameof(EPaperRefreshTime);
    public const string EPaperGhosting = nameof(EPaperGhosting);
    public const string EPaperInkDensity = nameof(EPaperInkDensity);
    public const string EPaperPaperBrightness = nameof(EPaperPaperBrightness);
    public const string EPaperPaperWarmth = nameof(EPaperPaperWarmth);
    public const string EPaperColorSaturation = nameof(EPaperColorSaturation);
    public const string EPaperSurfaceTexture = nameof(EPaperSurfaceTexture);
    public const string EPaperEdgeSoftness = nameof(EPaperEdgeSoftness);
    public const string ProjectionOpticalBlur = nameof(ProjectionOpticalBlur);
    public const string ProjectionDiffusion = nameof(ProjectionDiffusion);
    public const string ProjectionScreenTexture = nameof(ProjectionScreenTexture);
    public const string ProjectionConvergence = nameof(ProjectionConvergence);
    public const string ProjectionLightOutput = nameof(ProjectionLightOutput);
    public const string ProjectionAmbientLight = nameof(ProjectionAmbientLight);
    public const string ProjectionVignette = nameof(ProjectionVignette);

    public const string ExclusiveDisplayTechnology = nameof(ExclusiveDisplayTechnology);
    public const string UnsupportedBackend = nameof(UnsupportedBackend);

    private static readonly string[] ParameterIds =
    [
        Brightness, Contrast, Gamma, Saturation, Sharpness, Dedithering, Denoising, Debanding,
        DetailRecovery,
        Deinterlacing, GeneralPersistence, MotionBlur, Flicker, Interlacing,
        InterlacingVisibility, BlackFrameInsertion,
        SignalConnection, SignalConnectionIntensity, SignalStandard, SignalStandardIntensity,
        Grain,
        Vhs,
        ChromaticAberration,
        Bloom,
        Sepia,
        CrtColorMode, CrtBeamWidth, CrtBeamIntensity, CrtBeamDiffusion,
        CrtHaloIntensity, CrtMask, CrtMaskSubpixels, CrtMaskIntensity,
        CrtHorizontalCurvature, CrtVerticalCurvature, CrtTrapezoid, CrtVignette,
        CrtScanlinesEnabled, CrtScanlineOrientation, CrtScanlineIntensity, CrtScanlineThickness,
        CrtScanlinePhase, CrtScanlineCompensation, CrtPatternEnabled, CrtPatternOrientation,
        CrtPatternFrequency, CrtPatternPhase, CrtPatternIntensity,
        FixedPixelTechnology, FixedPixelSubpixels, FixedPixelMonochromeColor,
        FixedPixelGridIntensity, FixedPixelPixelGap, FixedPixelResponseTime,
        FixedPixelPersistence, FixedPixelBacklight, FixedPixelBacklightBleed, FixedPixelBlackDepth,
        PlasmaCellStructure, PlasmaDiffusion, PlasmaTemporalDithering, PlasmaPersistence,
        PlasmaBlackDepth, PlasmaPhosphorIntensity, PlasmaGammaResponse,
        PlasmaAutomaticBrightnessLimiter,
        VectorLineThreshold, VectorLineIntensity, VectorBeamWidth, VectorBeamFocus,
        VectorPhosphorColor, VectorHaloIntensity, VectorHaloRadius, VectorPersistence,
        VfdColor, VfdPhosphorIntensity, VfdEmissionThreshold, VfdGlassDarkening,
        VfdStructure, VfdCellSize, VfdCellGap, VfdHaloIntensity, VfdHaloRadius,
        VfdPersistence,
        LedMatrixColor, LedMatrixCellSize, LedMatrixCellGap, LedMatrixDiffusion,
        LedMatrixBrightness, LedMatrixShape, LedMatrixHaloRadius, LedMatrixBlackDepth,
        DotMatrixPalette, DotMatrixShape, DotMatrixCellSize, DotMatrixDotSize,
        DotMatrixCellGap, DotMatrixContrast, DotMatrixBrightness, DotMatrixHaloIntensity,
        DotMatrixResponseTime, DotMatrixPersistence, SegmentDisplayLayout, SegmentDisplayColor,
        SegmentDisplayCellSize, SegmentDisplayHorizontalGap, SegmentDisplayVerticalGap,
        SegmentDisplayThickness, SegmentDisplaySegmentGap, SegmentDisplayEndShape,
        SegmentDisplayDecimalPoint, SegmentDisplayColon, SegmentDisplayBrightness,
        SegmentDisplayActivationThreshold, SegmentDisplayContrast,
        SegmentDisplayOffSegmentVisibility, SegmentDisplayBlackDepth, SegmentDisplayGlow,
        SegmentDisplayHaloRadius, SegmentDisplayResponseTime, SegmentDisplayPersistence,
        EPaperColorMode, EPaperContrast, EPaperDithering,
        EPaperRefreshTime, EPaperGhosting, EPaperInkDensity, EPaperPaperBrightness,
        EPaperPaperWarmth, EPaperColorSaturation, EPaperSurfaceTexture,
        EPaperEdgeSoftness, ProjectionOpticalBlur, ProjectionDiffusion,
        ProjectionScreenTexture, ProjectionConvergence, ProjectionLightOutput,
        ProjectionAmbientLight, ProjectionVignette
    ];

}
