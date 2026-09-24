using GWGUI.App.Constants.Rendering.Emulation;
using System.Numerics;

namespace GWGUI.App.Rendering.Emulation.Processing.OpenGl;

internal sealed class OpenGlVideoProgramUniforms
{
    private readonly OpenGlNativeApi _api;
    private readonly int _adjustments;
    private readonly int _processing;
    private readonly int _output;
    private readonly int _crtDisplay;
    private readonly int _crtBeam;
    private readonly int _crtOptical;
    private readonly int _crtGeometry;
    private readonly int _crtScanlines;
    private readonly int _crtPattern;
    private readonly int _crtPatternIntensity;
    private readonly int _fixedDisplay;
    private readonly int _fixedSpatial;
    private readonly int _fixedTechnology;
    private readonly int _fixedTemporal;
    private readonly int _plasmaEffect;
    private readonly int _plasmaTemporal;
    private readonly int _plasmaDisplay;
    private readonly int _vectorEffect;
    private readonly int _vectorTemporal;
    private readonly int _vectorDisplay;
    private readonly int _segmentGeometry;
    private readonly int _segmentShape;
    private readonly int _segmentEmission;
    private readonly int _segmentOptical;
    private readonly int _segmentTemporal;
    private readonly int _general;
    private readonly int _restoration;
    private readonly int _temporal;
    private readonly int _signal;
    private readonly int _signal2;
    private readonly int _stylistic;
    private readonly int _stylistic2;
    private readonly int _vfdDisplay;
    private readonly int _vfdStructure;
    private readonly int _vfdOptical;
    private readonly int _ledMatrixEmission;
    private readonly int _ledMatrixStructure;
    private readonly int _dotMatrixGeometry;
    private readonly int _dotMatrixEmission;
    private readonly int _dotMatrixTemporal;
    private readonly int _ePaperInkAndColor;
    private readonly int _ePaperSurface;
    private readonly int _ePaperTemporal;
    private readonly int _projection;
    private readonly int _projectionScreen;

    internal OpenGlVideoProgramUniforms(OpenGlNativeApi api, uint program)
    {
        _api = api;
        _adjustments = Location(OpenGlVideoUniformNames.Adjustments);
        _processing = Location(OpenGlVideoUniformNames.Processing);
        _output = Location(OpenGlVideoUniformNames.Output);
        _crtDisplay = Location(OpenGlVideoUniformNames.CrtDisplay);
        _crtBeam = Location(OpenGlVideoUniformNames.CrtBeam);
        _crtOptical = Location(OpenGlVideoUniformNames.CrtOptical);
        _crtGeometry = Location(OpenGlVideoUniformNames.CrtGeometry);
        _crtScanlines = Location(OpenGlVideoUniformNames.CrtScanlines);
        _crtPattern = Location(OpenGlVideoUniformNames.CrtPattern);
        _crtPatternIntensity = Location(OpenGlVideoUniformNames.CrtPatternIntensity);
        _fixedDisplay = Location(OpenGlVideoUniformNames.FixedDisplay);
        _fixedSpatial = Location(OpenGlVideoUniformNames.FixedSpatial);
        _fixedTechnology = Location(OpenGlVideoUniformNames.FixedTechnology);
        _fixedTemporal = Location(OpenGlVideoUniformNames.FixedTemporal);
        _plasmaEffect = Location(OpenGlVideoUniformNames.PlasmaEffect);
        _plasmaTemporal = Location(OpenGlVideoUniformNames.PlasmaTemporal);
        _plasmaDisplay = Location(OpenGlVideoUniformNames.PlasmaDisplay);
        _vectorEffect = Location(OpenGlVideoUniformNames.VectorEffect);
        _vectorTemporal = Location(OpenGlVideoUniformNames.VectorTemporal);
        _vectorDisplay = Location(OpenGlVideoUniformNames.VectorDisplay);
        _segmentGeometry = Location(OpenGlVideoUniformNames.SegmentGeometry);
        _segmentShape = Location(OpenGlVideoUniformNames.SegmentShape);
        _segmentEmission = Location(OpenGlVideoUniformNames.SegmentEmission);
        _segmentOptical = Location(OpenGlVideoUniformNames.SegmentOptical);
        _segmentTemporal = Location(OpenGlVideoUniformNames.SegmentTemporal);
        _general = Location(OpenGlVideoUniformNames.General);
        _restoration = Location(OpenGlVideoUniformNames.Restoration);
        _temporal = Location(OpenGlVideoUniformNames.Temporal);
        _signal = Location(OpenGlVideoUniformNames.Signal);
        _signal2 = Location(OpenGlVideoUniformNames.Signal2);
        _stylistic = Location(OpenGlVideoUniformNames.Stylistic);
        _stylistic2 = Location(OpenGlVideoUniformNames.Stylistic2);
        _vfdDisplay = Location(OpenGlVideoUniformNames.VfdDisplay);
        _vfdStructure = Location(OpenGlVideoUniformNames.VfdStructure);
        _vfdOptical = Location(OpenGlVideoUniformNames.VfdOptical);
        _ledMatrixEmission = Location(OpenGlVideoUniformNames.LedMatrixEmission);
        _ledMatrixStructure = Location(OpenGlVideoUniformNames.LedMatrixStructure);
        _dotMatrixGeometry = Location(OpenGlVideoUniformNames.DotMatrixGeometry);
        _dotMatrixEmission = Location(OpenGlVideoUniformNames.DotMatrixEmission);
        _dotMatrixTemporal = Location(OpenGlVideoUniformNames.DotMatrixTemporal);
        _ePaperInkAndColor = Location(OpenGlVideoUniformNames.EPaperInkAndColor);
        _ePaperSurface = Location(OpenGlVideoUniformNames.EPaperSurface);
        _ePaperTemporal = Location(OpenGlVideoUniformNames.EPaperTemporal);
        _projection = Location(OpenGlVideoUniformNames.Projection);
        _projectionScreen = Location(OpenGlVideoUniformNames.ProjectionScreen);

        int Location(string name) => api.GetUniformLocation(program, name);
    }

