using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Dictionaries;

public static class EmulationVideoProcessingCatalog
{
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
    public const string CompositeSimulation = nameof(CompositeSimulation);
    public const string SVideoSimulation = nameof(SVideoSimulation);
    public const string RfSimulation = nameof(RfSimulation);
    public const string PalSimulation = nameof(PalSimulation);
    public const string NtscSimulation = nameof(NtscSimulation);
    public const string Grain = nameof(Grain);
    public const string Vhs = nameof(Vhs);
    public const string ChromaticAberration = nameof(ChromaticAberration);
    public const string Bloom = nameof(Bloom);
    public const string Sepia = nameof(Sepia);
    public const string Grayscale = nameof(Grayscale);
    public const string CrtColorMode = nameof(CrtColorMode);
    public const string CrtCustomColor = nameof(CrtCustomColor);
    public const string CrtBeamWidth = nameof(CrtBeamWidth);
    public const string CrtBeamIntensity = nameof(CrtBeamIntensity);
    public const string CrtBeamDiffusion = nameof(CrtBeamDiffusion);
    public const string CrtHaloIntensity = nameof(CrtHaloIntensity);
    public const string CrtMask = nameof(CrtMask);
    public const string CrtMaskSubpixels = nameof(CrtMaskSubpixels);
    public const string CrtMaskIntensity = nameof(CrtMaskIntensity);
    public const string CrtCurvature = nameof(CrtCurvature);
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
    public const string FixedPixelBlackDepth = nameof(FixedPixelBlackDepth);
    public const string PlasmaCellStructure = nameof(PlasmaCellStructure);
    public const string PlasmaDiffusion = nameof(PlasmaDiffusion);
    public const string PlasmaTemporalDithering = nameof(PlasmaTemporalDithering);
    public const string PlasmaPersistence = nameof(PlasmaPersistence);
    public const string VectorLineThreshold = nameof(VectorLineThreshold);
    public const string VectorLineIntensity = nameof(VectorLineIntensity);
    public const string VectorHaloIntensity = nameof(VectorHaloIntensity);
    public const string VectorPersistence = nameof(VectorPersistence);
    public const string VfdColor = nameof(VfdColor);
    public const string VfdPhosphorIntensity = nameof(VfdPhosphorIntensity);
    public const string VfdHaloIntensity = nameof(VfdHaloIntensity);
    public const string VfdPersistence = nameof(VfdPersistence);
    public const string LedMatrixColor = nameof(LedMatrixColor);
    public const string LedMatrixCellSize = nameof(LedMatrixCellSize);
    public const string LedMatrixCellGap = nameof(LedMatrixCellGap);
    public const string LedMatrixDiffusion = nameof(LedMatrixDiffusion);
    public const string LedMatrixBrightness = nameof(LedMatrixBrightness);
    public const string DotMatrixPalette = nameof(DotMatrixPalette);
    public const string DotMatrixShape = nameof(DotMatrixShape);
    public const string DotMatrixDotSize = nameof(DotMatrixDotSize);
    public const string DotMatrixContrast = nameof(DotMatrixContrast);
    public const string DotMatrixResponseTime = nameof(DotMatrixResponseTime);
    public const string SegmentDisplayLayout = nameof(SegmentDisplayLayout);
    public const string SegmentDisplayColor = nameof(SegmentDisplayColor);
    public const string SegmentDisplayThickness = nameof(SegmentDisplayThickness);
    public const string SegmentDisplayContrast = nameof(SegmentDisplayContrast);
    public const string SegmentDisplayGlow = nameof(SegmentDisplayGlow);
    public const string SegmentDisplayResponseTime = nameof(SegmentDisplayResponseTime);
    public const string EPaperColorMode = nameof(EPaperColorMode);
    public const string EPaperContrast = nameof(EPaperContrast);
    public const string EPaperDithering = nameof(EPaperDithering);
    public const string EPaperRefreshTime = nameof(EPaperRefreshTime);
    public const string EPaperGhosting = nameof(EPaperGhosting);
    public const string ProjectionOpticalBlur = nameof(ProjectionOpticalBlur);
    public const string ProjectionDiffusion = nameof(ProjectionDiffusion);
    public const string ProjectionScreenTexture = nameof(ProjectionScreenTexture);
    public const string ProjectionConvergence = nameof(ProjectionConvergence);

    public const string ExclusiveDisplayTechnology = nameof(ExclusiveDisplayTechnology);
    public const string UnsupportedBackend = nameof(UnsupportedBackend);

