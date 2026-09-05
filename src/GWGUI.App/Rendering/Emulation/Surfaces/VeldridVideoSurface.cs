using GWGUI.App.Functions.Rendering.Emulation;
using GWGUI.App.Factories.Rendering.Emulation;
using GWGUI.App.Interfaces.Rendering.Emulation;
using GWGUI.App.Rendering.Emulation.Processing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GWGUI.Emulation;
using Veldrid;
using Veldrid.SPIRV;

namespace GWGUI.App.Rendering.Emulation.Surfaces;

internal sealed class VeldridVideoSurface : HwndHost, IEmulationVideoSurface
{
    private readonly GraphicsBackend _backend;
    private readonly object _deviceGate = new();
    private IntPtr _hwnd;
    private GraphicsDevice? _device;
    private DeviceBuffer? _vertexBuffer;
    private DeviceBuffer? _parameterBuffer;
    private Texture? _texture;
    private TextureView? _textureView;
    private Texture? _historyTexture;
    private TextureView? _historyTextureView;
    private ResourceLayout? _layout;
    private ResourceSet? _set;
    private Pipeline? _pipeline;
    private CommandList? _commands;
    private Shader[]? _shaders;
    private int _width;
    private int _height;
    private GWGUI.VideoPresentation.Enums.EmulationVideoSampling _sampling;
    private GWGUI.VideoPresentation.Enums.EmulationVideoDisplayTechnology _displayTechnology;
    private uint _swapchainWidth;
    private uint _swapchainHeight;
    private bool _hasHistory;
    private TimeSpan _historyTimestamp;
    private long _historySequence;
    private WriteableBitmap? _snapshot;
    private VideoFrame? _snapshotSourceFrame;
    private EmulationVideoProcessingSize _snapshotOutputSize;
    private EmulationVideoProcessingConfiguration _snapshotConfiguration =
        EmulationVideoProcessingConfigurationFunctions.Normalize(null);
    private readonly IEmulationVideoProcessingPipeline _videoProcessingPipeline;
    private readonly SoftwareEmulationVideoProcessingPipeline _snapshotPipeline = new();
    private EmulationVideoProcessingConfiguration _videoProcessing =
        EmulationVideoProcessingConfigurationFunctions.Normalize(null);

    internal VeldridVideoSurface(GraphicsBackend backend)
    {
        _backend = backend;
        _videoProcessingPipeline = EmulationVideoProcessingPipelineFactory.Create(
            backend == GraphicsBackend.Vulkan
                ? EmulationVideoRenderer.Vulkan : EmulationVideoRenderer.Direct3D11);
        Focusable = true;
    }

    public FrameworkElement View => this;
    public BitmapSource? Snapshot => EnsureSnapshot();
    public EmulationVideoRenderer Renderer => _backend == GraphicsBackend.Vulkan
        ? EmulationVideoRenderer.Vulkan : EmulationVideoRenderer.Direct3D11;
    public IntPtr InputHandle => _hwnd;
    public EmulationVideoProcessingConfiguration VideoProcessing => _videoProcessing;
    public Task<BitmapSource?> CaptureSnapshotAsync() =>
        EmulationVideoSnapshotFunctions.CreateAsync(_snapshotSourceFrame,
            _snapshotConfiguration, _snapshotOutputSize);

