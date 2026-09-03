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
    private readonly int _vectorEffectLocation;
    private readonly int _vectorTemporalLocation;
    private readonly int _generalLocation;
    private readonly int _restorationLocation;
    private readonly int _temporalLocation;
    private readonly int _signalLocation;
    private readonly int _signal2Location;
    private readonly int _stylisticLocation;
    private readonly int _stylistic2Location;
    private readonly int _vfdLocation;
    private readonly int _ledMatrixLocation;
    private readonly int _dotMatrixLocation;
    private readonly int _ePaperLocation;
    private readonly int _projectionLocation;

    internal OpenGlVideoProcessingProgram()
    {
        uint vertex = 0;
        uint fragment = 0;
        uint program = 0;
        try
        {
            vertex = Compile(VertexShader, VertexSource);
            fragment = Compile(FragmentShader, FragmentSource);
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
        _vectorEffectLocation = _getUniformLocation(_program, "VectorEffect");
        _vectorTemporalLocation = _getUniformLocation(_program, "VectorTemporal");
        _generalLocation = _getUniformLocation(_program, "General");
        _restorationLocation = _getUniformLocation(_program, "Restoration");
        _temporalLocation = _getUniformLocation(_program, "Temporal");
        _signalLocation = _getUniformLocation(_program, "Signal");
        _signal2Location = _getUniformLocation(_program, "Signal2");
        _stylisticLocation = _getUniformLocation(_program, "Stylistic");
        _stylistic2Location = _getUniformLocation(_program, "Stylistic2");
        _vfdLocation = _getUniformLocation(_program, "Vfd");
        _ledMatrixLocation = _getUniformLocation(_program, "LedMatrix");
        _dotMatrixLocation = _getUniformLocation(_program, "DotMatrix");
        _ePaperLocation = _getUniformLocation(_program, "EPaper");
        _projectionLocation = _getUniformLocation(_program, "Projection");
        _useProgram(_program);
        _uniform1i(_sourceLocation, 0);
        _uniform1i(_historyLocation, 1);
        _useProgram(0);
    }

    internal void Use(EmulationVideoProcessingConfiguration configuration,
        int sourceWidth, int sourceHeight, int outputWidth, int outputHeight,
        bool hasHistory = false, double elapsedMilliseconds = 0, long sequence = 0)
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
        var plasma = PlasmaVideoShaderParameters.From(configuration, hasHistory, sequence);
        Set(_plasmaEffectLocation, plasma.Effect);
        Set(_plasmaTemporalLocation, plasma.Temporal);
        var vector = VectorVideoShaderParameters.From(configuration, hasHistory);
        Set(_vectorEffectLocation, vector.Effect);
        Set(_vectorTemporalLocation, vector.Temporal);
        Set(_generalLocation, new((float)configuration.DisplayTechnology, hasHistory ? 1f : 0f, sequence % 4096, (float)elapsedMilliseconds));
        Set(_restorationLocation, new(configuration.Restoration.Dedithering / 100f, configuration.Restoration.Denoising / 100f, configuration.Restoration.Debanding / 100f, (float)configuration.Restoration.Deinterlacing));
        Set(_temporalLocation, new(configuration.Temporal.GeneralPersistence / 100f, configuration.Temporal.MotionBlur / 100f, configuration.Temporal.Flicker / 100f, configuration.Temporal.Interlacing > 0 ? 1f : 0f));
        Set(_signalLocation, new(configuration.SignalSimulation.Composite / 100f, configuration.SignalSimulation.SVideo / 100f, configuration.SignalSimulation.Rf / 100f, configuration.SignalSimulation.Pal / 100f));
        Set(_signal2Location, new(configuration.SignalSimulation.Ntsc / 100f, configuration.Temporal.BlackFrameInsertion ? 1f : 0f, configuration.Temporal.InterlacingVisibility / 100f, 0f));
        Set(_stylisticLocation, new(configuration.Stylistic.Grain / 100f, configuration.Stylistic.Vhs / 100f, configuration.Stylistic.ChromaticAberration / 100f, configuration.Stylistic.Bloom / 100f));
        Set(_stylistic2Location, new(configuration.Stylistic.Sepia / 100f, configuration.Stylistic.Grayscale / 100f, configuration.Restoration.DetailRecovery / 100f, 0f));
        Set(_vfdLocation, new((float)configuration.Vfd.Color, configuration.Vfd.PhosphorIntensity / 100f, configuration.Vfd.HaloIntensity / 100f, configuration.Vfd.PersistenceIntensity / 100f));
        Set(_ledMatrixLocation, new((float)configuration.LedMatrix.Color, configuration.LedMatrix.CellSize / 100f, Math.Max(configuration.LedMatrix.CellGap, configuration.LedMatrix.Diffusion) / 100f, configuration.LedMatrix.Brightness / 100f));
        Set(_dotMatrixLocation, new((float)configuration.DotMatrix.Palette, (float)configuration.DotMatrix.Shape, configuration.DotMatrix.DotSize / 100f, configuration.DotMatrix.Contrast / 100f));
        Set(_ePaperLocation, new((float)configuration.EPaper.ColorMode, configuration.EPaper.Contrast / 100f, configuration.EPaper.Dithering / 100f, configuration.EPaper.Ghosting / 100f));
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

    internal static readonly string FragmentSource = """
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
        uniform vec4 VectorEffect;
        uniform vec4 VectorTemporal;
        uniform vec4 General;
        uniform vec4 Restoration;
        uniform vec4 Temporal;
        uniform vec4 Signal;
        uniform vec4 Signal2;
        uniform vec4 Stylistic;
        uniform vec4 Stylistic2;
        uniform vec4 Vfd;
        uniform vec4 LedMatrix;
        uniform vec4 DotMatrix;
        uniform vec4 EPaper;
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
            float curvature = CrtGeometry.x * 0.18;
            return (normalized * (1.0 + curvature * normalized.yx * normalized.yx) + 1.0) * 0.5;
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

        vec3 fixedPixel(vec3 color, vec2 uv)
        {
            vec2 sourcePosition = uv * Processing.zw;
            vec2 fraction = fract(sourcePosition);
            int subpixels = int(FixedDisplay.z + 0.5);
            if (subpixels == 0)
            {
                float luminance = dot(color, vec3(0.2126, 0.7152, 0.0722));
                color = luminance * FixedSpatial.yzw;
            }
            else if (FixedDisplay.w > 0.0)
            {
                int selected = int(floor(min(2.0, fraction.x * 3.0)));
                if (subpixels == 2) selected = 2 - selected;
                float attenuation = FixedDisplay.w * 0.35;
                for (int channel = 0; channel < 3; channel++)
                    if (channel != selected) color[channel] *= 1.0 - attenuation;
            }

            float halfGap = FixedSpatial.x * 0.45;
            if (FixedDisplay.w > 0.0 && halfGap > 0.0)
            {
                vec2 distanceToEdge = min(fraction, 1.0 - fraction);
                float edge = min(distanceToEdge.x, distanceToEdge.y);
                if (edge < halfGap)
                    color *= 1.0 - FixedDisplay.w * (1.0 - edge / halfGap);
            }

            if (FixedDisplay.y < 1.5 && FixedTechnology.x >= 0.0)
                color *= 0.5 + FixedTechnology.x * 0.5;
            if (FixedTechnology.y >= 0.0)
            {
                float blackFloor = (1.0 - FixedTechnology.y) * 0.12;
                color = vec3(blackFloor) + color * (1.0 - blackFloor);
            }
            return clamp(color, 0.0, 1.0);
        }

        vec3 fixedPixelWithHistory(vec3 color, vec2 uv)
        {
            color = fixedPixel(color, uv);
            if (FixedTemporal.z < 0.5) return color;
            vec3 previous = texture2D(History,
                clamp(uv, 0.5 / Processing.zw, 1.0 - 0.5 / Processing.zw)).rgb;
            previous = fixedPixel(adjustColor(previous), uv);
            float response = FixedTemporal.x <= 0.0 ? 1.0
                : 1.0 - exp(-FixedTemporal.w / FixedTemporal.x);
            color = mix(previous, color, response);
            return clamp(max(color, previous * FixedTemporal.y), 0.0, 1.0);
        }

        float plasmaBayer(vec2 pixel)
        {
            int x = int(mod(pixel.x + PlasmaTemporal.z, 4.0));
            int y = int(mod(pixel.y + PlasmaTemporal.z, 4.0));
            if (y == 0) return x == 0 ? 0.0 : (x == 1 ? 8.0 : (x == 2 ? 2.0 : 10.0));
            if (y == 1) return x == 0 ? 12.0 : (x == 1 ? 4.0 : (x == 2 ? 14.0 : 6.0));
            if (y == 2) return x == 0 ? 3.0 : (x == 1 ? 11.0 : (x == 2 ? 1.0 : 9.0));
            return x == 0 ? 15.0 : (x == 1 ? 7.0 : (x == 2 ? 13.0 : 5.0));
        }

        vec3 plasmaCellAndDither(vec3 color, vec2 uv)
        {
            vec2 sourcePosition = uv * Processing.zw;
            vec2 fraction = fract(sourcePosition);
            float structure = PlasmaEffect.y;
            if (structure > 0.0)
            {
                int selected = int(floor(min(2.0, fraction.x * 3.0)));
                for (int channel = 0; channel < 3; channel++)
                    if (channel != selected) color[channel] *= 1.0 - structure * 0.35;
                float edge = min(min(fraction.x, 1.0 - fraction.x),
                    min(fraction.y, 1.0 - fraction.y));
                float halfGap = structure * 0.20;
                if (edge < halfGap)
                    color *= 1.0 - structure * 0.5 * (1.0 - edge / halfGap);
            }
            if (PlasmaEffect.w > 0.0)
            {
                vec2 pixel = floor(uv * Output.xy);
                float offset = (plasmaBayer(pixel) - 7.5) / 7.5 * PlasmaEffect.w * 0.04;
                color = clamp(color + vec3(offset), 0.0, 1.0);
            }
            return color;
        }

        vec3 plasmaBase(vec2 uv)
        {
            return plasmaCellAndDither(adjustColor(sampleConfigured(uv).rgb), uv);
        }

        vec3 plasmaPixel(vec2 uv)
        {
            vec3 color = plasmaBase(uv);
            if (PlasmaEffect.z > 0.0)
            {
                vec2 stepSize = 1.0 / max(Output.xy, vec2(1.0));
                vec3 average = vec3(0.0);
                for (int y = -1; y <= 1; y++)
                    for (int x = -1; x <= 1; x++)
                        average += plasmaBase(uv + vec2(float(x), float(y)) * stepSize);
                color = mix(color, average / 9.0, PlasmaEffect.z * 0.5);
            }
            if (PlasmaTemporal.y > 0.5)
            {
                vec3 previous = texture2D(History,
                    clamp(uv, 0.5 / Processing.zw, 1.0 - 0.5 / Processing.zw)).rgb;
                previous = plasmaCellAndDither(adjustColor(previous), uv);
                color = max(color, previous * PlasmaTemporal.x);
            }
            return clamp(color, 0.0, 1.0);
        }

        float vectorLuminance(vec2 uv)
        {
            return dot(adjustColor(sampleConfigured(uv).rgb), vec3(0.2126, 0.7152, 0.0722));
        }

        float vectorEmission(vec2 uv)
        {
            vec2 stepSize = 1.0 / max(Output.xy, vec2(1.0));
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
            float magnitude = clamp(length(vec2(gradientX, gradientY)) / 4.0, 0.0, 1.0);
            return smoothstep(VectorEffect.y, min(1.0, VectorEffect.y + 0.10), magnitude);
        }

        vec3 vectorPixel(vec2 uv)
        {
            vec3 color = adjustColor(sampleConfigured(uv).rgb);
            float line = vectorEmission(uv) * VectorEffect.z;
            color += (vec3(1.0) - color) * line;
            if (VectorEffect.w > 0.0 && VectorEffect.z > 0.0)
            {
                vec2 stepSize = 1.0 / max(Output.xy, vec2(1.0));
                float average = 0.0;
                for (int y = -1; y <= 1; y++)
                    for (int x = -1; x <= 1; x++)
                        average += vectorEmission(uv + vec2(float(x), float(y)) * stepSize);
                color += vec3(average / 9.0 * VectorEffect.z * VectorEffect.w * 0.5);
            }
            if (VectorTemporal.y > 0.5)
            {
                vec3 previous = texture2D(History,
                    clamp(uv, 0.5 / Processing.zw, 1.0 - 0.5 / Processing.zw)).rgb;
                color = max(color, adjustColor(previous) * VectorTemporal.x);
            }
            return clamp(color, 0.0, 1.0);
        }

        vec3 crtPixel(vec2 originalUv)
        {
            if (CrtDisplay.x < 0.5)
            {
                vec3 color = adjustColor(sampleConfigured(originalUv).rgb);
                if (FixedDisplay.x > 0.5) return fixedPixelWithHistory(color, originalUv);
                if (PlasmaEffect.x > 0.5) return plasmaPixel(originalUv);
                if (VectorEffect.x > 0.5) return vectorPixel(originalUv);
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
            color = mix(color, neighborhood, CrtBeam.w * 0.35);
            color = clamp(color * (1.0 + CrtBeam.z * 0.5)
                + neighborhood * CrtOptical.x * 0.5, 0.0, 1.0);

            vec2 pixel = floor(originalUv * Output.xy);
            int mask = int(CrtOptical.y + 0.5);
            if (mask != 0 && CrtOptical.w > 0.0)
            {
                int subpixelLayout = int(CrtOptical.z + 0.5);
                int selected = subpixelLayout == 0 ? -1 : int(mod(pixel.x, 3.0));
                if (subpixelLayout == 2) selected = 2 - selected;
                if (mask == 2) selected = int(mod(float(selected) + mod(pixel.y, 2.0), 3.0));
                bool slotGap = mask == 3 && int(mod(pixel.y, 4.0)) == 3;
                float strength = CrtOptical.w * 0.75;
                for (int channel = 0; channel < 3; channel++)
                {
                    float attenuation = slotGap || (selected >= 0 && channel != selected)
                        ? strength : strength * 0.18;
                    if (subpixelLayout == 0) attenuation = int(mod(pixel.x + pixel.y, 2.0)) == 0
                        ? strength * 0.18 : strength;
                    color[channel] *= 1.0 - attenuation;
                }
            }

            if (CrtGeometry.z > 0.5 && CrtScanlines.x > 0.0)
            {
                float coordinate = CrtGeometry.w < 0.5 ? pixel.y : pixel.x;
                float wave = 0.5 + 0.5 * cos(3.14159265
                    * (coordinate + 0.25 + CrtScanlines.z * 2.0));
                float exponent = mix(8.0, 0.5, CrtScanlines.y);
                float compensation = 1.0 + CrtScanlines.w * CrtScanlines.x * 0.5;
                color *= (1.0 - CrtScanlines.x * pow(wave, exponent)) * compensation;
            }

            if (CrtPattern.x > 0.5 && CrtPatternIntensity.x > 0.0)
            {
                float coordinate = CrtPattern.y < 0.5 ? pixel.y : pixel.x;
                float axisLength = CrtPattern.y < 0.5 ? Output.y : Output.x;
                float cycles = 1.0 + CrtPattern.z * 31.0;
                float wave = 0.5 + 0.5 * cos(6.2831853 * (coordinate + 0.5)
                    * cycles / axisLength + CrtPattern.w * 6.2831853);
                color *= 1.0 - CrtPatternIntensity.x * 0.5 * wave;
            }

            vec2 normalized = originalUv * 2.0 - 1.0;
            float radius = clamp(dot(normalized, normalized) * 0.5, 0.0, 1.0);
            color *= 1.0 - CrtGeometry.y * 0.75 * pow(radius, 1.5);
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
        vec3 extraDisplay(vec3 color,vec2 uv){int t=int(General.x+.5);vec2 p=floor(uv*Output.xy);if(t==5){int x=int(Vfd.x+.5);vec3 k=x==1?vec3(.05,1.0,.12):x==2?vec3(1.0,.45,.02):x==3?vec3(1.0,.04,.02):vec3(.05,.45,1.0);color=k*dot(color,vec3(.2126,.7152,.0722))*(.5+Vfd.y);}else if(t==6){float z=max(2.0,2.0+LedMatrix.y*10.0);vec2 q=fract(p/z)-.5;color*=smoothstep(.5,.5-LedMatrix.z*.35,max(abs(q.x),abs(q.y)))*(.5+LedMatrix.w);}else if(t==7){vec2 q=fract(p/vec2(6.0,8.0))-.5;float d=int(DotMatrix.y+.5)==0?length(q):max(abs(q.x),abs(q.y));color*=smoothstep(.5,.5-DotMatrix.z*.45,d)*(.5+DotMatrix.w);}else if(t==8){vec2 q=fract(p/vec2(8.0,12.0))-.5;float bars=min(abs(q.x),min(abs(q.y),abs(q.x+q.y)*.7));color=vec3(1.0,.05,.02)*dot(color,vec3(.2126,.7152,.0722))*smoothstep(.18,.04,bars);}else if(t==9){float y=dot(color,vec3(.2126,.7152,.0722)),n=int(EPaper.x+.5)==0?1.0:15.0;y=floor(y*n+extraHash(p)*EPaper.z)/max(n,1.0);color=int(EPaper.x+.5)==2?mix(vec3(y),color,.45):vec3(y);color=mix(vec3(.92),color,.4+EPaper.y*.6);}else if(t==10){vec2 s=1.0/max(Output.xy,vec2(1.0));color=mix(color,(extraRaw(uv-s)+extraRaw(uv+s))*.5,Projection.x*.55+Projection.y*.25);color*=1.0-(extraHash(p)-.5)*Projection.z*.12;}return clamp(color,0.0,1.0);}
        vec3 postColor(vec3 color,vec2 uv){vec2 s=1.0/max(Output.xy,vec2(1.0));float q=max(max(Signal.x,Signal.y),Signal.z);vec3 b=extraRaw(uv-vec2(s.x*(1.0+q*3.0),0.0));color=mix(color,vec3(b.r,color.g,b.b),q*.45);color+=vec3((extraHash(floor(uv*Output.xy))-.5)*max(Signal.w,Signal2.x)*.08);if(Stylistic.z>0.0){float o=Stylistic.z*s.x*5.0;color.r=extraRaw(uv+vec2(o,0.0)).r;color.b=extraRaw(uv-vec2(o,0.0)).b;}color+=vec3((extraHash(uv*Output.xy)-.5)*Stylistic.x*.16);color=mix(color,extraRaw(uv+vec2(sin(uv.y*80.0)*s.x*4.0,0.0)),Stylistic.y*.35);color+=extraRaw(uv)*Stylistic.w*.25;float g=dot(color,vec3(.2126,.7152,.0722));color=mix(color,vec3(g),Stylistic2.y);vec3 e=vec3(dot(color,vec3(.393,.769,.189)),dot(color,vec3(.349,.686,.168)),dot(color,vec3(.272,.534,.131)));return clamp(mix(color,e,Stylistic2.x),0.0,1.0);}
        """ + FilterGeneralPersistence.Shader + FilterMotionBlur.Shader
        + FilterFlicker.Shader + FilterInterlacing.Shader
        + FilterBlackFrameInsertion.Shader + """
        void main()
        {
            vec3 center=extraDisplay(restoreColor(crtPixel(TextureCoordinate),TextureCoordinate),TextureCoordinate);
            center=postColor(center,TextureCoordinate);
            center=filterInterlacing(center,TextureCoordinate,Processing.w,General.z,Temporal.w,Signal2.z);
            center=filterFlicker(center,General.z,Temporal.z);
            if(General.y>.5)
            {
                vec2 historyUv=clamp(TextureCoordinate,.5/Processing.zw,1.0-.5/Processing.zw);
                vec3 previous=extraDisplay(adjustColor(texture2D(History,historyUv).rgb),TextureCoordinate);
                previous=postColor(previous,TextureCoordinate);
                previous=filterInterlacing(previous,TextureCoordinate,Processing.w,General.z-1.0,Temporal.w,Signal2.z);
                previous=filterFlicker(previous,General.z-1.0,Temporal.z);
                center=filterMotionBlur(center,previous,Temporal.y);
                center=filterGeneralPersistence(center,previous,Temporal.x);
            }
            center=filterBlackFrameInsertion(center,General.z,Signal2.y);
            gl_FragColor=vec4(linearToSrgb(center.r),linearToSrgb(center.g),linearToSrgb(center.b),1.0);
        }
        """;
}