    internal void Apply(EmulationVideoProcessingConfiguration configuration,
        int sourceWidth, int sourceHeight, int outputWidth, int outputHeight,
        bool hasHistory, double elapsedMilliseconds, long sequence, float averageLuminance)
    {
        var adjustments = configuration.Adjustments;
        _api.SetUniform(_adjustments,
            adjustments.Brightness / OpenGlVideoConstants.BrightnessUniformDivisor,
            MathF.Pow(2f,
                adjustments.Contrast / OpenGlVideoConstants.ContrastExponentDivisor),
            (float)EmulationImageAdjustmentFunctions.GammaExponent(adjustments.Gamma),
            1f + adjustments.Saturation / OpenGlVideoConstants.SaturationUniformDivisor);
        _api.SetUniform(_processing,
            adjustments.Sharpness / OpenGlVideoConstants.SharpnessUniformDivisor,
            (float)configuration.Sampling, sourceWidth, sourceHeight);
        _api.SetUniform(_output, outputWidth, outputHeight, 0f, 0f);

        var crt = CrtVideoShaderParameters.From(configuration);
        Set(_crtDisplay, crt.Display);
        Set(_crtBeam, crt.Beam);
        Set(_crtOptical, crt.Optical);
        Set(_crtGeometry, crt.Geometry);
        Set(_crtScanlines, crt.Scanlines);
        Set(_crtPattern, crt.Pattern);
        Set(_crtPatternIntensity, crt.PatternIntensity);

        var fixedPixel = FixedPixelVideoShaderParameters.From(
            configuration, hasHistory, elapsedMilliseconds);
        Set(_fixedDisplay, fixedPixel.Display);
        Set(_fixedSpatial, fixedPixel.Spatial);
        Set(_fixedTechnology, fixedPixel.Technology);
        Set(_fixedTemporal, fixedPixel.Temporal);

        var plasma = PlasmaVideoShaderParameters.From(
            configuration, hasHistory, sequence, averageLuminance);
        Set(_plasmaEffect, plasma.Effect);
        Set(_plasmaTemporal, plasma.Temporal);
        Set(_plasmaDisplay, plasma.Display);

        var vector = VectorVideoShaderParameters.From(configuration, hasHistory);
        Set(_vectorEffect, vector.Effect);
        Set(_vectorTemporal, vector.Temporal);
        Set(_vectorDisplay, vector.Display);

        var segmentDisplay = SegmentDisplayVideoShaderParameters.From(
            configuration, hasHistory, elapsedMilliseconds);
        Set(_segmentGeometry, segmentDisplay.Geometry);
        Set(_segmentShape, segmentDisplay.Shape);
        Set(_segmentEmission, segmentDisplay.Emission);
        Set(_segmentOptical, segmentDisplay.Optical);
        Set(_segmentTemporal, segmentDisplay.Temporal);

        Set(_general, new((float)configuration.DisplayTechnology, hasHistory ? 1f : 0f,
            sequence % OpenGlVideoConstants.ShaderSequenceCycle,
            (float)elapsedMilliseconds));
        Set(_restoration, new(
            configuration.Restoration.Dedithering / OpenGlVideoConstants.PercentageDivisor,
            configuration.Restoration.Denoising / OpenGlVideoConstants.PercentageDivisor,
            configuration.Restoration.Debanding / OpenGlVideoConstants.PercentageDivisor,
            (float)configuration.Restoration.Deinterlacing));
        Set(_temporal, new(
            configuration.Temporal.GeneralPersistence / OpenGlVideoConstants.PercentageDivisor,
            configuration.Temporal.MotionBlur / OpenGlVideoConstants.PercentageDivisor,
            configuration.Temporal.Flicker / OpenGlVideoConstants.PercentageDivisor,
            configuration.Temporal.Interlacing > 0 ? 1f : 0f));
        Set(_signal, new(
            (float)configuration.SignalSimulation.Connection,
            configuration.SignalSimulation.ConnectionIntensity
                / OpenGlVideoConstants.PercentageDivisor,
            (float)configuration.SignalSimulation.Standard,
            configuration.SignalSimulation.StandardIntensity
                / OpenGlVideoConstants.PercentageDivisor));
        Set(_signal2, new(0f, configuration.Temporal.BlackFrameInsertion ? 1f : 0f,
            configuration.Temporal.InterlacingVisibility
                / OpenGlVideoConstants.PercentageDivisor, 0f));
        Set(_stylistic, new(
            configuration.Stylistic.Grain / OpenGlVideoConstants.PercentageDivisor,
            configuration.Stylistic.Vhs / OpenGlVideoConstants.PercentageDivisor,
            configuration.Stylistic.ChromaticAberration
                / OpenGlVideoConstants.PercentageDivisor,
            configuration.Stylistic.Bloom / OpenGlVideoConstants.PercentageDivisor));
        Set(_stylistic2, new(configuration.Stylistic.Sepia ? 1f : 0f, 0f,
            configuration.Restoration.DetailRecovery
                / OpenGlVideoConstants.PercentageDivisor, 0f));

        var vfd = VfdVideoShaderParameters.From(configuration, hasHistory, elapsedMilliseconds);
        Set(_vfdDisplay, vfd.Display);
        Set(_vfdStructure, vfd.Structure);
        Set(_vfdOptical, vfd.Optical);

        var ledMatrix = LedMatrixVideoShaderParameters.From(configuration);
        Set(_ledMatrixEmission, ledMatrix.Emission);
        Set(_ledMatrixStructure, ledMatrix.Structure);

        var dotMatrix = DotMatrixVideoShaderParameters.From(
            configuration, hasHistory, elapsedMilliseconds);
        Set(_dotMatrixGeometry, dotMatrix.Geometry);
        Set(_dotMatrixEmission, dotMatrix.Emission);
        Set(_dotMatrixTemporal, dotMatrix.Temporal);

        var ePaper = EPaperVideoShaderParameters.From(
            configuration, hasHistory, elapsedMilliseconds);
        Set(_ePaperInkAndColor, ePaper.InkAndColor);
        Set(_ePaperSurface, ePaper.PaperSurface);
        Set(_ePaperTemporal, ePaper.Temporal);

        Set(_projection, new(
            configuration.Projection.OpticalBlur / OpenGlVideoConstants.PercentageDivisor,
            configuration.Projection.Diffusion / OpenGlVideoConstants.PercentageDivisor,
            configuration.Projection.ScreenTexture / OpenGlVideoConstants.PercentageDivisor,
            configuration.Projection.Convergence / OpenGlVideoConstants.PercentageDivisor));
        Set(_projectionScreen, new(
            configuration.Projection.LightOutput / OpenGlVideoConstants.PercentageDivisor,
            configuration.Projection.AmbientLight / OpenGlVideoConstants.PercentageDivisor,
            configuration.Projection.Vignette / OpenGlVideoConstants.PercentageDivisor, 0f));
    }

    private void Set(int location, Vector4 value) => _api.SetUniform(location, value);
}