    private static readonly string[] ParameterIds =
    [
        Brightness, Contrast, Gamma, Saturation, Sharpness, Dedithering, Denoising, Debanding,
        DetailRecovery,
        Deinterlacing, GeneralPersistence, MotionBlur, Flicker, Interlacing,
        InterlacingVisibility, BlackFrameInsertion,
        CompositeSimulation,
        SVideoSimulation,
        RfSimulation,
        PalSimulation,
        NtscSimulation,
        Grain,
        Vhs,
        ChromaticAberration,
        Bloom,
        Sepia,
        Grayscale,
        CrtColorMode, CrtCustomColor, CrtBeamWidth, CrtBeamIntensity, CrtBeamDiffusion,
        CrtHaloIntensity, CrtMask, CrtMaskSubpixels, CrtMaskIntensity, CrtCurvature, CrtVignette,
        CrtScanlinesEnabled, CrtScanlineOrientation, CrtScanlineIntensity, CrtScanlineThickness,
        CrtScanlinePhase, CrtScanlineCompensation, CrtPatternEnabled, CrtPatternOrientation,
        CrtPatternFrequency, CrtPatternPhase, CrtPatternIntensity,
        FixedPixelTechnology, FixedPixelSubpixels, FixedPixelMonochromeColor,
        FixedPixelGridIntensity, FixedPixelPixelGap, FixedPixelResponseTime,
        FixedPixelPersistence, FixedPixelBacklight, FixedPixelBlackDepth,
        PlasmaCellStructure, PlasmaDiffusion, PlasmaTemporalDithering, PlasmaPersistence,
        VectorLineThreshold, VectorLineIntensity, VectorHaloIntensity, VectorPersistence,
        VfdColor, VfdPhosphorIntensity, VfdHaloIntensity, VfdPersistence,
        LedMatrixColor, LedMatrixCellSize, LedMatrixCellGap, LedMatrixDiffusion,
        LedMatrixBrightness, DotMatrixPalette, DotMatrixShape, DotMatrixDotSize,
        DotMatrixContrast, DotMatrixResponseTime, SegmentDisplayLayout, SegmentDisplayColor,
        SegmentDisplayThickness, SegmentDisplayContrast, SegmentDisplayGlow,
        SegmentDisplayResponseTime, EPaperColorMode, EPaperContrast, EPaperDithering,
        EPaperRefreshTime, EPaperGhosting, ProjectionOpticalBlur, ProjectionDiffusion,
        ProjectionScreenTexture, ProjectionConvergence
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

    public static IReadOnlyDictionary<EmulationSubpixelLayout, string> SubpixelLayoutResourceKeys { get; }
        = ResourceKeys("Subpixels", Enum.GetValues<EmulationSubpixelLayout>());

    public static IReadOnlyDictionary<EmulationFixedPixelTechnology, string> FixedPixelTechnologyResourceKeys { get; }
        = ResourceKeys("FixedPixel.Technology", Enum.GetValues<EmulationFixedPixelTechnology>());

    public static IReadOnlyDictionary<EmulationVideoPreset, string> PresetResourceKeys { get; }
        = ResourceKeys("Preset", Enum.GetValues<EmulationVideoPreset>());

    public static IReadOnlyDictionary<EmulationVfdColor, string> VfdColorResourceKeys { get; }
        = ResourceKeys("Vfd.Color", Enum.GetValues<EmulationVfdColor>());

    public static IReadOnlyDictionary<EmulationLedMatrixColor, string> LedMatrixColorResourceKeys { get; }
        = ResourceKeys("LedMatrix.Color", Enum.GetValues<EmulationLedMatrixColor>());

    public static IReadOnlyDictionary<EmulationDotMatrixPalette, string> DotMatrixPaletteResourceKeys { get; }
        = ResourceKeys("DotMatrix.Palette", Enum.GetValues<EmulationDotMatrixPalette>());

    public static IReadOnlyDictionary<EmulationDotMatrixShape, string> DotMatrixShapeResourceKeys { get; }
        = ResourceKeys("DotMatrix.Shape", Enum.GetValues<EmulationDotMatrixShape>());

    public static IReadOnlyDictionary<EmulationSegmentDisplayLayout, string> SegmentDisplayLayoutResourceKeys { get; }
        = ResourceKeys("SegmentDisplay.Layout", Enum.GetValues<EmulationSegmentDisplayLayout>());

    public static IReadOnlyDictionary<EmulationSegmentDisplayColor, string> SegmentDisplayColorResourceKeys { get; }
        = ResourceKeys("SegmentDisplay.Color", Enum.GetValues<EmulationSegmentDisplayColor>());

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
            [CompositeSimulation] = 0,
            [SVideoSimulation] = 0,
            [RfSimulation] = 0,
            [PalSimulation] = 0,
            [NtscSimulation] = 0,
            [Grain] = 0,
            [Vhs] = 0,
            [ChromaticAberration] = 0,
            [Bloom] = 0,
            [Sepia] = 0,
            [Grayscale] = 0,
            [CrtColorMode] = EmulationCrtColorMode.Color, [CrtCustomColor] = null,
            [CrtBeamWidth] = 0, [CrtBeamIntensity] = 0, [CrtBeamDiffusion] = 0,
            [CrtHaloIntensity] = 0, [CrtMask] = EmulationCrtMask.None,
            [CrtMaskSubpixels] = EmulationSubpixelLayout.Rgb, [CrtMaskIntensity] = 0,
            [CrtCurvature] = 0, [CrtVignette] = 0, [CrtScanlinesEnabled] = false,
            [CrtScanlineOrientation] = EmulationPatternOrientation.Horizontal,
            [CrtScanlineIntensity] = 0, [CrtScanlineThickness] = 0, [CrtScanlinePhase] = 0,
            [CrtScanlineCompensation] = 0, [CrtPatternEnabled] = false,
            [CrtPatternOrientation] = EmulationPatternOrientation.Horizontal,
            [CrtPatternFrequency] = 0, [CrtPatternPhase] = 0, [CrtPatternIntensity] = 0,
            [FixedPixelTechnology] = EmulationFixedPixelTechnology.Lcd,
            [FixedPixelSubpixels] = EmulationSubpixelLayout.Rgb, [FixedPixelMonochromeColor] = null,
            [FixedPixelGridIntensity] = 0, [FixedPixelPixelGap] = 0, [FixedPixelResponseTime] = 0,
            [FixedPixelPersistence] = 0, [FixedPixelBacklight] = null, [FixedPixelBlackDepth] = null,
            [PlasmaCellStructure] = 0, [PlasmaDiffusion] = 0, [PlasmaTemporalDithering] = 0,
            [PlasmaPersistence] = 0, [VectorLineThreshold] = 0, [VectorLineIntensity] = 0,
            [VectorHaloIntensity] = 0, [VectorPersistence] = 0,
            [VfdColor] = EmulationVfdColor.Blue, [VfdPhosphorIntensity] = 70,
            [VfdHaloIntensity] = 25, [VfdPersistence] = 20,
            [LedMatrixColor] = EmulationLedMatrixColor.Rgb, [LedMatrixCellSize] = 35,
            [LedMatrixCellGap] = 30, [LedMatrixDiffusion] = 20, [LedMatrixBrightness] = 75,
            [DotMatrixPalette] = EmulationDotMatrixPalette.Green,
            [DotMatrixShape] = EmulationDotMatrixShape.Round, [DotMatrixDotSize] = 55,
            [DotMatrixContrast] = 70, [DotMatrixResponseTime] = 120,
            [SegmentDisplayLayout] = EmulationSegmentDisplayLayout.Seven,
            [SegmentDisplayColor] = EmulationSegmentDisplayColor.Red,
            [SegmentDisplayThickness] = 55, [SegmentDisplayContrast] = 80,
            [SegmentDisplayGlow] = 20, [SegmentDisplayResponseTime] = 30,
            [EPaperColorMode] = EmulationEPaperColorMode.Monochrome, [EPaperContrast] = 70,
            [EPaperDithering] = 35, [EPaperRefreshTime] = 500, [EPaperGhosting] = 20,
            [ProjectionOpticalBlur] = 20, [ProjectionDiffusion] = 15,
            [ProjectionScreenTexture] = 10, [ProjectionConvergence] = 5
        };

    public static IReadOnlyDictionary<string, EmulationVideoDisplayTechnology> RequiredTechnologies { get; } =
        TechnologyRequirements();

    public static IReadOnlyDictionary<string, string> RequiredParameters { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [CrtCustomColor] = CrtColorMode,
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
            [CrtCustomColor] = EmulationCrtColorMode.Custom,
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
        Add(LedMatrixColor, LedMatrixBrightness, EmulationVideoDisplayTechnology.LedMatrix);
        Add(DotMatrixPalette, DotMatrixResponseTime, EmulationVideoDisplayTechnology.DotMatrix);
        Add(SegmentDisplayLayout, SegmentDisplayResponseTime,
            EmulationVideoDisplayTechnology.SegmentDisplay);
        Add(EPaperColorMode, EPaperGhosting, EmulationVideoDisplayTechnology.EPaper);
        Add(ProjectionOpticalBlur, ProjectionConvergence,
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
                Plasma = new(35, 30, 20, 20)
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
                MaskIntensity: maskIntensity, Curvature: curvature, Vignette: vignette,
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
