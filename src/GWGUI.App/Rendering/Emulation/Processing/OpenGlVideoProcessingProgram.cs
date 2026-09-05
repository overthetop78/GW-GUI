using System.Runtime.InteropServices;
using System.Text;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal sealed class OpenGlVideoProcessingProgram : IDisposable
{
    private const uint VertexShader = 0x8B31;
    private const uint FragmentShader = 0x8B30;
    private const uint CompileStatus = 0x8B81;
    private const uint LinkStatus = 0x8B82;
    private const int InfoLogCapacity = 4096;
    private readonly DeleteProgramDelegate _deleteProgram = Load<DeleteProgramDelegate>("glDeleteProgram");
    private readonly GetUniformLocationDelegate _getUniformLocation = Load<GetUniformLocationDelegate>("glGetUniformLocation");
    private readonly Uniform1iDelegate _uniform1i = Load<Uniform1iDelegate>("glUniform1i");
    private readonly Uniform4fDelegate _uniform4f = Load<Uniform4fDelegate>("glUniform4f");
    private readonly UseProgramDelegate _useProgram = Load<UseProgramDelegate>("glUseProgram");
    private readonly uint _program;
    private readonly int _sourceLocation;
    private readonly int _historyLocation;
    private readonly int _adjustmentsLocation;
    private readonly int _processingLocation;
    private readonly int _outputLocation;
    private readonly int _crtDisplayLocation;
    private readonly int _crtBeamLocation;
    private readonly int _crtOpticalLocation;
    private readonly int _crtGeometryLocation;
    private readonly int _crtScanlinesLocation;
    private readonly int _crtPatternLocation;
    private readonly int _crtPatternIntensityLocation;
    private readonly int _fixedDisplayLocation;
    private readonly int _fixedSpatialLocation;
    private readonly int _fixedTechnologyLocation;
    private readonly int _fixedTemporalLocation;
    private readonly int _plasmaEffectLocation;
    private readonly int _plasmaTemporalLocation;
    private readonly int _plasmaDisplayLocation;
    private readonly int _vectorEffectLocation;
    private readonly int _vectorTemporalLocation;
    private readonly int _vectorDisplayLocation;
    private readonly int _segmentGeometryLocation;
    private readonly int _segmentShapeLocation;
    private readonly int _segmentEmissionLocation;
    private readonly int _segmentOpticalLocation;
    private readonly int _segmentTemporalLocation;
    private readonly int _generalLocation;
    private readonly int _restorationLocation;
    private readonly int _temporalLocation;
    private readonly int _signalLocation;
    private readonly int _signal2Location;
    private readonly int _stylisticLocation;
    private readonly int _stylistic2Location;
    private readonly int _vfdDisplayLocation;
    private readonly int _vfdStructureLocation;
    private readonly int _vfdOpticalLocation;
    private readonly int _ledMatrixEmissionLocation;
    private readonly int _ledMatrixStructureLocation;
    private readonly int _dotMatrixGeometryLocation;
    private readonly int _dotMatrixEmissionLocation;
    private readonly int _dotMatrixTemporalLocation;
    private readonly int _ePaperInkAndColorLocation;
    private readonly int _ePaperSurfaceLocation;
    private readonly int _ePaperTemporalLocation;
    private readonly int _projectionLocation;

    internal OpenGlVideoProcessingProgram(EmulationVideoSampling sampling,
        EmulationVideoDisplayTechnology displayTechnology)
    {
        uint vertex = 0;
        uint fragment = 0;
        uint program = 0;
        try
        {
            vertex = Compile(VertexShader, VertexSource);
            fragment = Compile(FragmentShader, Fragment(sampling, displayTechnology));
            var createProgram = Load<CreateProgramDelegate>("glCreateProgram");
            var attachShader = Load<AttachShaderDelegate>("glAttachShader");
            var linkProgram = Load<LinkProgramDelegate>("glLinkProgram");
            var getProgram = Load<GetProgramivDelegate>("glGetProgramiv");
            program = createProgram();
            attachShader(program, vertex);
            attachShader(program, fragment);
            linkProgram(program);
            getProgram(program, LinkStatus, out var linked);
            if (linked == 0) throw new InvalidOperationException(ProgramLog(program));
            _program = program;
            program = 0;
        }
        finally
        {
            var deleteShader = Load<DeleteShaderDelegate>("glDeleteShader");
            if (vertex != 0) deleteShader(vertex);
            if (fragment != 0) deleteShader(fragment);
            if (program != 0) _deleteProgram(program);
        }
        _sourceLocation = _getUniformLocation(_program, "Source");
        _historyLocation = _getUniformLocation(_program, "History");
        _adjustmentsLocation = _getUniformLocation(_program, "Adjustments");
        _processingLocation = _getUniformLocation(_program, "Processing");
        _outputLocation = _getUniformLocation(_program, "Output");
        _crtDisplayLocation = _getUniformLocation(_program, "CrtDisplay");
        _crtBeamLocation = _getUniformLocation(_program, "CrtBeam");
        _crtOpticalLocation = _getUniformLocation(_program, "CrtOptical");
        _crtGeometryLocation = _getUniformLocation(_program, "CrtGeometry");
        _crtScanlinesLocation = _getUniformLocation(_program, "CrtScanlines");
        _crtPatternLocation = _getUniformLocation(_program, "CrtPattern");
        _crtPatternIntensityLocation = _getUniformLocation(_program, "CrtPatternIntensity");
        _fixedDisplayLocation = _getUniformLocation(_program, "FixedDisplay");
        _fixedSpatialLocation = _getUniformLocation(_program, "FixedSpatial");
        _fixedTechnologyLocation = _getUniformLocation(_program, "FixedTechnology");
        _fixedTemporalLocation = _getUniformLocation(_program, "FixedTemporal");
        _plasmaEffectLocation = _getUniformLocation(_program, "PlasmaEffect");
        _plasmaTemporalLocation = _getUniformLocation(_program, "PlasmaTemporal");
        _plasmaDisplayLocation = _getUniformLocation(_program, "PlasmaDisplay");
        _vectorEffectLocation = _getUniformLocation(_program, "VectorEffect");
        _vectorTemporalLocation = _getUniformLocation(_program, "VectorTemporal");
        _vectorDisplayLocation = _getUniformLocation(_program, "VectorDisplay");
        _segmentGeometryLocation = _getUniformLocation(_program, "SegmentGeometry");
        _segmentShapeLocation = _getUniformLocation(_program, "SegmentShape");
        _segmentEmissionLocation = _getUniformLocation(_program, "SegmentEmission");
        _segmentOpticalLocation = _getUniformLocation(_program, "SegmentOptical");
        _segmentTemporalLocation = _getUniformLocation(_program, "SegmentTemporal");
        _generalLocation = _getUniformLocation(_program, "General");
        _restorationLocation = _getUniformLocation(_program, "Restoration");
        _temporalLocation = _getUniformLocation(_program, "Temporal");
        _signalLocation = _getUniformLocation(_program, "Signal");
        _signal2Location = _getUniformLocation(_program, "Signal2");
        _stylisticLocation = _getUniformLocation(_program, "Stylistic");
        _stylistic2Location = _getUniformLocation(_program, "Stylistic2");
        _vfdDisplayLocation = _getUniformLocation(_program, "VfdDisplay");
        _vfdStructureLocation = _getUniformLocation(_program, "VfdStructure");
        _vfdOpticalLocation = _getUniformLocation(_program, "VfdOptical");
        _ledMatrixEmissionLocation = _getUniformLocation(_program, "LedMatrixEmission");
        _ledMatrixStructureLocation = _getUniformLocation(_program, "LedMatrixStructure");
        _dotMatrixGeometryLocation = _getUniformLocation(_program, "DotMatrixGeometry");
        _dotMatrixEmissionLocation = _getUniformLocation(_program, "DotMatrixEmission");
        _dotMatrixTemporalLocation = _getUniformLocation(_program, "DotMatrixTemporal");
        _ePaperInkAndColorLocation = _getUniformLocation(_program, "EPaperInkAndColor");
        _ePaperSurfaceLocation = _getUniformLocation(_program, "EPaperSurface");
        _ePaperTemporalLocation = _getUniformLocation(_program, "EPaperTemporal");
        _projectionLocation = _getUniformLocation(_program, "Projection");
        _useProgram(_program);
        _uniform1i(_sourceLocation, 0);
        _uniform1i(_historyLocation, 1);
        _useProgram(0);
    }

    internal void Use(EmulationVideoProcessingConfiguration configuration,
        int sourceWidth, int sourceHeight, int outputWidth, int outputHeight,
        bool hasHistory = false, double elapsedMilliseconds = 0, long sequence = 0,
        float averageLuminance = 0f)
    {
        var adjustments = configuration.Adjustments;
        _useProgram(_program);
        _uniform4f(_adjustmentsLocation, adjustments.Brightness / 20f,
            MathF.Pow(2f, adjustments.Contrast / 5f),
            (float)EmulationImageAdjustmentFunctions.GammaExponent(adjustments.Gamma),
            1f + adjustments.Saturation / 10f);
        _uniform4f(_processingLocation, adjustments.Sharpness / 10f,
            (float)configuration.Sampling, sourceWidth, sourceHeight);
        _uniform4f(_outputLocation, outputWidth, outputHeight, 0f, 0f);
        var crt = CrtVideoShaderParameters.From(configuration);
        Set(_crtDisplayLocation, crt.Display);
        Set(_crtBeamLocation, crt.Beam);
        Set(_crtOpticalLocation, crt.Optical);
        Set(_crtGeometryLocation, crt.Geometry);
        Set(_crtScanlinesLocation, crt.Scanlines);
        Set(_crtPatternLocation, crt.Pattern);
        Set(_crtPatternIntensityLocation, crt.PatternIntensity);
        var fixedPixel = FixedPixelVideoShaderParameters.From(
            configuration, hasHistory, elapsedMilliseconds);
        Set(_fixedDisplayLocation, fixedPixel.Display);
        Set(_fixedSpatialLocation, fixedPixel.Spatial);
        Set(_fixedTechnologyLocation, fixedPixel.Technology);
        Set(_fixedTemporalLocation, fixedPixel.Temporal);
        var plasma = PlasmaVideoShaderParameters.From(configuration, hasHistory, sequence,
            averageLuminance);
        Set(_plasmaEffectLocation, plasma.Effect);
        Set(_plasmaTemporalLocation, plasma.Temporal);
        Set(_plasmaDisplayLocation, plasma.Display);
        var vector = VectorVideoShaderParameters.From(configuration, hasHistory);
        Set(_vectorEffectLocation, vector.Effect);
        Set(_vectorTemporalLocation, vector.Temporal);
        Set(_vectorDisplayLocation, vector.Display);
        var segmentDisplay = SegmentDisplayVideoShaderParameters.From(configuration,
            hasHistory, elapsedMilliseconds);
        Set(_segmentGeometryLocation, segmentDisplay.Geometry);
        Set(_segmentShapeLocation, segmentDisplay.Shape);
        Set(_segmentEmissionLocation, segmentDisplay.Emission);
        Set(_segmentOpticalLocation, segmentDisplay.Optical);
        Set(_segmentTemporalLocation, segmentDisplay.Temporal);
        Set(_generalLocation, new((float)configuration.DisplayTechnology, hasHistory ? 1f : 0f, sequence % 4096, (float)elapsedMilliseconds));
        Set(_restorationLocation, new(configuration.Restoration.Dedithering / 100f, configuration.Restoration.Denoising / 100f, configuration.Restoration.Debanding / 100f, (float)configuration.Restoration.Deinterlacing));
        Set(_temporalLocation, new(configuration.Temporal.GeneralPersistence / 100f, configuration.Temporal.MotionBlur / 100f, configuration.Temporal.Flicker / 100f, configuration.Temporal.Interlacing > 0 ? 1f : 0f));
        Set(_signalLocation, new((float)configuration.SignalSimulation.Connection,
            configuration.SignalSimulation.ConnectionIntensity / 100f,
            (float)configuration.SignalSimulation.Standard,
            configuration.SignalSimulation.StandardIntensity / 100f));
        Set(_signal2Location, new(0f, configuration.Temporal.BlackFrameInsertion ? 1f : 0f,
            configuration.Temporal.InterlacingVisibility / 100f, 0f));
        Set(_stylisticLocation, new(configuration.Stylistic.Grain / 100f, configuration.Stylistic.Vhs / 100f, configuration.Stylistic.ChromaticAberration / 100f, configuration.Stylistic.Bloom / 100f));
        Set(_stylistic2Location, new(configuration.Stylistic.Sepia ? 1f : 0f, 0f, configuration.Restoration.DetailRecovery / 100f, 0f));
        var vfd = VfdVideoShaderParameters.From(configuration, hasHistory, elapsedMilliseconds);
        Set(_vfdDisplayLocation, vfd.Display);
        Set(_vfdStructureLocation, vfd.Structure);
        Set(_vfdOpticalLocation, vfd.Optical);
        var ledMatrix = LedMatrixVideoShaderParameters.From(configuration);
        Set(_ledMatrixEmissionLocation, ledMatrix.Emission);
        Set(_ledMatrixStructureLocation, ledMatrix.Structure);
        var dotMatrix = DotMatrixVideoShaderParameters.From(configuration, hasHistory,
            elapsedMilliseconds);
        Set(_dotMatrixGeometryLocation, dotMatrix.Geometry);
        Set(_dotMatrixEmissionLocation, dotMatrix.Emission);
        Set(_dotMatrixTemporalLocation, dotMatrix.Temporal);
        var ePaper = EPaperVideoShaderParameters.From(configuration, hasHistory,
            elapsedMilliseconds);
        Set(_ePaperInkAndColorLocation, ePaper.InkAndColor);
        Set(_ePaperSurfaceLocation, ePaper.PaperSurface);
        Set(_ePaperTemporalLocation, ePaper.Temporal);
        Set(_projectionLocation, new(configuration.Projection.OpticalBlur / 100f, configuration.Projection.Diffusion / 100f, configuration.Projection.ScreenTexture / 100f, configuration.Projection.Convergence / 100f));
    }

    private void Set(int location, System.Numerics.Vector4 value) =>
        _uniform4f(location, value.X, value.Y, value.Z, value.W);

    internal void Stop() => _useProgram(0);
    public void Dispose() => _deleteProgram(_program);

    private static uint Compile(uint stage, string source)
    {
        var createShader = Load<CreateShaderDelegate>("glCreateShader");
        var shaderSource = Load<ShaderSourceDelegate>("glShaderSource");
        var compileShader = Load<CompileShaderDelegate>("glCompileShader");
        var getShader = Load<GetShaderivDelegate>("glGetShaderiv");
        var deleteShader = Load<DeleteShaderDelegate>("glDeleteShader");
        var shader = createShader(stage);
        var bytes = Encoding.UTF8.GetBytes(source);
        var sourcePointer = Marshal.AllocHGlobal(bytes.Length + 1);
        var pointers = Marshal.AllocHGlobal(IntPtr.Size);
        var lengths = Marshal.AllocHGlobal(sizeof(int));
        try
        {
            Marshal.Copy(bytes, 0, sourcePointer, bytes.Length);
            Marshal.WriteByte(sourcePointer, bytes.Length, 0);
            Marshal.WriteIntPtr(pointers, sourcePointer);
            Marshal.WriteInt32(lengths, bytes.Length);
            shaderSource(shader, 1, pointers, lengths);
            compileShader(shader);
            getShader(shader, CompileStatus, out var compiled);
            if (compiled != 0) return shader;
            var error = ShaderLog(shader);
            deleteShader(shader);
            throw new InvalidOperationException(error);
        }
        finally
        {
            Marshal.FreeHGlobal(lengths);
            Marshal.FreeHGlobal(pointers);
            Marshal.FreeHGlobal(sourcePointer);
        }
    }

    private static string ShaderLog(uint shader)
    {
        var log = new StringBuilder(InfoLogCapacity);
        Load<GetShaderInfoLogDelegate>("glGetShaderInfoLog")(shader, log.Capacity, out _, log);
        return log.ToString();
    }

    private static string ProgramLog(uint program)
    {
        var log = new StringBuilder(InfoLogCapacity);
        Load<GetProgramInfoLogDelegate>("glGetProgramInfoLog")(program, log.Capacity, out _, log);
        return log.ToString();
    }

    private static T Load<T>(string name) where T : Delegate
    {
        var address = WglGetProcAddress(name);
        if (address == IntPtr.Zero || address == new IntPtr(1) || address == new IntPtr(2)
            || address == new IntPtr(3) || address == new IntPtr(-1))
            throw new InvalidOperationException(name);
        return Marshal.GetDelegateForFunctionPointer<T>(address);
    }

    [DllImport("opengl32.dll", EntryPoint = "wglGetProcAddress", CharSet = CharSet.Ansi)]
    private static extern IntPtr WglGetProcAddress(string name);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate uint CreateShaderDelegate(uint stage);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate void ShaderSourceDelegate(uint shader, int count, IntPtr strings, IntPtr lengths);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate void CompileShaderDelegate(uint shader);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate void GetShaderivDelegate(uint shader, uint property, out int value);
    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)] private delegate void GetShaderInfoLogDelegate(uint shader, int capacity, out int length, StringBuilder log);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate void DeleteShaderDelegate(uint shader);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate uint CreateProgramDelegate();
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate void AttachShaderDelegate(uint program, uint shader);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate void LinkProgramDelegate(uint program);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate void GetProgramivDelegate(uint program, uint property, out int value);
    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)] private delegate void GetProgramInfoLogDelegate(uint program, int capacity, out int length, StringBuilder log);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate void DeleteProgramDelegate(uint program);
    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)] private delegate int GetUniformLocationDelegate(uint program, string name);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate void Uniform1iDelegate(int location, int value);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate void Uniform4fDelegate(int location, float x, float y, float z, float w);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate void UseProgramDelegate(uint program);

    internal const string VertexSource = """
        #version 120
        varying vec2 TextureCoordinate;
        void main()
        {
            gl_Position = ftransform();
            TextureCoordinate = gl_MultiTexCoord0.xy;
        }
        """;

    private static readonly string FragmentTemplate = """
        #version 120
        uniform sampler2D Source;
        uniform sampler2D History;
        uniform vec4 Adjustments;
        uniform vec4 Processing;
        uniform vec4 Output;
        uniform vec4 CrtDisplay;
        uniform vec4 CrtBeam;
        uniform vec4 CrtOptical;
        uniform vec4 CrtGeometry;
        uniform vec4 CrtScanlines;
        uniform vec4 CrtPattern;
        uniform vec4 CrtPatternIntensity;
        uniform vec4 FixedDisplay;
        uniform vec4 FixedSpatial;
        uniform vec4 FixedTechnology;
        uniform vec4 FixedTemporal;
        uniform vec4 PlasmaEffect;
        uniform vec4 PlasmaTemporal;
        uniform vec4 PlasmaDisplay;
        uniform vec4 VectorEffect;
        uniform vec4 VectorTemporal;
        uniform vec4 VectorDisplay;
        uniform vec4 SegmentGeometry;
        uniform vec4 SegmentShape;
        uniform vec4 SegmentEmission;
        uniform vec4 SegmentOptical;
        uniform vec4 SegmentTemporal;
        uniform vec4 General;
        uniform vec4 Restoration;
        uniform vec4 Temporal;
        uniform vec4 Signal;
        uniform vec4 Signal2;
        uniform vec4 Stylistic;
        uniform vec4 Stylistic2;
        uniform vec4 VfdDisplay;
        uniform vec4 VfdStructure;
        uniform vec4 VfdOptical;
        uniform vec4 LedMatrixEmission;
        uniform vec4 LedMatrixStructure;
        uniform vec4 DotMatrixGeometry;
        uniform vec4 DotMatrixEmission;
        uniform vec4 DotMatrixTemporal;
        uniform vec4 EPaperInkAndColor;
        uniform vec4 EPaperSurface;
        uniform vec4 EPaperTemporal;
        uniform vec4 Projection;
        varying vec2 TextureCoordinate;

        """ + VideoBrightnessParameterFunctions.Shader
        + VideoContrastParameterFunctions.Shader + VideoGammaParameterFunctions.Shader
        + VideoSaturationParameterFunctions.Shader + VideoSharpnessParameterFunctions.Shader
        + FilterBicubic.OpenGlShader + FilterNormal.OpenGlShader
        + FilterBilinear.OpenGlShader + FilterSharpBilinear.OpenGlShader
        + FilterJinc2.OpenGlShader + FilterLanczos.OpenGlShader
        + FilterXbr.OpenGlShader + FilterHqx.OpenGlShader + FilterXbrz.OpenGlShader
        + FilterHq2x.OpenGlShader + FilterHq3x.OpenGlShader + FilterHq4x.OpenGlShader
        + FilterTwoXSai.OpenGlShader + FilterSuperTwoXSai.OpenGlShader
        + FilterSuperEagle.OpenGlShader + FilterEpxScale2x.OpenGlShader
        + FilterScaleFx.OpenGlShader + FilterScaleNx.OpenGlShader + FilterSabr.OpenGlShader
        + """
        vec4 sampleConfigured(vec2 uv)
        {
            int sampling = int(Processing.y + 0.5);
            if (sampling == 0) return pointSample(uv);
            if (sampling == 1) return linearSample(uv);
            if (sampling == 2) return sharpBilinearSample(uv);
            if (sampling == 3) return bicubicSample(uv);
            if (sampling == 4) return xbrSample(uv);
            if (sampling == 5) return xbrzSample(uv);
            if (sampling == 6) return hqxSample(uv);
            if (sampling == 7) return scaleFxSample(uv);
            if (sampling == 8) return scaleNxSample(uv);
            if (sampling == 9) return sabrSample(uv);
            if (sampling == 10) return hq2xSample(uv);
            if (sampling == 11) return hq3xSample(uv);
            if (sampling == 12) return hq4xSample(uv);
            if (sampling == 13) return twoXSaiSample(uv);
            if (sampling == 14) return superTwoXSaiSample(uv);
            if (sampling == 15) return superEagleSample(uv);
            if (sampling == 16) return epxScale2xSample(uv);
            if (sampling == 17) return jinc2Sample(uv);
            if (sampling == 18) return lanczosSample(uv);
            return pointSample(uv);
        }

        float srgbToLinear(float value)
        {
            value = clamp(value, 0.0, 1.0);
            return value <= 0.04045 ? value / 12.92
                : pow((value + 0.055) / 1.055, 2.4);
        }

        float linearToSrgb(float value)
        {
            value = clamp(value, 0.0, 1.0);
            return value <= 0.0031308 ? value * 12.92
                : 1.055 * pow(value, 1.0 / 2.4) - 0.055;
        }

        vec3 adjustColor(vec3 color)
        {
            vec3 linear = vec3(srgbToLinear(color.r), srgbToLinear(color.g), srgbToLinear(color.b));
            linear = videoBrightnessParameter(linear, Adjustments.x);
            linear = videoContrastParameter(linear, Adjustments.y);
            linear = videoGammaParameter(linear, Adjustments.z);
            return videoSaturationParameter(linear, Adjustments.w);
        }

        vec3 crtPalette(vec3 color)
        {
            if (CrtDisplay.x < 0.5 || CrtDisplay.y < 0.5) return color;
            float luminance = dot(color, vec3(0.2126, 0.7152, 0.0722));
            return luminance * vec3(CrtDisplay.zw, CrtBeam.x);
        }

        vec2 crtUv(vec2 uv)
        {
            vec2 normalized = uv * 2.0 - 1.0;
            normalized.x *= 1.0 + CrtGeometry.x * 0.28 * normalized.y * normalized.y
                + CrtGeometry.z * 0.22 * normalized.y;
            normalized.y *= 1.0 + CrtGeometry.y * 0.28 * normalized.x * normalized.x;
            return (normalized + 1.0) * 0.5;
        }

        vec3 crtBase(vec2 uv)
        {
            vec3 center = adjustColor(sampleConfigured(uv).rgb);
            if (abs(Processing.x) > 0.0001)
            {
                vec2 stepSize = 1.0 / max(Processing.zw, vec2(1.0));
                vec3 average = vec3(0.0);
                for (int y = -1; y <= 1; y++)
                    for (int x = -1; x <= 1; x++)
                        average += adjustColor(sampleConfigured(uv + vec2(x, y) * stepSize).rgb);
                center = videoSharpnessParameter(center, average / 9.0, Processing.x);
            }
            return crtPalette(center);
        }

        """ + "\n" + """
        #if DISPLAY_TECHNOLOGY == 2
        """ + "\n" + FilterFixedPixelSubpixels.Shader + FilterFixedPixelGrid.Shader
        + FilterLcdDisplay.Shader + FilterLedBacklitLcdDisplay.Shader + FilterOledDisplay.Shader
        + FilterFixedPixelResponse.Shader + FilterFixedPixelPersistence.Shader + "\n" + """
        #endif
        #if DISPLAY_TECHNOLOGY == 3
        """ + "\n" + FilterPlasmaCellStructure.Shader + FilterPlasmaTemporalDithering.Shader
        + FilterPlasmaLightDiffusion.Shader + FilterPlasmaPersistence.Shader
        + FilterPlasmaBlackDepth.Shader + FilterPlasmaPhosphorIntensity.Shader
        + FilterPlasmaGammaResponse.Shader
        + FilterPlasmaAutomaticBrightnessLimiter.Shader + "\n" + """
        #endif
        #if DISPLAY_TECHNOLOGY == 4
        """ + "\n" + FilterVectorLineDetection.Shader + FilterVectorLineIntensity.Shader
        + FilterVectorBeamWidth.Shader + FilterVectorBeamFocus.Shader
        + FilterVectorHalo.Shader + FilterVectorHaloRadius.Shader
        + FilterVectorPhosphorColor.Shader + FilterVectorPersistence.Shader + "\n" + """
        #endif
        #if DISPLAY_TECHNOLOGY == 5
        """ + "\n" + FilterVfdEmissionThreshold.Shader + FilterVfdPhosphorIntensity.Shader
        + FilterVfdPhosphorColor.Shader + FilterVfdGlass.Shader
        + FilterVfdCellStructure.Shader + FilterVfdHaloRadius.Shader
        + FilterVfdHalo.Shader + FilterVfdPersistence.Shader + "\n" + """
        #endif
        #if DISPLAY_TECHNOLOGY == 6
        """ + "\n" + FilterLedMatrixCellStructure.Shader
        + FilterLedMatrixColor.Shader + FilterLedMatrixBrightness.Shader
        + FilterLedMatrixHalo.Shader + FilterLedMatrixBlackDepth.Shader + "\n" + """
        #endif
        #if DISPLAY_TECHNOLOGY == 7
        """ + "\n" + FilterDotMatrixCellSize.Shader + FilterDotMatrixCellGap.Shader
        + FilterDotMatrixShape.Shader + FilterDotMatrixDotSize.Shader + FilterDotMatrixContrast.Shader
        + FilterDotMatrixBrightness.Shader + FilterDotMatrixPalette.Shader
        + FilterDotMatrixHalo.Shader + FilterDotMatrixResponse.Shader
        + FilterDotMatrixPersistence.Shader + "\n" + """
        #endif
        #if DISPLAY_TECHNOLOGY == 8
        """ + "\n" + FilterSegmentDisplay.OpenGlShader + "\n" + """
        #endif
        #if DISPLAY_TECHNOLOGY == 9
        """ + "\n" + FilterEPaper.OpenGlShader + "\n" + """
        #endif
        #if DISPLAY_TECHNOLOGY == 2
        vec3 fixedPixel(vec3 color, vec2 uv)
        {
            vec2 fraction=fract(uv*Processing.zw);
            color=filterFixedPixelSubpixels(color,fraction,FixedDisplay.z,FixedSpatial.yzw,
                FixedDisplay.w,Output.x/max(Processing.z,1.0));
            color=filterFixedPixelGrid(color,fraction,FixedDisplay.w,FixedSpatial.x,
                Output.xy/max(Processing.zw,vec2(1.0)));
            int technology=int(FixedDisplay.y+.5);
            if(technology<2)
            {
                vec2 stepSize=1.0/max(Processing.zw,vec2(1.0));
                vec3 neighbour=(adjustColor(sampleConfigured(uv-vec2(stepSize.x,0.0)).rgb)
                    +adjustColor(sampleConfigured(uv+vec2(stepSize.x,0.0)).rgb)
                    +adjustColor(sampleConfigured(uv-vec2(0.0,stepSize.y)).rgb)
                    +adjustColor(sampleConfigured(uv+vec2(0.0,stepSize.y)).rgb))*.25;
                float light=max(neighbour.r,max(neighbour.g,neighbour.b));
                color=technology==0
                    ?filterLcdDisplay(color,FixedTechnology.x,FixedTechnology.y,FixedTechnology.z,light)
                    :filterLedBacklitLcdDisplay(color,FixedTechnology.x,FixedTechnology.y,FixedTechnology.z,light);
            }
            else color=filterOledDisplay(color,FixedTechnology.y);
            return clamp(color,0.0,1.0);
        }

        vec3 fixedPixelWithHistory(vec3 color, vec2 uv)
        {
            color=fixedPixel(color,uv);
            if(FixedTemporal.z<.5)return color;
            vec3 previous=texture2D(History,
                clamp(uv,.5/Processing.zw,1.0-.5/Processing.zw)).rgb;
            previous=fixedPixel(adjustColor(previous),uv);
            color=filterFixedPixelResponse(previous,color,FixedTemporal.x,FixedTemporal.w);
            return filterFixedPixelPersistence(color,previous,FixedTemporal.y);
        }
        #endif
        #if DISPLAY_TECHNOLOGY == 3
        vec3 plasmaBase(vec2 uv)
        {
            vec3 color=adjustColor(sampleConfigured(uv).rgb);
            color=filterPlasmaBlackDepth(color,PlasmaDisplay.x);
            color=filterPlasmaGammaResponse(color,PlasmaDisplay.z);
            color=filterPlasmaPhosphorIntensity(color,PlasmaDisplay.y);
            color=filterPlasmaAutomaticBrightnessLimiter(color,PlasmaDisplay.w,
                PlasmaTemporal.w);
            color=filterPlasmaCellStructure(color,fract(uv*Processing.zw),
                PlasmaEffect.y,Output.xy/max(Processing.zw,vec2(1.0)));
            return filterPlasmaTemporalDithering(color,floor(uv*Output.xy),
                PlasmaEffect.w,PlasmaTemporal.z);
        }

        vec3 plasmaPixel(vec2 uv)
        {
            vec3 color = plasmaBase(uv);
            if (PlasmaEffect.z > 0.0)
            {
                vec2 stepSize=1.0/max(Processing.zw,vec2(1.0));
                vec3 nearLight=(plasmaBase(uv-vec2(stepSize.x,0.0))
                    +plasmaBase(uv+vec2(stepSize.x,0.0))
                    +plasmaBase(uv-vec2(0.0,stepSize.y))
                    +plasmaBase(uv+vec2(0.0,stepSize.y)))*.25;
                vec3 farLight=(plasmaBase(uv-vec2(stepSize.x*2.0,0.0))
                    +plasmaBase(uv+vec2(stepSize.x*2.0,0.0))
                    +plasmaBase(uv-vec2(0.0,stepSize.y*2.0))
                    +plasmaBase(uv+vec2(0.0,stepSize.y*2.0)))*.25;
                color=filterPlasmaLightDiffusion(color,nearLight,farLight,PlasmaEffect.z);
            }
            if (PlasmaTemporal.y > 0.5)
            {
                vec3 previous = texture2D(History,
                    clamp(uv, 0.5 / Processing.zw, 1.0 - 0.5 / Processing.zw)).rgb;
                previous=filterPlasmaCellStructure(adjustColor(previous),
                    fract(uv*Processing.zw),PlasmaEffect.y,
                    Output.xy/max(Processing.zw,vec2(1.0)));
                color=filterPlasmaPersistence(color,previous,PlasmaTemporal.x);
            }
            return clamp(color, 0.0, 1.0);
        }
        #endif
        #if DISPLAY_TECHNOLOGY == 4
        float vectorLuminance(vec2 uv)
        {
            return dot(adjustColor(sampleConfigured(uv).rgb), vec3(0.2126, 0.7152, 0.0722));
        }

        float vectorEmission(vec2 uv)
        {
            vec2 stepSize = 1.0 / max(Processing.zw, vec2(1.0));
            float topLeft = vectorLuminance(uv + vec2(-1.0, -1.0) * stepSize);
            float top = vectorLuminance(uv + vec2(0.0, -1.0) * stepSize);
            float topRight = vectorLuminance(uv + vec2(1.0, -1.0) * stepSize);
            float left = vectorLuminance(uv + vec2(-1.0, 0.0) * stepSize);
            float right = vectorLuminance(uv + vec2(1.0, 0.0) * stepSize);
            float bottomLeft = vectorLuminance(uv + vec2(-1.0, 1.0) * stepSize);
            float bottom = vectorLuminance(uv + vec2(0.0, 1.0) * stepSize);
            float bottomRight = vectorLuminance(uv + vec2(1.0, 1.0) * stepSize);
            float gradientX = -topLeft - 2.0 * left - bottomLeft
                + topRight + 2.0 * right + bottomRight;
            float gradientY = -topLeft - 2.0 * top - topRight
                + bottomLeft + 2.0 * bottom + bottomRight;
            return filterVectorLineDetection(gradientX,gradientY,VectorEffect.y);
        }

        vec3 vectorPixel(vec2 uv)
        {
            vec3 color = adjustColor(sampleConfigured(uv).rgb);
            vec2 sourceStep=1.0/max(Processing.zw,vec2(1.0));
            float center=vectorEmission(uv);
            float nearEmission=max(max(vectorEmission(uv+vec2(sourceStep.x,0.0)),
                vectorEmission(uv-vec2(sourceStep.x,0.0))),max(
                vectorEmission(uv+vec2(0.0,sourceStep.y)),
                vectorEmission(uv-vec2(0.0,sourceStep.y))));
            float farEmission=max(max(vectorEmission(uv+vec2(sourceStep.x*2.0,0.0)),
                vectorEmission(uv-vec2(sourceStep.x*2.0,0.0))),max(
                vectorEmission(uv+vec2(0.0,sourceStep.y*2.0)),
                vectorEmission(uv-vec2(0.0,sourceStep.y*2.0))));
            float emission=filterVectorBeamWidth(center,nearEmission,farEmission,VectorDisplay.x);
            emission=filterVectorBeamFocus(emission,(center+nearEmission*4.0)/5.0,VectorDisplay.y);
            color=filterVectorLineIntensity(color,emission,VectorEffect.z);
            if (VectorEffect.w > 0.0 && VectorEffect.z > 0.0)
            {
                float radius=filterVectorHaloRadius(VectorDisplay.w);
                vec2 stepSize=sourceStep*radius;
                float average=0.0;
                for (int y = -1; y <= 1; y++)
                    for (int x = -1; x <= 1; x++)
                        average += vectorEmission(uv + vec2(float(x), float(y)) * stepSize);
                color=filterVectorHalo(color,average/9.0,VectorEffect.z,VectorEffect.w);
            }
            color=filterVectorPhosphorColor(color,VectorDisplay.z);
            if (VectorTemporal.y > 0.5)
            {
                vec3 previous = texture2D(History,
                    clamp(uv, 0.5 / Processing.zw, 1.0 - 0.5 / Processing.zw)).rgb;
                color=filterVectorPersistence(color,adjustColor(previous),VectorTemporal.x);
            }
            return clamp(color, 0.0, 1.0);
        }
        #endif
        vec3 crtPixel(vec2 originalUv)
        {
            if (CrtDisplay.x < 0.5)
            {
                vec3 color = adjustColor(sampleConfigured(originalUv).rgb);
        #if DISPLAY_TECHNOLOGY == 2
                if (FixedDisplay.x > 0.5) return fixedPixelWithHistory(color, originalUv);
        #endif
        #if DISPLAY_TECHNOLOGY == 3
                if (PlasmaEffect.x > 0.5) return plasmaPixel(originalUv);
        #endif
        #if DISPLAY_TECHNOLOGY == 4
                if (VectorEffect.x > 0.5) return vectorPixel(originalUv);
        #endif
                return color;
            }
            vec2 uv = crtUv(originalUv);
            if (any(lessThan(uv, vec2(0.0))) || any(greaterThan(uv, vec2(1.0)))) return vec3(0.0);
            vec2 stepSize = 1.0 / max(Output.xy, vec2(1.0));
            vec3 source = crtBase(uv);
            vec3 vertical = (crtBase(uv - vec2(0.0, stepSize.y))
                + crtBase(uv + vec2(0.0, stepSize.y))) * 0.5;
            vec3 neighborhood = vec3(0.0);
            for (int y = -1; y <= 1; y++)
                for (int x = -1; x <= 1; x++)
                    neighborhood += crtBase(uv + vec2(float(x), float(y)) * stepSize);
            neighborhood /= 9.0;
            vec3 color = mix(source, vertical, CrtBeam.y * 0.45);
            color = mix(color, neighborhood, CrtBeam.w * 0.72);
            color = clamp(color * (1.0 + CrtBeam.z * 0.5)
                + max(neighborhood-vec3(.35),vec3(0.0)) * CrtOptical.x * 0.85, 0.0, 1.0);

            vec2 pixel = floor(originalUv * Processing.zw);
            int mask = int(CrtOptical.y + 0.5);
            if (mask != 0 && CrtOptical.w > 0.0)
            {
                int subpixelLayout = int(CrtOptical.z + 0.5);
                int selected = subpixelLayout == 0 ? -1 : int(mod(pixel.x, 3.0));
                if (subpixelLayout == 2) selected = 2 - selected;
                if (mask == 2) selected = int(mod(float(selected) + mod(pixel.y, 2.0), 3.0));
                bool slotGap = mask == 3 && int(mod(pixel.y, 4.0)) == 3;
                float strength = CrtOptical.w * 0.88;
                for (int channel = 0; channel < 3; channel++)
                {
                    float attenuation = slotGap || (selected >= 0 && channel != selected)
                        ? strength : strength * 0.18;
                    if (subpixelLayout == 0) attenuation = int(mod(pixel.x + pixel.y, 2.0)) == 0
                        ? strength * 0.18 : strength;
                    color[channel] *= 1.0 - attenuation;
                }
            }

            if (CrtPatternIntensity.y > 0.5 && CrtScanlines.x > 0.0)
            {
                float coordinate = CrtPatternIntensity.z < 0.5
                    ? originalUv.y * Processing.w : originalUv.x * Processing.z;
                float gapStart = mix(.47,.18,CrtScanlines.y);
                float cycle=fract((coordinate+CrtScanlines.z*.25)*.5);
                float distanceFromBeam=min(abs(cycle-.25),1.0-abs(cycle-.25));
                float gap=smoothstep(gapStart,min(.5,gapStart+.055),distanceFromBeam);
                float coverage = 1.0-gapStart*2.0;
                float compensation = 1.0+CrtScanlines.w*CrtScanlines.x*coverage*.45;
                color *= (1.0-CrtScanlines.x*gap*.94)*compensation;
            }

            if (CrtPattern.x > 0.5 && CrtPatternIntensity.x > 0.0)
            {
                float coordinate = CrtPattern.y < 0.5 ? pixel.y : pixel.x;
                float axisLength = CrtPattern.y < 0.5 ? Processing.w : Processing.z;
                float cycles = 1.0 + CrtPattern.z * 31.0;
                float wave = 0.5 + 0.5 * cos(6.2831853 * (coordinate + 0.5)
                    * cycles / axisLength + CrtPattern.w * 6.2831853);
                color *= 1.0 - CrtPatternIntensity.x * 0.85 * wave;
            }

            vec2 normalized = originalUv * 2.0 - 1.0;
            float radius = clamp(dot(normalized, normalized) * 0.5, 0.0, 1.0);
            color *= 1.0 - CrtGeometry.w * 0.92 * pow(radius, 1.5);
            return clamp(color, 0.0, 1.0);
        }

        float extraHash(vec2 p){return fract(sin(dot(p,vec2(12.9898,78.233))+General.z)*43758.5453);}
        vec3 extraRaw(vec2 uv){return adjustColor(sampleConfigured(uv).rgb);}
        float restorationDistance(vec3 a,vec3 b){vec3 d=a-b;return length(d);}
        float restorationLuminance(vec3 c){return dot(c,vec3(.2126,.7152,.0722));}
        vec3 restoreColor(vec3 color,vec2 uv)
        {
            vec2 s=1.0/max(Output.xy,vec2(1.0));
            vec3 l=extraRaw(uv-vec2(s.x,0.0)),r=extraRaw(uv+vec2(s.x,0.0));
            vec3 u=extraRaw(uv-vec2(0.0,s.y)),d=extraRaw(uv+vec2(0.0,s.y));
            vec3 ul=extraRaw(uv-s),ur=extraRaw(uv+vec2(s.x,-s.y));
            vec3 dl=extraRaw(uv+vec2(-s.x,s.y)),dr=extraRaw(uv+s);
            vec3 axial=(l+r+u+d)*.25;

            float dedither=Restoration.x;
            if(dedither>0.0)
            {
                int diagonalMatches=0;
                if(restorationDistance(color,ul)<=.025)diagonalMatches++;
                if(restorationDistance(color,ur)<=.025)diagonalMatches++;
                if(restorationDistance(color,dl)<=.025)diagonalMatches++;
                if(restorationDistance(color,dr)<=.025)diagonalMatches++;
                int axialMatches=0;
                if(restorationDistance(axial,l)<=.025)axialMatches++;
                if(restorationDistance(axial,r)<=.025)axialMatches++;
                if(restorationDistance(axial,u)<=.025)axialMatches++;
                if(restorationDistance(axial,d)<=.025)axialMatches++;
                float patternContrast=restorationDistance(color,axial);
                if(diagonalMatches>=3&&axialMatches>=3&&patternContrast>=.015&&patternContrast<=.45)
                    color=mix(color,(color+axial)*.5,dedither);
            }

            float denoise=Restoration.y;
            if(denoise>0.0)
            {
                float sigma=.02+.16*denoise,iv=1.0/(2.0*sigma*sigma);
                float wl=2.0*exp(-dot(color-l,color-l)*iv),wr=2.0*exp(-dot(color-r,color-r)*iv);
                float wu=2.0*exp(-dot(color-u,color-u)*iv),wd=2.0*exp(-dot(color-d,color-d)*iv);
                float wul=exp(-dot(color-ul,color-ul)*iv),wur=exp(-dot(color-ur,color-ur)*iv);
                float wdl=exp(-dot(color-dl,color-dl)*iv),wdr=exp(-dot(color-dr,color-dr)*iv);
                float weight=4.0+wl+wr+wu+wd+wul+wur+wdl+wdr;
                vec3 filtered=(color*4.0+l*wl+r*wr+u*wu+d*wd+ul*wul+ur*wur+dl*wdl+dr*wdr)/weight;
                color=mix(color,filtered,denoise);
            }

            float deband=Restoration.z;
            if(deband>0.0)
            {
                float threshold=.01+.05*deband;
                float hs=max(restorationDistance(l,color),restorationDistance(r,color));
                float vs=max(restorationDistance(u,color),restorationDistance(d,color));
                bool hv=hs>.0005&&hs<=threshold&&(restorationLuminance(l)-restorationLuminance(color))*(restorationLuminance(r)-restorationLuminance(color))<=.000001;
                bool vv=vs>.0005&&vs<=threshold&&(restorationLuminance(u)-restorationLuminance(color))*(restorationLuminance(d)-restorationLuminance(color))<=.000001;
                if(hv||vv) color=mix(color,hv&&(!vv||hs<=vs)?(l+color+r)/3.0:(u+color+d)/3.0,deband);
            }

            float details=Stylistic2.z;
            if(details>0.0)
            {
                vec3 average=(l+r+u+d+ul+ur+dl+dr)/8.0;
                float localContrast=max(max(max(restorationDistance(color,l),restorationDistance(color,r)),max(restorationDistance(color,u),restorationDistance(color,d))),max(max(restorationDistance(color,ul),restorationDistance(color,ur)),max(restorationDistance(color,dl),restorationDistance(color,dr))));
                float amount=details*clamp((.35-localContrast)/.30,0.0,1.0);
                vec3 minimum=min(color,min(min(l,r),min(u,d))),maximum=max(color,max(max(l,r),max(u,d)));
                vec3 extension=vec3(localContrast*.25*details);
                color=clamp(color+(color-average)*amount,minimum-extension,maximum+extension);
            }
            if(int(Restoration.w+.5)==3)color=mix(color,(u+d)*.5,.5);
            return clamp(color,0.0,1.0);
        }
        #if DISPLAY_TECHNOLOGY == 5
        float vfdEmission(vec2 uv)
        {
            vec3 sampleColor=adjustColor(sampleConfigured(clamp(uv,vec2(0.0),vec2(1.0))).rgb);
            return filterVfdEmissionThreshold(dot(sampleColor,vec3(.2126,.7152,.0722)),VfdDisplay.z);
        }
        vec3 vfdPixel(vec3 source,vec2 uv)
        {
            float mask=filterVfdCellStructure(uv,Processing.zw,VfdStructure.x,
                VfdStructure.y,VfdStructure.z);
            float emission=filterVfdPhosphorIntensity(
                filterVfdEmissionThreshold(dot(source,vec3(.2126,.7152,.0722)),VfdDisplay.z)*mask,
                VfdDisplay.y);
            vec2 stepSize=1.0/max(Processing.zw,vec2(1.0));
            float radius=filterVfdHaloRadius(VfdOptical.x);
            float nearRadius=max(1.0,radius*.5);
            float nearEmission=(vfdEmission(uv+vec2(stepSize.x*nearRadius,0.0))
                +vfdEmission(uv-vec2(stepSize.x*nearRadius,0.0))
                +vfdEmission(uv+vec2(0.0,stepSize.y*nearRadius))
                +vfdEmission(uv-vec2(0.0,stepSize.y*nearRadius)))*.25;
            float farEmission=(vfdEmission(uv+vec2(stepSize.x*radius,stepSize.y*radius))
                +vfdEmission(uv+vec2(-stepSize.x*radius,stepSize.y*radius))
                +vfdEmission(uv+vec2(stepSize.x*radius,-stepSize.y*radius))
                +vfdEmission(uv-vec2(stepSize.x*radius,stepSize.y*radius)))*.25;
            float halo=filterVfdPhosphorIntensity(
                filterVfdHalo(nearEmission,farEmission,VfdStructure.w),VfdDisplay.y);
            return clamp(filterVfdGlass(source,VfdDisplay.w)
                +filterVfdPhosphorColor(emission,VfdDisplay.x)
                +filterVfdPhosphorColor(halo,VfdDisplay.x),0.0,1.0);
        }
        #endif
        #if DISPLAY_TECHNOLOGY == 6
        vec3 ledMatrixPixel(vec2 uv)
        {
            vec2 sourceSize=max(Processing.zw,vec2(1.0));
            float pitch=filterLedMatrixPitch(LedMatrixStructure.x);
            vec2 sourcePosition=uv*sourceSize;
            vec2 cell=floor(sourcePosition/pitch);
            vec2 centerUv=(cell+.5)*pitch/sourceSize;
            vec2 sampleOffset=vec2(pitch*.28)/sourceSize;
            vec3 emission=(adjustColor(sampleConfigured(centerUv).rgb)*4.0
                +adjustColor(sampleConfigured(centerUv-vec2(sampleOffset.x,0.0)).rgb)
                +adjustColor(sampleConfigured(centerUv+vec2(sampleOffset.x,0.0)).rgb)
                +adjustColor(sampleConfigured(centerUv-vec2(0.0,sampleOffset.y)).rgb)
                +adjustColor(sampleConfigured(centerUv+vec2(0.0,sampleOffset.y)).rgb))/8.0;
            emission=filterLedMatrixColor(emission,LedMatrixEmission.x);
            emission=filterLedMatrixBrightness(emission,LedMatrixEmission.y);
            vec2 localPosition=fract(sourcePosition/pitch)-.5;
            float distance=filterLedMatrixDistance(localPosition,LedMatrixEmission.w);
            float edgeWidth=max(sourceSize.x/max(Output.x,1.0),
                sourceSize.y/max(Output.y,1.0))/pitch;
            float core=filterLedMatrixCore(distance,LedMatrixStructure.y,edgeWidth);
            float halo=filterLedMatrixHalo(distance,LedMatrixStructure.y,
                LedMatrixStructure.w,LedMatrixStructure.z)*(1.0-core);
            return clamp(filterLedMatrixBlackDepth(LedMatrixEmission.z)
                +emission*(core+halo),0.0,1.0);
        }
        #endif
        #if DISPLAY_TECHNOLOGY == 7
        vec3 dotMatrixSample(vec2 uv,bool history)
        {
            vec2 sourceSize=max(Processing.zw,vec2(1.0));
            float pitch=filterDotMatrixPitch(DotMatrixGeometry.z);
            vec2 cell=floor(uv*sourceSize/pitch);
            vec2 centerUv=(cell+.5)*pitch/sourceSize;
            vec2 offset=vec2(pitch*.28)/sourceSize;
            if(history)
                return (adjustColor(texture2D(History,centerUv).rgb)*4.0
                    +adjustColor(texture2D(History,centerUv-vec2(offset.x,0.0)).rgb)
                    +adjustColor(texture2D(History,centerUv+vec2(offset.x,0.0)).rgb)
                    +adjustColor(texture2D(History,centerUv-vec2(0.0,offset.y)).rgb)
                    +adjustColor(texture2D(History,centerUv+vec2(0.0,offset.y)).rgb))/8.0;
            return (adjustColor(sampleConfigured(centerUv).rgb)*4.0
                +adjustColor(sampleConfigured(centerUv-vec2(offset.x,0.0)).rgb)
                +adjustColor(sampleConfigured(centerUv+vec2(offset.x,0.0)).rgb)
                +adjustColor(sampleConfigured(centerUv-vec2(0.0,offset.y)).rgb)
                +adjustColor(sampleConfigured(centerUv+vec2(0.0,offset.y)).rgb))/8.0;
        }
        vec3 dotMatrixPixel(vec2 uv,bool history)
        {
            vec2 sourceSize=max(Processing.zw,vec2(1.0));
            float pitch=filterDotMatrixPitch(DotMatrixGeometry.z);
            vec2 local=fract(uv*sourceSize/pitch)-.5;
            float distance=filterDotMatrixDistance(local,DotMatrixGeometry.y);
            float radius=filterDotMatrixRadius(DotMatrixGeometry.w,DotMatrixEmission.x);
            float edge=max(sourceSize.x/max(Output.x,1.0),sourceSize.y/max(Output.y,1.0))/pitch;
            float core=smoothstep(radius+edge,radius-edge,distance);
            float halo=filterDotMatrixHalo(distance,radius,DotMatrixEmission.w)*(1.0-core);
            vec3 source=dotMatrixSample(uv,history);
            float level=filterDotMatrixBrightness(filterDotMatrixContrast(
                dot(source,vec3(.2126,.7152,.0722)),DotMatrixEmission.y),DotMatrixEmission.z);
            return mix(filterDotMatrixBackground(DotMatrixGeometry.x),
                filterDotMatrixForeground(source,DotMatrixGeometry.x),
                clamp(level*(core+halo),0.0,1.0));
        }
        #endif
        vec3 extraDisplay(vec3 color,vec2 uv){int t=int(General.x+.5);vec2 p=floor(uv*Output.xy);
        #if DISPLAY_TECHNOLOGY == 5
            if(t==5)color=vfdPixel(color,uv);
        #endif
        #if DISPLAY_TECHNOLOGY == 6
            if(t==6)color=ledMatrixPixel(uv);
        #endif
        #if DISPLAY_TECHNOLOGY == 7
            if(t==7)color=dotMatrixPixel(uv,false);
        #endif
        #if DISPLAY_TECHNOLOGY == 8
            if(t==8)color=segmentDisplayPixel(uv);
        #endif
        #if DISPLAY_TECHNOLOGY == 9
            if(t==9)color=ePaperPixel(uv);
        #endif
        #if DISPLAY_TECHNOLOGY == 10
            if(t==10){vec2 s=1.0/max(Output.xy,vec2(1.0));color=mix(color,(extraRaw(uv-s)+extraRaw(uv+s))*.5,Projection.x*.55+Projection.y*.25);color*=1.0-(extraHash(p)-.5)*Projection.z*.12;}
        #endif
            return clamp(color,0.0,1.0);}
        """ + SignalConnectionRgbScart.Shader + SignalConnectionComponent.Shader
        + SignalConnectionSVideo.Shader + SignalConnectionComposite.Shader
        + SignalConnectionRf.Shader + SignalStandardPal.Shader + SignalStandardNtsc.Shader
        + SignalStandardSecam.Shader + FilterGrain.Shader + FilterVhs.Shader
        + FilterChromaticAberration.Shader + FilterBloom.Shader + FilterSepia.Shader + """
        vec3 signalEffects(vec3 color,vec2 uv)
        {
            vec2 stepSize=1.0/max(Processing.zw,vec2(1.0));
            vec3 left=extraRaw(uv-vec2(stepSize.x,0.0)),right=extraRaw(uv+vec2(stepSize.x,0.0));
            vec3 up=extraRaw(uv-vec2(0.0,stepSize.y)),down=extraRaw(uv+vec2(0.0,stepSize.y));
            int connection=int(Signal.x+.5);float amount=Signal.y;
            int standard=int(Signal.z+.5);if(standard==0)standard=General.w>18.2?1:2;
            float phase=mod(floor(uv.x*Processing.z)+floor(uv.y*Processing.w)+General.z,2.0)*2.0-1.0;
            if(connection==1)color=signalConnectionRgbScart(color,left,amount);
            else if(connection==2)color=signalConnectionComponent(color,left,right,amount);
            else if(connection==3)color=signalConnectionSVideo(color,left,right,amount);
            else if(connection==4)color=signalConnectionComposite(color,left,right,amount,phase);
            else if(connection==5)color=signalConnectionRf(color,left,right,amount,extraHash(floor(uv*Output.xy))-.5,float(standard),floor(uv.y*Processing.w));
            if(standard==1)color=signalStandardPal(color,mod(floor(uv.y*Processing.w),2.0)<.5?down:up,Signal.w);
            else if(standard==2)color=signalStandardNtsc(color,left,Signal.w);
            else if(standard==3)color=signalStandardSecam(color,up,Signal.w,floor(uv.y*Processing.w));
            return color;
        }
        vec3 postColor(vec3 color,vec2 uv)
        {
            color=signalEffects(color,uv);vec2 s=1.0/max(Output.xy,vec2(1.0));
            if(Stylistic.y>0.0){float n=extraHash(floor(uv*Output.xy)+General.z)-.5;float w=(sin(uv.y*Processing.w*.071+General.z*.31)*4.0+n*4.0)*Stylistic.y;vec2 q=uv+vec2(w*s.x,0.0);color=filterVhs(color,extraRaw(q),extraRaw(q-vec2(s.x*2.0,0.0)),extraRaw(q+vec2(s.x*2.0,0.0)),Stylistic.y,n,floor(uv.y*Processing.w),uv.y);}
            if(Stylistic.z>0.0){float o=Stylistic.z*s.x*7.0;color=filterChromaticAberration(extraRaw(uv+vec2(o,0.0)),color,extraRaw(uv-vec2(o,0.0)));}
            if(Stylistic.w>0.0)color=filterBloom(color,extraRaw(uv+vec2(s.x*2.0,0.0)),extraRaw(uv-vec2(s.x*2.0,0.0)),extraRaw(uv+vec2(0.0,s.y*2.0)),extraRaw(uv-vec2(0.0,s.y*2.0)),extraRaw(uv+vec2(s.x*5.0,0.0)),extraRaw(uv-vec2(s.x*5.0,0.0)),extraRaw(uv+vec2(0.0,s.y*5.0)),extraRaw(uv-vec2(0.0,s.y*5.0)),Stylistic.w);
            color=filterSepia(color,Stylistic2.x);
            color=filterGrain(color,Stylistic.x,extraHash(floor(uv*Output.xy)+General.z)*2.0-1.0);
            return clamp(color,0.0,1.0);
        }
        """ + FilterGeneralPersistence.Shader + FilterMotionBlur.Shader
        + FilterFlicker.Shader + FilterInterlacing.Shader
        + FilterBlackFrameInsertion.Shader + """
        void main()
        {
            vec3 center=extraDisplay(restoreColor(crtPixel(TextureCoordinate),TextureCoordinate),TextureCoordinate);
            center=postColor(center,TextureCoordinate);
            vec2 historyUv=clamp(TextureCoordinate,.5/Processing.zw,1.0-.5/Processing.zw);
            vec3 previous=extraDisplay(adjustColor(texture2D(History,historyUv).rgb),TextureCoordinate);
            previous=postColor(previous,TextureCoordinate);
        #if DISPLAY_TECHNOLOGY == 7
            if(DotMatrixTemporal.z>.5)
            {
                previous=postColor(dotMatrixPixel(TextureCoordinate,true),TextureCoordinate);
                center=filterDotMatrixResponse(previous,center,DotMatrixTemporal.x);
                center=filterDotMatrixPersistence(center,previous,DotMatrixTemporal.y,
                    DotMatrixGeometry.x,filterDotMatrixBackground(DotMatrixGeometry.x));
            }
        #endif
        #if DISPLAY_TECHNOLOGY == 5
            if(VfdOptical.w>.5)
                center=filterVfdPersistence(center,previous,VfdOptical.y,VfdOptical.z);
        #endif
            center=filterInterlacing(center,previous,TextureCoordinate,Processing.w,General.z,Temporal.w,Signal2.z,General.y);
            center=filterFlicker(center,General.z,Temporal.z);
            if(General.y>.5)
            {
                previous=filterFlicker(previous,General.z-1.0,Temporal.z);
                center=filterMotionBlur(center,previous,Temporal.y);
                center=filterGeneralPersistence(center,previous,Temporal.x);
            }
            center=filterBlackFrameInsertion(center,General.z,Signal2.y);
            gl_FragColor=vec4(linearToSrgb(center.r),linearToSrgb(center.g),linearToSrgb(center.b),1.0);
        }
        """;

    internal static string Fragment(EmulationVideoSampling sampling,
        EmulationVideoDisplayTechnology displayTechnology) =>
        FragmentTemplate.Replace("#version 120", $"#version 120\n#define DISPLAY_TECHNOLOGY {(int)displayTechnology}");
}
