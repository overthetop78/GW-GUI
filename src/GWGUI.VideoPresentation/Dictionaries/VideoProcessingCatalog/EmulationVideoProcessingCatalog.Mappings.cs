using GWGUI.VideoPresentation.Contracts;
using GWGUI.VideoPresentation.Enums;

namespace GWGUI.VideoPresentation.Dictionaries;

public static partial class EmulationVideoProcessingCatalog
{
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
        = Enum.GetValues<EmulationVideoPreset>().ToDictionary(
            preset => preset,
            preset => preset switch
            {
                EmulationVideoPreset.Oled => "Emulation.Video.FixedPixel.Technology.Oled",
                EmulationVideoPreset.Plasma => "Emulation.Video.Technology.Plasma",
                _ => $"Emulation.Video.Preset.{preset}"
            });

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
        ParameterIds.ToDictionary(id => id,
            id => id == Gamma ? "Emulation.Video.Gamma" : $"Emulation.Video.Parameter.{id}",
            StringComparer.Ordinal);

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

}
