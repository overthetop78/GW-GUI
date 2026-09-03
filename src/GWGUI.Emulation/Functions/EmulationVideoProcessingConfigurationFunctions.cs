using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Functions;

public static class EmulationVideoProcessingConfigurationFunctions
{
    public static EmulationVideoProcessingConfiguration Normalize(
        EmulationVideoProcessingConfiguration? configuration)
    {
        configuration ??= new EmulationVideoProcessingConfiguration();
        var crt = configuration.Crt ?? new EmulationCrtVideoConfiguration();
        var fixedPixel = configuration.FixedPixel ?? new EmulationFixedPixelVideoConfiguration();
        var plasma = configuration.Plasma ?? new EmulationPlasmaVideoConfiguration();
        var vector = configuration.Vector ?? new EmulationVectorVideoConfiguration();
        var vfd = configuration.Vfd ?? new EmulationVfdVideoConfiguration();
        var ledMatrix = configuration.LedMatrix ?? new EmulationLedMatrixVideoConfiguration();
        var dotMatrix = configuration.DotMatrix ?? new EmulationDotMatrixVideoConfiguration();
        var segmentDisplay = configuration.SegmentDisplay
            ?? new EmulationSegmentDisplayVideoConfiguration();
        var ePaper = configuration.EPaper ?? new EmulationEPaperVideoConfiguration();
        var projection = configuration.Projection ?? new EmulationProjectionVideoConfiguration();
        var restoration = configuration.Restoration ?? new EmulationImageRestorationConfiguration();
        var temporal = configuration.Temporal ?? new EmulationTemporalVideoConfiguration();
        var signalSimulation = configuration.SignalSimulation
            ?? new EmulationSignalSimulationConfiguration();
        var stylistic = configuration.Stylistic ?? new EmulationStylisticVideoConfiguration();
        return configuration with
        {
            DisplayTechnology = DefinedOrDefault(configuration.DisplayTechnology,
                EmulationVideoDisplayTechnology.Normal),
            Sampling = DefinedOrDefault(configuration.Sampling, EmulationVideoSampling.Nearest),
            Adjustments = EmulationImageAdjustmentFunctions.Normalize(configuration.Adjustments),
            Restoration = restoration with
            {
                Dedithering = Intensity(restoration.Dedithering),
                Denoising = Intensity(restoration.Denoising),
                Debanding = Intensity(restoration.Debanding),
                DetailRecovery = Intensity(restoration.DetailRecovery),
                Deinterlacing = DefinedOrDefault(restoration.Deinterlacing,
                    EmulationDeinterlacingMode.Off)
            },
            Temporal = temporal with
            {
                GeneralPersistence = Intensity(temporal.GeneralPersistence),
                MotionBlur = Intensity(temporal.MotionBlur),
                Flicker = Intensity(temporal.Flicker),
                Interlacing = Intensity(temporal.Interlacing),
                InterlacingVisibility = Intensity(temporal.InterlacingVisibility)
            },
            SignalSimulation = signalSimulation with
            {
                Connection = DefinedOrDefault(signalSimulation.Connection,
                    EmulationSignalConnection.None),
                ConnectionIntensity = Intensity(signalSimulation.ConnectionIntensity),
                Standard = DefinedOrDefault(signalSimulation.Standard,
                    EmulationSignalStandard.Automatic),
                StandardIntensity = Intensity(signalSimulation.StandardIntensity)
            },
            Stylistic = stylistic with
            {
                Grain = Intensity(stylistic.Grain),
                Vhs = Intensity(stylistic.Vhs),
                ChromaticAberration = Intensity(stylistic.ChromaticAberration),
                Bloom = Intensity(stylistic.Bloom),
                Sepia = stylistic.Sepia
            },
            Crt = crt with
            {
                ColorMode = DefinedOrDefault(crt.ColorMode, EmulationCrtColorMode.Color),
                BeamWidth = Intensity(crt.BeamWidth),
                BeamIntensity = Intensity(crt.BeamIntensity),
                BeamDiffusion = Intensity(crt.BeamDiffusion),
                HaloIntensity = Intensity(crt.HaloIntensity),
                Mask = DefinedOrDefault(crt.Mask, EmulationCrtMask.None),
                MaskSubpixels = DefinedOrDefault(crt.MaskSubpixels, EmulationSubpixelLayout.Rgb),
                MaskIntensity = Intensity(crt.MaskIntensity),
                HorizontalCurvature = SignedIntensity(crt.HorizontalCurvature),
                VerticalCurvature = SignedIntensity(crt.VerticalCurvature),
                Trapezoid = SignedIntensity(crt.Trapezoid),
                Vignette = Intensity(crt.Vignette),
                ScanlineOrientation = DefinedOrDefault(crt.ScanlineOrientation,
                    EmulationPatternOrientation.Horizontal),
                ScanlineIntensity = Intensity(crt.ScanlineIntensity),
                ScanlineThickness = Intensity(crt.ScanlineThickness),
                ScanlinePhase = DefinedOrDefault(crt.ScanlinePhase,
                    EmulationScanlinePhase.Zero),
                ScanlineCompensation = Intensity(crt.ScanlineCompensation),
                PatternOrientation = DefinedOrDefault(crt.PatternOrientation,
                    EmulationPatternOrientation.Horizontal),
                PatternFrequency = Intensity(crt.PatternFrequency),
                PatternPhase = Intensity(crt.PatternPhase),
                PatternIntensity = Intensity(crt.PatternIntensity)
            },
            FixedPixel = fixedPixel with
            {
                Technology = DefinedOrDefault(fixedPixel.Technology,
                    EmulationFixedPixelTechnology.Lcd),
                Subpixels = DefinedOrDefault(fixedPixel.Subpixels, EmulationSubpixelLayout.Rgb),
                GridIntensity = Intensity(fixedPixel.GridIntensity),
                PixelGap = Intensity(fixedPixel.PixelGap),
                MonochromeColorArgb = null,
                MonochromePalette = fixedPixel.MonochromeColorArgb is uint legacyColor
                    ? EmulationMonochromePaletteFunctions.FromArgb(legacyColor)
                    : DefinedOrDefault(fixedPixel.MonochromePalette,
                        EmulationMonochromePalette.Green),
                ResponseTimeMilliseconds = Duration(fixedPixel.ResponseTimeMilliseconds),
                PersistenceIntensity = Intensity(fixedPixel.PersistenceIntensity),
                BacklightIntensity = OptionalIntensity(fixedPixel.BacklightIntensity),
                BlackDepth = OptionalIntensity(fixedPixel.BlackDepth),
                BacklightBleedIntensity = Intensity(fixedPixel.BacklightBleedIntensity)
            },
            Plasma = plasma with
            {
                CellStructure = Intensity(plasma.CellStructure),
                Diffusion = Intensity(plasma.Diffusion),
                TemporalDithering = Intensity(plasma.TemporalDithering),
                PersistenceIntensity = Intensity(plasma.PersistenceIntensity)
            },
            Vector = vector with
            {
                LineThreshold = Intensity(vector.LineThreshold),
                LineIntensity = Intensity(vector.LineIntensity),
                HaloIntensity = Intensity(vector.HaloIntensity),
                PersistenceIntensity = Intensity(vector.PersistenceIntensity)
            },
            Vfd = vfd with
            {
                Color = DefinedOrDefault(vfd.Color, EmulationVfdColor.Blue),
                PhosphorIntensity = Intensity(vfd.PhosphorIntensity),
                HaloIntensity = Intensity(vfd.HaloIntensity),
                PersistenceIntensity = Intensity(vfd.PersistenceIntensity)
            },
            LedMatrix = ledMatrix with
            {
                Color = DefinedOrDefault(ledMatrix.Color, EmulationLedMatrixColor.Rgb),
                CellSize = Intensity(ledMatrix.CellSize),
                CellGap = Intensity(ledMatrix.CellGap),
                Diffusion = Intensity(ledMatrix.Diffusion),
                Brightness = Intensity(ledMatrix.Brightness)
            },
            DotMatrix = dotMatrix with
            {
                Palette = DefinedOrDefault(dotMatrix.Palette, EmulationDotMatrixPalette.Green),
                Shape = DefinedOrDefault(dotMatrix.Shape, EmulationDotMatrixShape.Round),
                DotSize = Intensity(dotMatrix.DotSize),
                Contrast = Intensity(dotMatrix.Contrast),
                ResponseTimeMilliseconds = Duration(dotMatrix.ResponseTimeMilliseconds)
            },
            SegmentDisplay = segmentDisplay with
            {
                Layout = DefinedOrDefault(segmentDisplay.Layout, EmulationSegmentDisplayLayout.Seven),
                Color = DefinedOrDefault(segmentDisplay.Color, EmulationSegmentDisplayColor.Red),
                Thickness = Intensity(segmentDisplay.Thickness),
                Contrast = Intensity(segmentDisplay.Contrast),
                Glow = Intensity(segmentDisplay.Glow),
                ResponseTimeMilliseconds = Duration(segmentDisplay.ResponseTimeMilliseconds)
            },
            EPaper = ePaper with
            {
                ColorMode = DefinedOrDefault(ePaper.ColorMode, EmulationEPaperColorMode.Monochrome),
                Contrast = Intensity(ePaper.Contrast),
                Dithering = Intensity(ePaper.Dithering),
                RefreshTimeMilliseconds = Duration(ePaper.RefreshTimeMilliseconds),
                Ghosting = Intensity(ePaper.Ghosting)
            },
            Projection = projection with
            {
                OpticalBlur = Intensity(projection.OpticalBlur),
                Diffusion = Intensity(projection.Diffusion),
                ScreenTexture = Intensity(projection.ScreenTexture),
                Convergence = Intensity(projection.Convergence)
            }
        };
    }

    private static int SignedIntensity(int value) => Math.Clamp(value, -100, 100);

    private static int Intensity(int value) => Math.Clamp(value,
        EmulationVideoProcessingLimits.IntensityMinimum,
        EmulationVideoProcessingLimits.IntensityMaximum);

    private static int Duration(int value) => Math.Clamp(value,
        EmulationVideoProcessingLimits.DurationMinimumMilliseconds,
        EmulationVideoProcessingLimits.DurationMaximumMilliseconds);

    private static int? OptionalIntensity(int? value) => value is null ? null : Intensity(value.Value);

    private static TEnum DefinedOrDefault<TEnum>(TEnum value, TEnum fallback)
        where TEnum : struct, Enum => Enum.IsDefined(value) ? value : fallback;
}
