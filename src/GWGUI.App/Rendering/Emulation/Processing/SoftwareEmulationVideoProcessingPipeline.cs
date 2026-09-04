using GWGUI.App.Constants.Rendering.Emulation;
using GWGUI.App.Functions.Rendering.Emulation;
using GWGUI.App.Interfaces.Rendering.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal sealed class SoftwareEmulationVideoProcessingPipeline : IEmulationVideoProcessingPipeline
{
    private const float RedLuminance = 0.2126f;
    private const float GreenLuminance = 0.7152f;
    private const float BlueLuminance = 0.0722f;
    private float[]? _fixedPixelHistory;
    private int _historyWidth;
    private int _historyHeight;
    private TimeSpan _historyTimestamp;
    private float[]? _vectorHistory;
    private int _vectorHistoryWidth;
    private int _vectorHistoryHeight;
    private long _vectorHistorySequence;
    private float[]? _dotMatrixHistory;
    private int _dotMatrixHistoryWidth;
    private int _dotMatrixHistoryHeight;
    private TimeSpan _dotMatrixHistoryTimestamp;
    private float[]? _segmentDisplayHistory;
    private int _segmentDisplayHistoryWidth;
    private int _segmentDisplayHistoryHeight;
    private TimeSpan _segmentDisplayHistoryTimestamp;
    private float[]? _ePaperHistory;
    private int _ePaperHistoryWidth;
    private int _ePaperHistoryHeight;
    private TimeSpan _ePaperHistoryTimestamp;
    private readonly FilterGeneralPersistence _generalPersistence = new();
    private readonly FilterMotionBlur _motionBlur = new();
    private readonly FilterInterlacing _interlacing = new();
    private readonly FilterPlasmaPersistence _plasmaPersistence = new();
    private readonly FilterVfdPersistence _vfdPersistence = new();
    private TimeSpan _signalTimestamp;
    private long _signalSequence;

    public EmulationVideoRenderer Renderer => EmulationVideoRenderer.Wpf;

    public VideoFrame Process(EmulationVideoProcessingConfiguration configuration,
        VideoFrame frame, EmulationVideoProcessingSize sourceSize,
        EmulationVideoProcessingSize outputSize)
    {
        var normalized = EmulationVideoProcessingConfigurationFunctions.Normalize(configuration);
        Validate(frame, sourceSize, outputSize);
        var sameSize = sourceSize == outputSize;
        if (sameSize && normalized.Adjustments == new EmulationImageAdjustments()
            && normalized.Restoration == new EmulationImageRestorationConfiguration()
            && normalized.Temporal == new EmulationTemporalVideoConfiguration()
            && normalized.SignalSimulation == new EmulationSignalSimulationConfiguration()
            && normalized.Stylistic == new EmulationStylisticVideoConfiguration()
            && normalized.DisplayTechnology == EmulationVideoDisplayTechnology.Normal)
        {
            ResetHistory();
            _plasmaPersistence.Reset();
            ResetVectorHistory();
            _vfdPersistence.Reset();
            ResetDotMatrixHistory();
            ResetSegmentDisplayHistory();
            ResetEPaperHistory();
            _motionBlur.Reset();
            _generalPersistence.Reset();
            return frame;
        }

        var pixels = EmulationVideoPixelFunctions.ToBgra32(frame);
        var linear = ToLinear(pixels, frame.Width, frame.Height);
        EmulationImageRestorationFunctions.ApplyDeinterlacing(linear, frame.Width, frame.Height,
            normalized.Restoration.Deinterlacing);
        EmulationImageRestorationFunctions.ApplyDedithering(linear, frame.Width, frame.Height,
            normalized.Restoration.Dedithering);
        EmulationImageRestorationFunctions.ApplyDenoising(linear, frame.Width, frame.Height,
            normalized.Restoration.Denoising);
        EmulationImageRestorationFunctions.ApplyDebanding(linear, frame.Width, frame.Height,
            normalized.Restoration.Debanding);
        EmulationImageRestorationFunctions.ApplyDetailRecovery(linear, frame.Width, frame.Height,
            normalized.Restoration.DetailRecovery);
        _interlacing.ApplyFieldWeave(linear, frame.Width, frame.Height, frame.Sequence,
            normalized.Temporal.Interlacing > 0, normalized.Temporal.InterlacingVisibility);
        var signalStandard = ResolveSignalStandard(frame, normalized.SignalSimulation.Standard);
        ApplySignalConnection(linear, frame.Width, frame.Height, frame.Sequence,
            normalized.SignalSimulation, signalStandard);
        ApplySignalStandard(linear, frame.Width, frame.Height, normalized.SignalSimulation,
            signalStandard);
        linear = Resample(linear, frame.Width, frame.Height,
            outputSize.Width, outputSize.Height, normalized.Sampling);
        VideoBrightnessParameterFunctions.Apply(linear, normalized.Adjustments.Brightness);
        VideoContrastParameterFunctions.Apply(linear, normalized.Adjustments.Contrast);
        VideoGammaParameterFunctions.Apply(linear, normalized.Adjustments.Gamma);
        VideoSaturationParameterFunctions.Apply(linear, normalized.Adjustments.Saturation);
        ApplyDisplayTechnology(linear, sourceSize.Width, sourceSize.Height,
            outputSize.Width, outputSize.Height, frame.Sequence, normalized);
        VideoSharpnessParameterFunctions.Apply(linear, outputSize.Width, outputSize.Height,
            normalized.Adjustments.Sharpness);
        FilterVhs.Apply(linear, outputSize.Width, outputSize.Height,
            frame.Sequence, normalized.Stylistic.Vhs);
        FilterChromaticAberration.Apply(linear, outputSize.Width,
            outputSize.Height, normalized.Stylistic.ChromaticAberration);
        FilterBloom.Apply(linear, outputSize.Width, outputSize.Height,
            normalized.Stylistic.Bloom);
        FilterSepia.Apply(linear, normalized.Stylistic.Sepia);
        FilterGrain.Apply(linear, outputSize.Width, outputSize.Height,
            frame.Sequence, normalized.Stylistic.Grain);
        ApplyFixedPixelTemporal(normalized, linear, outputSize.Width, outputSize.Height,
            frame.Timestamp);
        if (normalized.DisplayTechnology == EmulationVideoDisplayTechnology.Plasma)
            _plasmaPersistence.Apply(linear, outputSize.Width, outputSize.Height,
                frame.Sequence, normalized.Plasma.PersistenceIntensity);
        else
            _plasmaPersistence.Reset();
        ApplyVectorTemporal(normalized, linear, outputSize.Width, outputSize.Height,
            frame.Sequence);
        if (normalized.DisplayTechnology == EmulationVideoDisplayTechnology.Vfd)
            _vfdPersistence.Apply(linear, outputSize.Width, outputSize.Height,
                frame.Timestamp, normalized.Vfd.PersistenceMilliseconds);
        else
            _vfdPersistence.Reset();
        ApplyDotMatrixTemporal(normalized, linear, outputSize.Width, outputSize.Height,
            frame.Timestamp);
        ApplySegmentDisplayTemporal(normalized, linear, outputSize.Width, outputSize.Height,
            frame.Timestamp);
        ApplyEPaperTemporal(normalized, linear, outputSize.Width, outputSize.Height,
            frame.Timestamp);
        FilterFlicker.Apply(linear, frame.Sequence, normalized.Temporal.Flicker);
        _motionBlur.Apply(linear, outputSize.Width, outputSize.Height, frame.Sequence,
            normalized.Temporal.MotionBlur);
        _generalPersistence.Apply(linear, outputSize.Width, outputSize.Height, frame.Sequence,
            normalized.Temporal.GeneralPersistence);
        FilterBlackFrameInsertion.Apply(linear, frame.Sequence,
            normalized.Temporal.BlackFrameInsertion);
        var output = ToSrgb(linear, outputSize.Width, outputSize.Height);
        return frame with
        {
            Pixels = output,
            Width = outputSize.Width,
            Height = outputSize.Height,
            Pitch = checked(outputSize.Width * EmulationVideoPixelConstants.BytesPerBgraPixel),
            PixelFormat = EmulationPixelFormat.Xrgb8888
        };
    }

    private static void ApplySignalConnection(float[] colors, int width, int height, long sequence,
        EmulationSignalSimulationConfiguration signal, EmulationSignalStandard standard)
    {
        switch (signal.Connection)
        {
            case EmulationSignalConnection.None:
                break;
            case EmulationSignalConnection.RgbScart:
                SignalConnectionRgbScart.Apply(colors, width, height, signal.ConnectionIntensity);
                break;
            case EmulationSignalConnection.Component:
                SignalConnectionComponent.Apply(colors, width, height, signal.ConnectionIntensity);
                break;
            case EmulationSignalConnection.SVideo:
                SignalConnectionSVideo.Apply(colors, width, height, signal.ConnectionIntensity);
                break;
            case EmulationSignalConnection.Composite:
                SignalConnectionComposite.Apply(colors, width, height, sequence,
                    signal.ConnectionIntensity);
                break;
            case EmulationSignalConnection.Rf:
                SignalConnectionRf.Apply(colors, width, height, sequence,
                    signal.ConnectionIntensity, standard);
                break;
        }
    }

    private void ApplySignalStandard(float[] colors, int width, int height,
        EmulationSignalSimulationConfiguration signal, EmulationSignalStandard standard)
    {
        switch (standard)
        {
            case EmulationSignalStandard.Pal:
                SignalStandardPal.Apply(colors, width, height, signal.StandardIntensity);
                break;
            case EmulationSignalStandard.Ntsc:
                SignalStandardNtsc.Apply(colors, width, height, signal.StandardIntensity);
                break;
            case EmulationSignalStandard.Secam:
                SignalStandardSecam.Apply(colors, width, height, signal.StandardIntensity);
                break;
        }
    }

    private EmulationSignalStandard ResolveSignalStandard(VideoFrame frame,
        EmulationSignalStandard standard)
    {
        if (standard == EmulationSignalStandard.Automatic)
        {
            var elapsed = frame.Sequence > _signalSequence
                ? (frame.Timestamp - _signalTimestamp).TotalMilliseconds : 0;
            standard = elapsed > 18.2 ? EmulationSignalStandard.Pal : EmulationSignalStandard.Ntsc;
        }
        _signalTimestamp = frame.Timestamp;
        _signalSequence = frame.Sequence;
        return standard;
    }

    internal static float SrgbToLinear(float value)
    {
        value = Math.Clamp(value, 0f, 1f);
        return value <= 0.04045f
            ? value / 12.92f
            : MathF.Pow((value + 0.055f) / 1.055f, 2.4f);
    }

    internal static float LinearToSrgb(float value)
    {
        value = Math.Clamp(value, 0f, 1f);
        return value <= 0.0031308f
            ? value * 12.92f
            : 1.055f * MathF.Pow(value, 1f / 2.4f) - 0.055f;
    }

    public void Dispose()
    {
        ResetHistory();
        _plasmaPersistence.Reset();
        ResetVectorHistory();
        _vfdPersistence.Reset();
        ResetDotMatrixHistory();
        ResetSegmentDisplayHistory();
        ResetEPaperHistory();
        _motionBlur.Reset();
        _generalPersistence.Reset();
    }

    private void ApplyFixedPixelTemporal(EmulationVideoProcessingConfiguration configuration,
        float[] colors, int width, int height, TimeSpan timestamp)
    {
        if (configuration.DisplayTechnology != EmulationVideoDisplayTechnology.FixedPixel)
        {
            ResetHistory();
            return;
        }

        var compatibleHistory = _fixedPixelHistory is not null
            && _historyWidth == width && _historyHeight == height
            && timestamp >= _historyTimestamp;
        if (compatibleHistory)
        {
            var elapsedMilliseconds = Math.Max(0.001,
                (timestamp - _historyTimestamp).TotalMilliseconds);
            var response = FilterFixedPixelResponse.BlendFactor(
                configuration.FixedPixel.ResponseTimeMilliseconds, elapsedMilliseconds);
            for (var index = 0; index < colors.Length; index++)
            {
                var responded = FilterFixedPixelResponse.Apply(
                    _fixedPixelHistory![index], colors[index], response);
                colors[index] = FilterFixedPixelPersistence.Apply(
                    responded, _fixedPixelHistory[index],
                    configuration.FixedPixel.PersistenceIntensity);
            }
        }

        _fixedPixelHistory = colors.ToArray();
        _historyWidth = width;
        _historyHeight = height;
        _historyTimestamp = timestamp;
    }

    private void ResetHistory()
    {
        _fixedPixelHistory = null;
        _historyWidth = 0;
        _historyHeight = 0;
        _historyTimestamp = TimeSpan.Zero;
    }

    private void ApplyVectorTemporal(EmulationVideoProcessingConfiguration configuration,
        float[] colors, int width, int height, long sequence)
    {
        if (configuration.DisplayTechnology != EmulationVideoDisplayTechnology.Vector)
        {
            ResetVectorHistory();
            return;
        }
        var compatibleHistory = _vectorHistory is not null
            && _vectorHistoryWidth == width && _vectorHistoryHeight == height
            && sequence >= _vectorHistorySequence;
        if (compatibleHistory)
            FilterVectorPersistence.Apply(colors, _vectorHistory!,
                configuration.Vector.PersistenceIntensity);
        _vectorHistory = colors.ToArray();
        _vectorHistoryWidth = width;
        _vectorHistoryHeight = height;
        _vectorHistorySequence = sequence;
    }

    private void ResetVectorHistory()
    {
        _vectorHistory = null;
        _vectorHistoryWidth = 0;
        _vectorHistoryHeight = 0;
        _vectorHistorySequence = 0;
    }

    private void ApplyDotMatrixTemporal(EmulationVideoProcessingConfiguration configuration,
        float[] colors, int width, int height, TimeSpan timestamp)
    {
        if (configuration.DisplayTechnology != EmulationVideoDisplayTechnology.DotMatrix)
        {
            ResetDotMatrixHistory();
            return;
        }
        var compatibleHistory = _dotMatrixHistory is not null
            && _dotMatrixHistoryWidth == width && _dotMatrixHistoryHeight == height
            && timestamp >= _dotMatrixHistoryTimestamp;
        if (compatibleHistory)
        {
            var elapsedMilliseconds = Math.Max(0.001,
                (timestamp - _dotMatrixHistoryTimestamp).TotalMilliseconds);
            var response = FilterDotMatrixResponse.BlendFactor(
                configuration.DotMatrix.ResponseTimeMilliseconds, elapsedMilliseconds);
            var reflective = configuration.DotMatrix.Palette is EmulationDotMatrixPalette.Green
                or EmulationDotMatrixPalette.Gray;
            var background = configuration.DotMatrix.Palette == EmulationDotMatrixPalette.Green
                ? (.16f, .25f, .075f) : (.64f, .68f, .62f);
            for (var index = 0; index < colors.Length; index++)
            {
                var responded = Lerp(_dotMatrixHistory![index], colors[index], response);
                colors[index] = FilterDotMatrixPersistence.Apply(responded,
                    _dotMatrixHistory[index], configuration.DotMatrix.PersistenceMilliseconds,
                    elapsedMilliseconds, reflective, index % 3 switch
                    {
                        0 => background.Item1,
                        1 => background.Item2,
                        _ => background.Item3
                    });
            }
        }
        _dotMatrixHistory = colors.ToArray();
        _dotMatrixHistoryWidth = width;
        _dotMatrixHistoryHeight = height;
        _dotMatrixHistoryTimestamp = timestamp;
    }

    private void ResetDotMatrixHistory()
    {
        _dotMatrixHistory = null;
        _dotMatrixHistoryWidth = 0;
        _dotMatrixHistoryHeight = 0;
        _dotMatrixHistoryTimestamp = TimeSpan.Zero;
    }

    private void ApplySegmentDisplayTemporal(EmulationVideoProcessingConfiguration configuration,
        float[] colors, int width, int height, TimeSpan timestamp)
    {
        if (configuration.DisplayTechnology != EmulationVideoDisplayTechnology.SegmentDisplay)
        {
            ResetSegmentDisplayHistory();
            return;
        }
        var compatibleHistory = _segmentDisplayHistory is not null
            && _segmentDisplayHistoryWidth == width && _segmentDisplayHistoryHeight == height
            && timestamp >= _segmentDisplayHistoryTimestamp;
        if (compatibleHistory)
        {
            var elapsedMilliseconds = Math.Max(0.001,
                (timestamp - _segmentDisplayHistoryTimestamp).TotalMilliseconds);
            var responseMilliseconds = configuration.SegmentDisplay.ResponseTimeMilliseconds;
            var response = responseMilliseconds == 0 ? 1f : (float)(1d
                - Math.Exp(-elapsedMilliseconds / responseMilliseconds));
            for (var index = 0; index < colors.Length; index++)
                colors[index] = Lerp(_segmentDisplayHistory![index], colors[index], response);
        }
        _segmentDisplayHistory = colors.ToArray();
        _segmentDisplayHistoryWidth = width;
        _segmentDisplayHistoryHeight = height;
        _segmentDisplayHistoryTimestamp = timestamp;
    }

    private void ResetSegmentDisplayHistory()
    {
        _segmentDisplayHistory = null;
        _segmentDisplayHistoryWidth = 0;
        _segmentDisplayHistoryHeight = 0;
        _segmentDisplayHistoryTimestamp = TimeSpan.Zero;
    }

    private void ApplyEPaperTemporal(EmulationVideoProcessingConfiguration configuration,
        float[] colors, int width, int height, TimeSpan timestamp)
    {
        if (configuration.DisplayTechnology != EmulationVideoDisplayTechnology.EPaper)
        {
            ResetEPaperHistory();
            return;
        }
        var compatibleHistory = _ePaperHistory is not null
            && _ePaperHistoryWidth == width && _ePaperHistoryHeight == height
            && timestamp >= _ePaperHistoryTimestamp;
        if (compatibleHistory)
        {
            var elapsedMilliseconds = Math.Max(0.001,
                (timestamp - _ePaperHistoryTimestamp).TotalMilliseconds);
            var refreshMilliseconds = configuration.EPaper.RefreshTimeMilliseconds;
            var response = refreshMilliseconds == 0 ? 1f : (float)(1d
                - Math.Exp(-elapsedMilliseconds / refreshMilliseconds));
            var ghosting = configuration.EPaper.Ghosting / 100f * 0.4f;
            for (var index = 0; index < colors.Length; index++)
            {
                var refreshed = Lerp(_ePaperHistory![index], colors[index], response);
                colors[index] = Lerp(refreshed, _ePaperHistory[index], ghosting);
            }
        }
        _ePaperHistory = colors.ToArray();
        _ePaperHistoryWidth = width;
        _ePaperHistoryHeight = height;
        _ePaperHistoryTimestamp = timestamp;
    }

    private void ResetEPaperHistory()
    {
        _ePaperHistory = null;
        _ePaperHistoryWidth = 0;
        _ePaperHistoryHeight = 0;
        _ePaperHistoryTimestamp = TimeSpan.Zero;
    }

    private static void Validate(VideoFrame frame, EmulationVideoProcessingSize sourceSize,
        EmulationVideoProcessingSize outputSize)
    {
        if (sourceSize.Width != frame.Width || sourceSize.Height != frame.Height)
            throw new ArgumentException(nameof(sourceSize));
        if (outputSize.Width <= 0 || outputSize.Height <= 0)
            throw new ArgumentOutOfRangeException(nameof(outputSize));
    }

    private static float[] ToLinear(byte[] pixels, int width, int height)
    {
        var result = new float[checked(width * height * 3)];
        for (var index = 0; index < width * height; index++)
        {
            var pixel = index * EmulationVideoPixelConstants.BytesPerBgraPixel;
            var color = index * 3;
            result[color] = SrgbToLinear(pixels[pixel + EmulationVideoPixelConstants.RedByteOffset] / 255f);
            result[color + 1] = SrgbToLinear(
                pixels[pixel + EmulationVideoPixelConstants.GreenByteOffset] / 255f);
            result[color + 2] = SrgbToLinear(
                pixels[pixel + EmulationVideoPixelConstants.BlueByteOffset] / 255f);
        }
        return result;
    }

    private static void ApplyDisplayTechnology(float[] colors, int sourceWidth, int sourceHeight,
        int outputWidth, int outputHeight, long sequence,
        EmulationVideoProcessingConfiguration configuration)
    {
        if (configuration.DisplayTechnology == EmulationVideoDisplayTechnology.FixedPixel)
        {
            FilterFixedPixel.Apply(colors, sourceWidth, sourceHeight,
                outputWidth, outputHeight, configuration.FixedPixel);
            return;
        }
        if (configuration.DisplayTechnology == EmulationVideoDisplayTechnology.Plasma)
        {
            var plasma = configuration.Plasma;
            FilterPlasmaBlackDepth.Apply(colors, plasma.BlackDepth);
            FilterPlasmaGammaResponse.Apply(colors, plasma.GammaResponse);
            FilterPlasmaPhosphorIntensity.Apply(colors, plasma.PhosphorIntensity);
            FilterPlasmaAutomaticBrightnessLimiter.Apply(colors,
                plasma.AutomaticBrightnessLimiter);
            FilterPlasmaCellStructure.Apply(colors, sourceWidth, sourceHeight,
                outputWidth, outputHeight, plasma.CellStructure);
            FilterPlasmaTemporalDithering.Apply(colors, outputWidth, outputHeight,
                sequence, plasma.TemporalDithering);
            FilterPlasmaLightDiffusion.Apply(colors, sourceWidth, sourceHeight,
                outputWidth, outputHeight, plasma.Diffusion);
            return;
        }
        if (configuration.DisplayTechnology == EmulationVideoDisplayTechnology.Vector)
        {
            var vector = configuration.Vector;
            if (vector.LineIntensity > 0)
            {
                var emission = FilterVectorLineDetection.Detect(colors, outputWidth,
                    outputHeight, vector.LineThreshold);
                emission = FilterVectorBeamWidth.Apply(emission, outputWidth, outputHeight,
                    vector.BeamWidth);
                emission = FilterVectorBeamFocus.Apply(emission, outputWidth, outputHeight,
                    vector.BeamFocus);
                FilterVectorLineIntensity.Apply(colors, emission, vector.LineIntensity);
                FilterVectorHalo.Apply(colors, emission, outputWidth, outputHeight,
                    vector.LineIntensity, vector.HaloIntensity, vector.HaloRadius);
                FilterVectorPhosphorColor.Apply(colors, vector.PhosphorColor);
            }
            return;
        }
        if (configuration.DisplayTechnology == EmulationVideoDisplayTechnology.Vfd)
        {
            var vfd = configuration.Vfd;
            var emission = FilterVfdEmissionThreshold.Extract(colors, vfd.EmissionThreshold);
            FilterVfdCellStructure.Apply(emission, outputWidth, outputHeight,
                sourceWidth, sourceHeight, vfd.Structure, vfd.CellSize, vfd.CellGap);
            FilterVfdPhosphorIntensity.Apply(emission, vfd.PhosphorIntensity);
            var halo = FilterVfdHalo.Create(emission, outputWidth, outputHeight,
                sourceWidth, sourceHeight, vfd.HaloRadius, vfd.HaloIntensity);
            FilterVfdGlass.Apply(colors, vfd.GlassDarkening);
            FilterVfdPhosphorColor.Apply(colors, emission, halo, vfd.Color);
            return;
        }
        if (configuration.DisplayTechnology == EmulationVideoDisplayTechnology.LedMatrix)
        {
            var ledMatrix = configuration.LedMatrix;
            var cells = FilterLedMatrixCellStructure.Create(colors, sourceWidth,
                sourceHeight, outputWidth, outputHeight, ledMatrix.CellSize,
                ledMatrix.CellGap, ledMatrix.Shape, ledMatrix.HaloRadius);
            FilterLedMatrixColor.Apply(cells.Emission, ledMatrix.Color);
            FilterLedMatrixBrightness.Apply(cells.Emission, ledMatrix.Brightness);
            FilterLedMatrixBlackDepth.Compose(colors, cells, ledMatrix.Diffusion,
                ledMatrix.BlackDepth);
            return;
        }
        if (configuration.DisplayTechnology == EmulationVideoDisplayTechnology.DotMatrix)
        {
            FilterDotMatrix.Apply(colors, sourceWidth, sourceHeight, outputWidth, outputHeight,
                configuration.DotMatrix);
            return;
        }
        if (configuration.DisplayTechnology == EmulationVideoDisplayTechnology.SegmentDisplay)
        {
            FilterSegmentDisplay.Apply(colors, outputWidth, outputHeight,
                configuration.SegmentDisplay);
            return;
        }
        if (configuration.DisplayTechnology == EmulationVideoDisplayTechnology.EPaper)
        {
            FilterEPaper.Apply(colors, outputWidth, outputHeight,
                configuration.EPaper);
            return;
        }
        if (configuration.DisplayTechnology == EmulationVideoDisplayTechnology.Projection)
        {
            FilterProjection.Apply(colors, outputWidth, outputHeight,
                configuration.Projection);
            return;
        }
        if (configuration.DisplayTechnology != EmulationVideoDisplayTechnology.Crt) return;

        if (configuration.Crt.ColorMode != EmulationCrtColorMode.Color)
        {
            var tint = CrtTint(configuration.Crt);
            for (var index = 0; index < colors.Length; index += 3)
            {
                var luminance = colors[index] * RedLuminance
                    + colors[index + 1] * GreenLuminance
                    + colors[index + 2] * BlueLuminance;
                colors[index] = luminance * tint.Red;
                colors[index + 1] = luminance * tint.Green;
                colors[index + 2] = luminance * tint.Blue;
            }
        }
        FilterCrt.Apply(colors, outputWidth, outputHeight, sourceWidth, sourceHeight,
            configuration.Crt);
    }

    private static (float Red, float Green, float Blue) CrtTint(
        EmulationCrtVideoConfiguration configuration)
    {
        var argb = configuration.ColorMode switch
        {
            EmulationCrtColorMode.Green => 0xFF66FF66u,
            EmulationCrtColorMode.Amber => 0xFFFFB000u,
            EmulationCrtColorMode.White => 0xFFFFFFFFu,
            EmulationCrtColorMode.Gray => 0xFFB0B0B0u,
            _ => 0xFFFFFFFFu
        };
        return (
            SrgbToLinear(((argb >> 16) & 0xff) / 255f),
            SrgbToLinear(((argb >> 8) & 0xff) / 255f),
            SrgbToLinear((argb & 0xff) / 255f));
    }

    private static float[] Resample(float[] source, int sourceWidth, int sourceHeight,
        int outputWidth, int outputHeight, EmulationVideoSampling sampling)
    {
        if (sourceWidth == outputWidth && sourceHeight == outputHeight) return source;
        var output = new float[checked(outputWidth * outputHeight * 3)];
        var scaleX = outputWidth / (float)sourceWidth;
        var scaleY = outputHeight / (float)sourceHeight;
        Parallel.For(0, outputHeight, y =>
        {
            var sourceY = (y + 0.5f) / scaleY - 0.5f;
            for (var x = 0; x < outputWidth; x++)
            {
                var sourceX = (x + 0.5f) / scaleX - 0.5f;
                if (sampling == EmulationVideoSampling.Xbr)
                {
                    FilterXbr.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }
                if (sampling == EmulationVideoSampling.Xbrz)
                {
                    FilterXbrz.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }
                if (sampling == EmulationVideoSampling.Hqx)
                {
                    FilterHqx.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }
                if (sampling == EmulationVideoSampling.Hq2x)
                {
                    FilterHq2x.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }
                if (sampling == EmulationVideoSampling.Hq3x)
                {
                    FilterHq3x.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }
                if (sampling == EmulationVideoSampling.Hq4x)
                {
                    FilterHq4x.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }
                if (sampling == EmulationVideoSampling.TwoXSai)
                {
                    FilterTwoXSai.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }
                if (sampling == EmulationVideoSampling.SuperTwoXSai)
                {
                    FilterSuperTwoXSai.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }
                if (sampling == EmulationVideoSampling.SuperEagle)
                {
                    FilterSuperEagle.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }
                if (sampling == EmulationVideoSampling.EpxScale2x)
                {
                    FilterEpxScale2x.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }                if (sampling == EmulationVideoSampling.ScaleFx)
                {
                    FilterScaleFx.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }
                if (sampling == EmulationVideoSampling.ScaleNx)
                {
                    FilterScaleNx.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f, scaleX, scaleY,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }
                if (sampling == EmulationVideoSampling.Sabr)
                {
                    FilterSabr.Sample(source, sourceWidth, sourceHeight,
                        sourceX + 0.5f, sourceY + 0.5f,
                        output.AsSpan((y * outputWidth + x) * 3, 3));
                    continue;
                }
                for (var channel = 0; channel < 3; channel++)
                {
                    output[(y * outputWidth + x) * 3 + channel] = sampling switch
                    {
                        EmulationVideoSampling.Nearest =>
                            FilterNormal.Sample(source, sourceWidth, sourceHeight,
                                sourceX, sourceY, channel),
                        EmulationVideoSampling.Bilinear =>
                            FilterBilinear.Sample(source, sourceWidth, sourceHeight,
                                sourceX, sourceY, channel),
                        EmulationVideoSampling.SharpBilinear =>
                            FilterSharpBilinear.Sample(source, sourceWidth, sourceHeight,
                                sourceX, sourceY, scaleX, scaleY, channel),
                        EmulationVideoSampling.Bicubic =>
                            FilterBicubic.Sample(source, sourceWidth, sourceHeight,
                                sourceX, sourceY, channel),
                        EmulationVideoSampling.Jinc2 =>
                            FilterJinc2.Sample(source, sourceWidth, sourceHeight,
                                sourceX, sourceY, channel),
                        EmulationVideoSampling.Lanczos =>
                            FilterLanczos.Sample(source, sourceWidth, sourceHeight,
                                sourceX, sourceY, channel),                        _ => throw new ArgumentOutOfRangeException(nameof(sampling), sampling, null)
                    };
                }
            }
        });
        return output;
    }



    private static float Sample(float[] source, int width, int height,
        int x, int y, int channel)
    {
        x = Math.Clamp(x, 0, width - 1);
        y = Math.Clamp(y, 0, height - 1);
        return source[(y * width + x) * 3 + channel];
    }

    private static float Lerp(float first, float second, float amount) =>
        first + (second - first) * amount;

    private static byte[] ToSrgb(float[] colors, int width, int height)
    {
        var output = new byte[checked(width * height
            * EmulationVideoPixelConstants.BytesPerBgraPixel)];
        for (var index = 0; index < width * height; index++)
        {
            var color = index * 3;
            var pixel = index * EmulationVideoPixelConstants.BytesPerBgraPixel;
            output[pixel + EmulationVideoPixelConstants.RedByteOffset] = ToByte(colors[color]);
            output[pixel + EmulationVideoPixelConstants.GreenByteOffset] = ToByte(colors[color + 1]);
            output[pixel + EmulationVideoPixelConstants.BlueByteOffset] = ToByte(colors[color + 2]);
            output[pixel + EmulationVideoPixelConstants.AlphaByteOffset] =
                EmulationVideoPixelConstants.OpaqueAlpha;
        }
        return output;
    }

    private static byte ToByte(float linear) => (byte)Math.Clamp(
        (int)MathF.Round(LinearToSrgb(linear) * 255f), 0, 255);
}