    public void SetVideoProcessing(EmulationVideoProcessingConfiguration configuration)
    {
        var displayTechnologyChanged = _videoProcessing.DisplayTechnology
            != configuration.DisplayTechnology;
        _videoProcessing = EmulationVideoProcessingConfigurationFunctions.Normalize(configuration);
        if (displayTechnologyChanged)
        {
            _hasHistory = false;
            _historyTimestamp = TimeSpan.Zero;
            _historySequence = 0;
        }
        _snapshot = null;
    }

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        _hwnd = NativeVideoWindowFunctions.Create(
            hwndParent.Handle, (int)ActualWidth, (int)ActualHeight);
        if (_hwnd == IntPtr.Zero) throw new InvalidOperationException("Unable to create the emulation video window.");
        return new HandleRef(this, _hwnd);
    }

    protected override void DestroyWindowCore(HandleRef hwnd) => NativeVideoWindowFunctions.Destroy(hwnd.Handle);

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        lock (_deviceGate) ResizeSwapchainToClient();
    }

    public void Present(VideoFrame frame)
    {
        lock (_deviceGate) PresentCore(frame);
    }

    private void PresentCore(VideoFrame frame)
    {
        if (_hwnd == IntPtr.Zero) return;
        var clientSize = NativeVideoWindowFunctions.GetClientSize(_hwnd);
        var outputSize = new EmulationVideoProcessingSize(
            Math.Max(1, clientSize.Width), Math.Max(1, clientSize.Height));
        var processed = frame;
        byte[]? convertedPixels = null;
        ReadOnlySpan<byte> pixels;
        if (frame.PixelFormat == EmulationPixelFormat.Xrgb8888
            && frame.Pitch == checked(frame.Width * 4))
            pixels = frame.Pixels.Span;
        else
        {
            convertedPixels = EmulationVideoPixelFunctions.ToBgra32(frame);
            pixels = convertedPixels;
        }
        EnsureDevice(processed.Width, processed.Height, _videoProcessing.Sampling,
            _videoProcessing.DisplayTechnology);
        ResizeSwapchainToClient();
        _device!.UpdateTexture(_texture!, pixels, 0, 0, 0,
            (uint)processed.Width, (uint)processed.Height, 1, 0, 0);
        var gpuConfiguration = _videoProcessing;
        var fixedPixel = gpuConfiguration.DisplayTechnology
            == EmulationVideoDisplayTechnology.FixedPixel;
        var plasma = gpuConfiguration.DisplayTechnology == EmulationVideoDisplayTechnology.Plasma;
        var vector = gpuConfiguration.DisplayTechnology == EmulationVideoDisplayTechnology.Vector;
        var segmentDisplay = gpuConfiguration.DisplayTechnology
            == EmulationVideoDisplayTechnology.SegmentDisplay;
        var temporalDisplay = fixedPixel || plasma || vector || segmentDisplay
            || gpuConfiguration.DisplayTechnology is EmulationVideoDisplayTechnology.Vfd
                or EmulationVideoDisplayTechnology.DotMatrix or EmulationVideoDisplayTechnology.EPaper
            || gpuConfiguration.Temporal.GeneralPersistence > 0
            || gpuConfiguration.Temporal.MotionBlur > 0
            || gpuConfiguration.Temporal.Interlacing > 0
            || gpuConfiguration.SignalSimulation.StandardIntensity > 0;
        var hasHistory = temporalDisplay && _hasHistory
            && (fixedPixel || segmentDisplay ? frame.Timestamp >= _historyTimestamp
                : frame.Sequence >= _historySequence);
        var elapsedMilliseconds = hasHistory
            ? (frame.Timestamp - _historyTimestamp).TotalMilliseconds : 0;
        _device.UpdateBuffer(_parameterBuffer!, 0, Parameters(
            gpuConfiguration, processed.Width, processed.Height,
            Math.Max(1, clientSize.Width), Math.Max(1, clientSize.Height),
            hasHistory, elapsedMilliseconds, frame.Sequence,
            plasma ? FilterPlasmaAutomaticBrightnessLimiter.MeasureBgra(pixels) : 0f));
        _commands!.Begin();
        _commands.SetFramebuffer(_device.MainSwapchain.Framebuffer);
        _commands.ClearColorTarget(0, RgbaFloat.Black);
        _commands.SetPipeline(_pipeline!);
        _commands.SetVertexBuffer(0, _vertexBuffer!);
        _commands.SetGraphicsResourceSet(0, _set!);
        _commands.Draw(4);
        _commands.End();
        _device.SubmitCommands(_commands);
        _device.SwapBuffers(_device.MainSwapchain);
        if (temporalDisplay)
        {
            _device.UpdateTexture(_historyTexture!, pixels, 0, 0, 0,
                (uint)processed.Width, (uint)processed.Height, 1, 0, 0);
            _hasHistory = true;
            _historyTimestamp = frame.Timestamp;
            _historySequence = frame.Sequence;
        }
        else
        {
            _hasHistory = false;
            _historyTimestamp = TimeSpan.Zero;
            _historySequence = 0;
        }
        _snapshotSourceFrame = frame;
        _snapshotOutputSize = outputSize;
        _snapshotConfiguration = _videoProcessing;
        _snapshot = null;
    }

    private void EnsureDevice(int width, int height,
        GWGUI.VideoPresentation.Enums.EmulationVideoSampling sampling,
        GWGUI.VideoPresentation.Enums.EmulationVideoDisplayTechnology displayTechnology)
    {
        if (_device is null)
        {
            var options = new GraphicsDeviceOptions(false, null, true, ResourceBindingModel.Improved, true, true);
            var source = SwapchainSource.CreateWin32(_hwnd, NativeVideoWindowFunctions.ModuleHandle);
            var clientSize = NativeVideoWindowFunctions.GetClientSize(_hwnd);
            _swapchainWidth = (uint)clientSize.Width;
            _swapchainHeight = (uint)clientSize.Height;
            var swapchain = new SwapchainDescription(source, _swapchainWidth,
                _swapchainHeight, null, false, true);
            _device = _backend == GraphicsBackend.Vulkan
                ? GraphicsDevice.CreateVulkan(options, swapchain)
                : GraphicsDevice.CreateD3D11(options, swapchain);
            _commands = _device.ResourceFactory.CreateCommandList();
        }
        if (_texture is not null && _width == width && _height == height
            && _sampling == sampling && _displayTechnology == displayTechnology) return;
        var factory = _device.ResourceFactory;
        Texture? texture = null;
        TextureView? textureView = null;
        Texture? historyTexture = null;
        TextureView? historyTextureView = null;
        DeviceBuffer? parameterBuffer = null;
        ResourceLayout? layout = null;
        ResourceSet? set = null;
        DeviceBuffer? vertexBuffer = null;
        Shader[]? shaders = null;
        Pipeline? pipeline = null;
        try
        {
            texture = factory.CreateTexture(TextureDescription.Texture2D((uint)width,
                (uint)height, 1, 1, Veldrid.PixelFormat.B8_G8_R8_A8_UNorm,
                TextureUsage.Sampled));
            textureView = factory.CreateTextureView(texture);
            historyTexture = factory.CreateTexture(TextureDescription.Texture2D((uint)width,
                (uint)height, 1, 1, Veldrid.PixelFormat.B8_G8_R8_A8_UNorm,
                TextureUsage.Sampled));
            historyTextureView = factory.CreateTextureView(historyTexture);
            parameterBuffer = factory.CreateBuffer(new BufferDescription(
                (uint)Marshal.SizeOf<VideoParameters>(), BufferUsage.UniformBuffer));
            layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("VideoParameters",
                    ResourceKind.UniformBuffer, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Source",
                    ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("History",
                    ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("PointSampler",
                    ResourceKind.Sampler, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("LinearSampler",
                    ResourceKind.Sampler, ShaderStages.Fragment)));
            set = factory.CreateResourceSet(new ResourceSetDescription(
                layout, parameterBuffer, textureView, historyTextureView,
                _device.PointSampler, _device.LinearSampler));
            var vertices = new[]
            {
                new Vertex(-1, -1, 0, 1), new Vertex(-1, 1, 0, 0),
                new Vertex(1, -1, 1, 1), new Vertex(1, 1, 1, 0)
            };
            vertexBuffer = factory.CreateBuffer(new BufferDescription(
                (uint)(vertices.Length * Marshal.SizeOf<Vertex>()), BufferUsage.VertexBuffer));
            _device.UpdateBuffer(vertexBuffer, 0, vertices);
            shaders = factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex,
                    Encoding.UTF8.GetBytes(VeldridVideoProcessingShaders.Vertex), "main"),
                new ShaderDescription(ShaderStages.Fragment,
                    Encoding.UTF8.GetBytes(VeldridVideoProcessingShaders.Fragment(
                        sampling, displayTechnology)), "main"));
            var shaderSet = new ShaderSetDescription(
                [new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate,
                        VertexElementFormat.Float2),
                    new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate,
                        VertexElementFormat.Float2))], shaders);
            pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend, DepthStencilStateDescription.Disabled,
                RasterizerStateDescription.CullNone, PrimitiveTopology.TriangleStrip, shaderSet,
                [layout], _device.MainSwapchain.Framebuffer.OutputDescription));

            DisposeFrameResources();
            _texture = texture; texture = null;
            _textureView = textureView; textureView = null;
            _historyTexture = historyTexture; historyTexture = null;
            _historyTextureView = historyTextureView; historyTextureView = null;
            _parameterBuffer = parameterBuffer; parameterBuffer = null;
            _layout = layout; layout = null;
            _set = set; set = null;
            _vertexBuffer = vertexBuffer; vertexBuffer = null;
            _shaders = shaders; shaders = null;
            _pipeline = pipeline; pipeline = null;
            _width = width; _height = height; _sampling = sampling;
            _displayTechnology = displayTechnology;
            _hasHistory = false;
            _historyTimestamp = TimeSpan.Zero;
            _historySequence = 0;
        }
        finally
        {
            pipeline?.Dispose();
            set?.Dispose();
            layout?.Dispose();
            parameterBuffer?.Dispose();
            textureView?.Dispose();
            texture?.Dispose();
            historyTextureView?.Dispose();
            historyTexture?.Dispose();
            vertexBuffer?.Dispose();
            if (shaders is not null) foreach (var shader in shaders) shader.Dispose();
        }
    }

    private void ResizeSwapchainToClient()
    {
        if (_device is null || _hwnd == IntPtr.Zero) return;
        var clientSize = NativeVideoWindowFunctions.GetClientSize(_hwnd);
        if ((uint)clientSize.Width == _swapchainWidth && (uint)clientSize.Height == _swapchainHeight) return;
        _device.ResizeMainWindow((uint)clientSize.Width, (uint)clientSize.Height);
        _swapchainWidth = (uint)clientSize.Width;
        _swapchainHeight = (uint)clientSize.Height;
    }

    private BitmapSource? EnsureSnapshot()
    {
        if (_snapshot is not null) return _snapshot;
        var frame = _snapshotSourceFrame;
        if (frame is null) return null;
        var snapshotFrame = _snapshotPipeline.Process(_snapshotConfiguration, frame,
            new EmulationVideoProcessingSize(frame.Width, frame.Height), _snapshotOutputSize);
        UpdateSnapshot(EmulationVideoPixelFunctions.ToBgra32(snapshotFrame),
            snapshotFrame.Width, snapshotFrame.Height);
        return _snapshot;
    }

    private void UpdateSnapshot(byte[] pixels, int width, int height)
    {
        if (_snapshot is null || _snapshot.PixelWidth != width || _snapshot.PixelHeight != height)
            _snapshot = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
        _snapshot.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);
    }

    private void DisposeFrameResources()
    {
        _pipeline?.Dispose(); _pipeline = null;
        _set?.Dispose(); _set = null;
        _layout?.Dispose(); _layout = null;
        _parameterBuffer?.Dispose(); _parameterBuffer = null;
        _textureView?.Dispose(); _textureView = null;
        _texture?.Dispose(); _texture = null;
        _historyTextureView?.Dispose(); _historyTextureView = null;
        _historyTexture?.Dispose(); _historyTexture = null;
        _vertexBuffer?.Dispose(); _vertexBuffer = null;
        if (_shaders is not null) foreach (var shader in _shaders) shader.Dispose();
        _shaders = null;
        _hasHistory = false;
        _historyTimestamp = TimeSpan.Zero;
        _historySequence = 0;
    }

    public new void Dispose()
    {
        _videoProcessingPipeline.Dispose();
        _snapshotPipeline.Dispose();
        lock (_deviceGate)
        {
            DisposeFrameResources();
            _commands?.Dispose();
            _device?.Dispose();
        }
        base.Dispose();
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly record struct Vertex(float X, float Y, float U, float V);

    [StructLayout(LayoutKind.Sequential)]
    private readonly record struct VideoParameters(
        Vector4 Adjustments,
        Vector4 Processing,
        Vector4 Output,
        Vector4 CrtDisplay,
        Vector4 CrtBeam,
        Vector4 CrtOptical,
        Vector4 CrtGeometry,
        Vector4 CrtScanlines,
        Vector4 CrtPattern,
        Vector4 CrtPatternIntensity,
        Vector4 FixedDisplay,
        Vector4 FixedSpatial,
        Vector4 FixedTechnology,
        Vector4 FixedTemporal,
        Vector4 PlasmaEffect,
        Vector4 PlasmaTemporal,
        Vector4 PlasmaDisplay,
        Vector4 VectorEffect,
        Vector4 VectorTemporal,
        Vector4 VectorDisplay,
        Vector4 Restoration,
        Vector4 SegmentGeometry,
        Vector4 SegmentShape,
        Vector4 SegmentEmission,
        Vector4 SegmentOptical,
        Vector4 SegmentTemporal,
        Vector4 General,
        Vector4 Restoration2,
        Vector4 Temporal,
        Vector4 Signal,
        Vector4 Signal2,
        Vector4 Stylistic,
        Vector4 Stylistic2,
        Vector4 VfdDisplay,
        Vector4 VfdStructure,
        Vector4 VfdOptical,
        Vector4 LedMatrixEmission,
        Vector4 LedMatrixStructure,
        Vector4 DotMatrixGeometry,
        Vector4 DotMatrixEmission,
        Vector4 DotMatrixTemporal,
        Vector4 EPaperInkAndColor,
        Vector4 EPaperSurface,
        Vector4 EPaperTemporal,
        Vector4 Projection,
        Vector4 ProjectionScreen);

    private static VideoParameters Parameters(EmulationVideoProcessingConfiguration configuration,
        int sourceWidth, int sourceHeight, int outputWidth, int outputHeight,
        bool hasHistory, double elapsedMilliseconds, long sequence,
        float averageLuminance)
    {
        var adjustments = configuration.Adjustments;
        var crt = CrtVideoShaderParameters.From(configuration);
        var fixedPixel = FixedPixelVideoShaderParameters.From(
            configuration, hasHistory, elapsedMilliseconds);
        var plasma = PlasmaVideoShaderParameters.From(configuration, hasHistory, sequence,
            averageLuminance);
        var vector = VectorVideoShaderParameters.From(configuration, hasHistory);
        var vfd = VfdVideoShaderParameters.From(configuration, hasHistory,
            elapsedMilliseconds);
        var ledMatrix = LedMatrixVideoShaderParameters.From(configuration);
        var dotMatrix = DotMatrixVideoShaderParameters.From(configuration, hasHistory,
            elapsedMilliseconds);
        var segmentDisplay = SegmentDisplayVideoShaderParameters.From(configuration,
            hasHistory, elapsedMilliseconds);
        var ePaper = EPaperVideoShaderParameters.From(configuration, hasHistory,
            elapsedMilliseconds);
        return new VideoParameters(
            new Vector4(
                adjustments.Brightness / 20f,
                MathF.Pow(2f, adjustments.Contrast / 5f),
                (float)EmulationImageAdjustmentFunctions.GammaExponent(adjustments.Gamma),
                1f + adjustments.Saturation / 10f),
            new Vector4(adjustments.Sharpness / 10f, (float)configuration.Sampling,
                sourceWidth, sourceHeight),
            new Vector4(outputWidth, outputHeight, (float)elapsedMilliseconds, 0f),
            crt.Display, crt.Beam, crt.Optical, crt.Geometry, crt.Scanlines,
            crt.Pattern, crt.PatternIntensity,
            fixedPixel.Display, fixedPixel.Spatial, fixedPixel.Technology,
            fixedPixel.Temporal, plasma.Effect, plasma.Temporal, plasma.Display,
            vector.Effect, vector.Temporal, vector.Display,
            new Vector4(configuration.Restoration.DetailRecovery / 100f, 0f, 0f, 0f),
            segmentDisplay.Geometry, segmentDisplay.Shape, segmentDisplay.Emission,
            segmentDisplay.Optical, segmentDisplay.Temporal,
            new Vector4((float)configuration.DisplayTechnology, hasHistory ? 1f : 0f, sequence % 4096, (float)elapsedMilliseconds),
            new Vector4(configuration.Restoration.Dedithering / 100f, configuration.Restoration.Denoising / 100f, configuration.Restoration.Debanding / 100f, (float)configuration.Restoration.Deinterlacing),
            new Vector4(configuration.Temporal.GeneralPersistence / 100f, configuration.Temporal.MotionBlur / 100f, configuration.Temporal.Flicker / 100f, configuration.Temporal.Interlacing > 0 ? 1f : 0f),
            new Vector4((float)configuration.SignalSimulation.Connection,
                configuration.SignalSimulation.ConnectionIntensity / 100f,
                (float)configuration.SignalSimulation.Standard,
                configuration.SignalSimulation.StandardIntensity / 100f),
            new Vector4(0f, configuration.Temporal.BlackFrameInsertion ? 1f : 0f,
                configuration.Temporal.InterlacingVisibility / 100f, 0f),
            new Vector4(configuration.Stylistic.Grain / 100f, configuration.Stylistic.Vhs / 100f, configuration.Stylistic.ChromaticAberration / 100f, configuration.Stylistic.Bloom / 100f),
            new Vector4(configuration.Stylistic.Sepia ? 1f : 0f, 0f, 0f, 0f),
            vfd.Display, vfd.Structure, vfd.Optical,
            ledMatrix.Emission, ledMatrix.Structure,
            dotMatrix.Geometry, dotMatrix.Emission, dotMatrix.Temporal,
            ePaper.InkAndColor, ePaper.PaperSurface, ePaper.Temporal,
            new Vector4(configuration.Projection.OpticalBlur / 100f, configuration.Projection.Diffusion / 100f, configuration.Projection.ScreenTexture / 100f, configuration.Projection.Convergence / 100f),
            new Vector4(configuration.Projection.LightOutput / 100f,
                configuration.Projection.AmbientLight / 100f, configuration.Projection.Vignette / 100f, 0f));
    }
}
