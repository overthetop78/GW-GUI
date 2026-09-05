using GWGUI.VideoPresentation.Contracts;
using GWGUI.VideoPresentation.Enums;

namespace GWGUI.VideoPresentation.Dictionaries;

public static class EmulationVideoProcessingCatalog
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

    public static IReadOnlyDictionary<EmulationVideoDisplayTechnology, string> DisplayTechnologyResourceKeys { get; }
        = ResourceKeys("Technology", Enum.GetValues<EmulationVideoDisplayTechnology>());

    public static IReadOnlyDictionary<EmulationVideoSampling, string> SamplingResourceKeys { get; }
        = ResourceKeys("Sampling",
        [
            EmulationVideoSampling.Nearest,
            EmulationVideoSampling.Bilinear,
            EmulationVideoSampling.SharpBilinear,
            EmulationVideoSampling.Bicubic,
            EmulationVideoSampling.Jinc2,
            EmulationVideoSampling.Lanczos,
            EmulationVideoSampling.Xbr,
            EmulationVideoSampling.Xbrz,
            EmulationVideoSampling.Hq2x,
            EmulationVideoSampling.Hq3x,
            EmulationVideoSampling.Hq4x,
            EmulationVideoSampling.TwoXSai,
            EmulationVideoSampling.SuperTwoXSai,
            EmulationVideoSampling.SuperEagle,
            EmulationVideoSampling.EpxScale2x,
            EmulationVideoSampling.ScaleFx,
            EmulationVideoSampling.ScaleNx,
            EmulationVideoSampling.Sabr
        ]);

    public static IReadOnlyDictionary<EmulationDeinterlacingMode, string> DeinterlacingResourceKeys { get; }
        = ResourceKeys("Deinterlacing", Enum.GetValues<EmulationDeinterlacingMode>());

    public static IReadOnlyDictionary<EmulationCrtColorMode, string> CrtColorModeResourceKeys { get; }
        = ResourceKeys("Crt.Color", Enum.GetValues<EmulationCrtColorMode>());

    public static IReadOnlyDictionary<EmulationCrtMask, string> CrtMaskResourceKeys { get; }
        = ResourceKeys("Crt.Mask", Enum.GetValues<EmulationCrtMask>());

    public static IReadOnlyDictionary<EmulationPatternOrientation, string> PatternOrientationResourceKeys { get; }
        = ResourceKeys("Orientation", Enum.GetValues<EmulationPatternOrientation>());

    public static IReadOnlyDictionary<EmulationScanlinePhase, string> ScanlinePhaseResourceKeys { get; }
        = ResourceKeys("Crt.ScanlinePhase", Enum.GetValues<EmulationScanlinePhase>());

    public static IReadOnlyDictionary<EmulationSubpixelLayout, string> SubpixelLayoutResourceKeys { get; }
        = ResourceKeys("Subpixels", Enum.GetValues<EmulationSubpixelLayout>());

    public static IReadOnlyDictionary<EmulationFixedPixelTechnology, string> FixedPixelTechnologyResourceKeys { get; }
        = ResourceKeys("FixedPixel.Technology", Enum.GetValues<EmulationFixedPixelTechnology>());

    public static IReadOnlyDictionary<EmulationMonochromePalette, string> MonochromePaletteResourceKeys { get; }
        = new Dictionary<EmulationMonochromePalette, string>
        {
            [EmulationMonochromePalette.Green] = "Emulation.Video.DotMatrix.Palette.Green",
            [EmulationMonochromePalette.Gray] = "Emulation.Video.DotMatrix.Palette.Gray",
            [EmulationMonochromePalette.Amber] = "Emulation.Video.DotMatrix.Palette.Amber",
            [EmulationMonochromePalette.Blue] = "Emulation.Video.DotMatrix.Palette.Blue",
            [EmulationMonochromePalette.White] = "Emulation.Video.SegmentDisplay.Color.White"
        };

    public static IReadOnlyDictionary<EmulationVideoPreset, string> PresetResourceKeys { get; }
        = ResourceKeys("Preset", Enum.GetValues<EmulationVideoPreset>());

    public static IReadOnlyDictionary<EmulationSignalConnection, string> SignalConnectionResourceKeys { get; }
        = ResourceKeys("Signal.Connection", Enum.GetValues<EmulationSignalConnection>());

    public static IReadOnlyDictionary<EmulationSignalStandard, string> SignalStandardResourceKeys { get; }
        = ResourceKeys("Signal.Standard", Enum.GetValues<EmulationSignalStandard>());

    public static IReadOnlyDictionary<EmulationVfdColor, string> VfdColorResourceKeys { get; }
        = ResourceKeys("Vfd.Color", Enum.GetValues<EmulationVfdColor>());

    public static IReadOnlyDictionary<EmulationVfdStructure, string> VfdStructureResourceKeys { get; }
        = ResourceKeys("Vfd.Structure", Enum.GetValues<EmulationVfdStructure>());

    public static IReadOnlyDictionary<EmulationLedMatrixColor, string> LedMatrixColorResourceKeys { get; }
        = ResourceKeys("LedMatrix.Color", Enum.GetValues<EmulationLedMatrixColor>());

    public static IReadOnlyDictionary<EmulationLedMatrixShape, string> LedMatrixShapeResourceKeys { get; }
        = ResourceKeys("LedMatrix.Shape", Enum.GetValues<EmulationLedMatrixShape>());

    public static IReadOnlyDictionary<EmulationDotMatrixPalette, string> DotMatrixPaletteResourceKeys { get; }
        = ResourceKeys("DotMatrix.Palette", Enum.GetValues<EmulationDotMatrixPalette>());

    public static IReadOnlyDictionary<EmulationDotMatrixShape, string> DotMatrixShapeResourceKeys { get; }
        = ResourceKeys("DotMatrix.Shape", Enum.GetValues<EmulationDotMatrixShape>());

    public static IReadOnlyDictionary<EmulationSegmentDisplayLayout, string> SegmentDisplayLayoutResourceKeys { get; }
        = ResourceKeys("SegmentDisplay.Layout", Enum.GetValues<EmulationSegmentDisplayLayout>());

    public static IReadOnlyDictionary<EmulationSegmentDisplayColor, string> SegmentDisplayColorResourceKeys { get; }
        = ResourceKeys("SegmentDisplay.Color", Enum.GetValues<EmulationSegmentDisplayColor>());

    public static IReadOnlyDictionary<EmulationSegmentEndShape, string> SegmentDisplayEndShapeResourceKeys { get; }
        = ResourceKeys("SegmentDisplay.EndShape", Enum.GetValues<EmulationSegmentEndShape>());

    public static IReadOnlyDictionary<EmulationEPaperColorMode, string> EPaperColorModeResourceKeys { get; }
        = ResourceKeys("EPaper.ColorMode", Enum.GetValues<EmulationEPaperColorMode>());

    public static IReadOnlyDictionary<string, string> ParameterResourceKeys { get; } =
        ParameterIds.ToDictionary(id => id, id => $"Emulation.Video.Parameter.{id}", StringComparer.Ordinal);

    public static IReadOnlyDictionary<string, object?> NeutralValues { get; } =
        new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            [Brightness] = 0, [Contrast] = 0, [Gamma] = 0, [Saturation] = 0, [Sharpness] = 0,
            [Dedithering] = 0, [Denoising] = 0, [Debanding] = 0, [DetailRecovery] = 0,
            [Deinterlacing] = EmulationDeinterlacingMode.Off,
            [GeneralPersistence] = 0,
            [MotionBlur] = 0,
            [Flicker] = 0,
            [Interlacing] = 0,
            [InterlacingVisibility] = 50,
            [BlackFrameInsertion] = false,
            [SignalConnection] = EmulationSignalConnection.None,
            [SignalConnectionIntensity] = 0,
            [SignalStandard] = EmulationSignalStandard.Automatic,
            [SignalStandardIntensity] = 0,
            [Grain] = 0,
            [Vhs] = 0,
            [ChromaticAberration] = 0,
            [Bloom] = 0,
            [Sepia] = false,
            [CrtColorMode] = EmulationCrtColorMode.Color,
            [CrtBeamWidth] = 0, [CrtBeamIntensity] = 0, [CrtBeamDiffusion] = 0,
            [CrtHaloIntensity] = 0, [CrtMask] = EmulationCrtMask.None,
            [CrtMaskSubpixels] = EmulationSubpixelLayout.Rgb, [CrtMaskIntensity] = 0,
            [CrtHorizontalCurvature] = 0, [CrtVerticalCurvature] = 0, [CrtTrapezoid] = 0,
            [CrtVignette] = 0, [CrtScanlinesEnabled] = false,
            [CrtScanlineOrientation] = EmulationPatternOrientation.Horizontal,
            [CrtScanlineIntensity] = 0, [CrtScanlineThickness] = 0,
            [CrtScanlinePhase] = EmulationScanlinePhase.Zero,
            [CrtScanlineCompensation] = 0, [CrtPatternEnabled] = false,
            [CrtPatternOrientation] = EmulationPatternOrientation.Horizontal,
            [CrtPatternFrequency] = 0, [CrtPatternPhase] = 0, [CrtPatternIntensity] = 0,
            [FixedPixelTechnology] = EmulationFixedPixelTechnology.Lcd,
            [FixedPixelSubpixels] = EmulationSubpixelLayout.Rgb, [FixedPixelMonochromeColor] = EmulationMonochromePalette.Green,
            [FixedPixelGridIntensity] = 0, [FixedPixelPixelGap] = 0, [FixedPixelResponseTime] = 0,
            [FixedPixelPersistence] = 0, [FixedPixelBacklight] = null, [FixedPixelBacklightBleed] = 25, [FixedPixelBlackDepth] = null,
            [PlasmaCellStructure] = 0, [PlasmaDiffusion] = 0, [PlasmaTemporalDithering] = 0,
            [PlasmaPersistence] = 0, [VectorLineThreshold] = 0, [VectorLineIntensity] = 0,
            [VectorBeamWidth] = 0, [VectorBeamFocus] = 100,
            [VectorPhosphorColor] = EmulationCrtColorMode.Color,
            [VectorHaloIntensity] = 0, [VectorHaloRadius] = 0, [VectorPersistence] = 0,
            [VfdColor] = EmulationVfdColor.Blue, [VfdPhosphorIntensity] = 70,
            [VfdEmissionThreshold] = 28, [VfdGlassDarkening] = 75,
            [VfdStructure] = EmulationVfdStructure.Graphic,
            [VfdCellSize] = 70, [VfdCellGap] = 20,
            [VfdHaloIntensity] = 25, [VfdHaloRadius] = 25, [VfdPersistence] = 20,
            [LedMatrixColor] = EmulationLedMatrixColor.Rgb, [LedMatrixCellSize] = 35,
            [LedMatrixCellGap] = 30, [LedMatrixDiffusion] = 20, [LedMatrixBrightness] = 75,
            [LedMatrixShape] = EmulationLedMatrixShape.Round,
            [LedMatrixHaloRadius] = 25, [LedMatrixBlackDepth] = 100,
            [DotMatrixPalette] = EmulationDotMatrixPalette.Green,
            [DotMatrixShape] = EmulationDotMatrixShape.Round, [DotMatrixCellSize] = 25,
            [DotMatrixDotSize] = 55, [DotMatrixCellGap] = 20,
            [DotMatrixContrast] = 70, [DotMatrixBrightness] = 80,
            [DotMatrixHaloIntensity] = 15, [DotMatrixResponseTime] = 120,
            [DotMatrixPersistence] = 0,
            [SegmentDisplayLayout] = EmulationSegmentDisplayLayout.Seven,
            [SegmentDisplayColor] = EmulationSegmentDisplayColor.Red,
            [SegmentDisplayCellSize] = 45, [SegmentDisplayHorizontalGap] = 15,
            [SegmentDisplayVerticalGap] = 20, [SegmentDisplayThickness] = 55,
            [SegmentDisplaySegmentGap] = 12,
            [SegmentDisplayEndShape] = EmulationSegmentEndShape.Beveled,
            [SegmentDisplayDecimalPoint] = false, [SegmentDisplayColon] = false,
            [SegmentDisplayBrightness] = 85, [SegmentDisplayActivationThreshold] = 45,
            [SegmentDisplayContrast] = 80, [SegmentDisplayOffSegmentVisibility] = 8,
            [SegmentDisplayBlackDepth] = 100, [SegmentDisplayGlow] = 20,
            [SegmentDisplayHaloRadius] = 25, [SegmentDisplayResponseTime] = 30,
            [SegmentDisplayPersistence] = 60,
            [EPaperColorMode] = EmulationEPaperColorMode.Monochrome, [EPaperContrast] = 70,
            [EPaperDithering] = 35, [EPaperRefreshTime] = 500, [EPaperGhosting] = 20,
            [EPaperInkDensity] = 90, [EPaperPaperBrightness] = 90,
            [EPaperPaperWarmth] = 35, [EPaperColorSaturation] = 55,
            [EPaperSurfaceTexture] = 10, [EPaperEdgeSoftness] = 10,
            [ProjectionOpticalBlur] = 20, [ProjectionDiffusion] = 15,
            [ProjectionScreenTexture] = 10, [ProjectionConvergence] = 5,
            [ProjectionLightOutput] = 50, [ProjectionAmbientLight] = 0,
            [ProjectionVignette] = 0
        };

    public static IReadOnlyDictionary<string, EmulationVideoDisplayTechnology> RequiredTechnologies { get; } =
        TechnologyRequirements();

    public static IReadOnlyDictionary<string, string> RequiredParameters { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [CrtScanlineOrientation] = CrtScanlinesEnabled,
            [CrtScanlineIntensity] = CrtScanlinesEnabled,
            [CrtScanlineThickness] = CrtScanlinesEnabled,
            [CrtScanlinePhase] = CrtScanlinesEnabled,
            [CrtScanlineCompensation] = CrtScanlinesEnabled,
            [CrtPatternOrientation] = CrtPatternEnabled,
            [CrtPatternFrequency] = CrtPatternEnabled,
            [CrtPatternPhase] = CrtPatternEnabled,
            [CrtPatternIntensity] = CrtPatternEnabled,
            [FixedPixelMonochromeColor] = FixedPixelSubpixels
        };

    public static IReadOnlyDictionary<string, object> RequiredParameterValues { get; } =
        new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [CrtScanlineOrientation] = true,
            [CrtScanlineIntensity] = true,
            [CrtScanlineThickness] = true,
            [CrtScanlinePhase] = true,
            [CrtScanlineCompensation] = true,
            [CrtPatternOrientation] = true,
            [CrtPatternFrequency] = true,
            [CrtPatternPhase] = true,
            [CrtPatternIntensity] = true,
            [FixedPixelMonochromeColor] = EmulationSubpixelLayout.Monochrome
        };

    public static IReadOnlyDictionary<EmulationVideoDisplayTechnology,
        IReadOnlySet<EmulationVideoDisplayTechnology>> IncompatibleTechnologies { get; }
        = Enum.GetValues<EmulationVideoDisplayTechnology>().ToDictionary(
            technology => technology,
            technology => (IReadOnlySet<EmulationVideoDisplayTechnology>)Enum
                .GetValues<EmulationVideoDisplayTechnology>()
                .Where(other => other != technology)
                .ToHashSet());

    public static IReadOnlyDictionary<string, string> CompatibilityResourceKeys { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ExclusiveDisplayTechnology] = "Emulation.Video.Compatibility.ExclusiveDisplayTechnology",
            [UnsupportedBackend] = "Emulation.Video.Limitation.UnsupportedBackend"
        };

    public static IReadOnlyDictionary<EmulationVideoPreset, EmulationVideoProcessingConfiguration>
        PresetConfigurations { get; } = CreatePresets();

    private static IReadOnlyDictionary<TEnum, string> ResourceKeys<TEnum>(string group,
        IEnumerable<TEnum> values) where TEnum : struct, Enum =>
        values.ToDictionary(value => value, value => $"Emulation.Video.{group}.{value}");

    private static IReadOnlyDictionary<string, EmulationVideoDisplayTechnology> TechnologyRequirements()
    {
        var result = new Dictionary<string, EmulationVideoDisplayTechnology>(StringComparer.Ordinal);
        Add(CrtColorMode, CrtPatternIntensity, EmulationVideoDisplayTechnology.Crt);
        Add(FixedPixelTechnology, FixedPixelBlackDepth, EmulationVideoDisplayTechnology.FixedPixel);
        Add(PlasmaCellStructure, PlasmaPersistence, EmulationVideoDisplayTechnology.Plasma);
        Add(VectorLineThreshold, VectorPersistence, EmulationVideoDisplayTechnology.Vector);
        Add(VfdColor, VfdPersistence, EmulationVideoDisplayTechnology.Vfd);
        Add(LedMatrixColor, LedMatrixBlackDepth, EmulationVideoDisplayTechnology.LedMatrix);
        Add(DotMatrixPalette, DotMatrixPersistence, EmulationVideoDisplayTechnology.DotMatrix);
        Add(SegmentDisplayLayout, SegmentDisplayPersistence,
            EmulationVideoDisplayTechnology.SegmentDisplay);
        Add(EPaperColorMode, EPaperEdgeSoftness, EmulationVideoDisplayTechnology.EPaper);
        Add(ProjectionOpticalBlur, ProjectionVignette,
            EmulationVideoDisplayTechnology.Projection);
        return result;

        void Add(string first, string last, EmulationVideoDisplayTechnology technology)
        {
            var start = Array.IndexOf(ParameterIds, first);
            var end = Array.IndexOf(ParameterIds, last);
            for (var index = start; index <= end; index++) result[ParameterIds[index]] = technology;
        }
    }

    private static IReadOnlyDictionary<EmulationVideoPreset, EmulationVideoProcessingConfiguration> CreatePresets() =>
        new Dictionary<EmulationVideoPreset, EmulationVideoProcessingConfiguration>
        {
            [EmulationVideoPreset.Normal] = new(),
            [EmulationVideoPreset.CrtArcadeColor] = CrtPreset(EmulationVideoSampling.SharpBilinear,
                EmulationCrtColorMode.Color, 35, 20, EmulationCrtMask.ApertureGrille, 45, 8, 8, 40, 50),
            [EmulationVideoPreset.CrtTelevisionColor] = CrtPreset(EmulationVideoSampling.Bilinear,
                EmulationCrtColorMode.Color, 55, 35, EmulationCrtMask.ShadowMask, 35, 18, 15, 25, 60),
            [EmulationVideoPreset.CrtGreen] = CrtPreset(EmulationVideoSampling.SharpBilinear,
                EmulationCrtColorMode.Green, 42, 35, EmulationCrtMask.None, 0, 12, 10, 35, 50),
            [EmulationVideoPreset.CrtAmber] = CrtPreset(EmulationVideoSampling.SharpBilinear,
                EmulationCrtColorMode.Amber, 42, 35, EmulationCrtMask.None, 0, 12, 10, 35, 50),
            [EmulationVideoPreset.CrtWhite] = CrtPreset(EmulationVideoSampling.SharpBilinear,
                EmulationCrtColorMode.White, 38, 25, EmulationCrtMask.None, 0, 10, 8, 30, 50),
            [EmulationVideoPreset.LcdColor] = FixedPixelPreset(EmulationFixedPixelTechnology.Lcd,
                EmulationSubpixelLayout.Rgb, 35, 20, 16, 10, 70, 8),
            [EmulationVideoPreset.LcdMonochrome] = FixedPixelPreset(EmulationFixedPixelTechnology.Lcd,
                EmulationSubpixelLayout.Monochrome, 45, 25, 35, 25, 60, 15),
            [EmulationVideoPreset.LedBacklitLcd] = FixedPixelPreset(
                EmulationFixedPixelTechnology.LedBacklitLcd, EmulationSubpixelLayout.Rgb,
                25, 15, 8, 5, 85, 12),
            [EmulationVideoPreset.Oled] = FixedPixelPreset(EmulationFixedPixelTechnology.Oled,
                EmulationSubpixelLayout.Rgb, 15, 10, 1, 0, null, 100),
            [EmulationVideoPreset.Plasma] = new()
            {
                DisplayTechnology = EmulationVideoDisplayTechnology.Plasma,
                Sampling = EmulationVideoSampling.Bilinear,
                Plasma = new(35, 30, 20, 20, 55, 25, 20, 35)
            },
            [EmulationVideoPreset.Vector] = new()
            {
                DisplayTechnology = EmulationVideoDisplayTechnology.Vector,
                Sampling = EmulationVideoSampling.Bilinear,
                Vector = new(50, 75, 45, 30)
            }
        };

    private static EmulationVideoProcessingConfiguration CrtPreset(EmulationVideoSampling sampling,
        EmulationCrtColorMode color, int beam, int halo, EmulationCrtMask mask, int maskIntensity,
        int curvature, int vignette, int scanlineIntensity, int scanlineThickness) => new()
        {
            DisplayTechnology = EmulationVideoDisplayTechnology.Crt,
            Sampling = sampling,
            Crt = new(ColorMode: color, BeamIntensity: beam, HaloIntensity: halo, Mask: mask,
                MaskIntensity: maskIntensity, HorizontalCurvature: curvature,
                VerticalCurvature: curvature, Vignette: vignette,
                ScanlinesEnabled: true, ScanlineIntensity: scanlineIntensity,
                ScanlineThickness: scanlineThickness)
        };

    private static EmulationVideoProcessingConfiguration FixedPixelPreset(
        EmulationFixedPixelTechnology technology, EmulationSubpixelLayout subpixels, int grid,
        int gap, int responseMilliseconds, int persistence, int? backlight, int? blackDepth) => new()
        {
            DisplayTechnology = EmulationVideoDisplayTechnology.FixedPixel,
            Sampling = EmulationVideoSampling.Nearest,
            FixedPixel = new(technology, subpixels, GridIntensity: grid, PixelGap: gap,
                ResponseTimeMilliseconds: responseMilliseconds, PersistenceIntensity: persistence,
                BacklightIntensity: backlight, BlackDepth: blackDepth)
        };
}
