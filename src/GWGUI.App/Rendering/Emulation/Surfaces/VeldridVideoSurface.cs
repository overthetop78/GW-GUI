using GWGUI.App.Functions.Rendering.Emulation;
using GWGUI.App.Interfaces.Rendering.Emulation;
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
    private IntPtr _hwnd;
    private GraphicsDevice? _device;
    private DeviceBuffer? _vertexBuffer;
    private Texture? _texture;
    private TextureView? _textureView;
    private ResourceLayout? _layout;
    private ResourceSet? _set;
    private Pipeline? _pipeline;
    private CommandList? _commands;
    private Shader[]? _shaders;
    private int _width;
    private int _height;
    private uint _swapchainWidth;
    private uint _swapchainHeight;
    private WriteableBitmap? _snapshot;

    internal VeldridVideoSurface(GraphicsBackend backend)
    {
        _backend = backend;
        Focusable = true;
    }

    public FrameworkElement View => this;
    public BitmapSource? Snapshot => _snapshot;
    public EmulationVideoRenderer Renderer => _backend == GraphicsBackend.Vulkan
        ? EmulationVideoRenderer.Vulkan : EmulationVideoRenderer.Direct3D11;
    public IntPtr InputHandle => _hwnd;

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
        ResizeSwapchainToClient();
    }

    public void Present(VideoFrame frame)
    {
        if (_hwnd == IntPtr.Zero) return;
        EnsureDevice(frame.Width, frame.Height);
        ResizeSwapchainToClient();
        var pixels = EmulationVideoPixelFunctions.ToBgra32(frame);
        _device!.UpdateTexture(_texture!, pixels, 0, 0, 0, (uint)frame.Width, (uint)frame.Height, 1, 0, 0);
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
        UpdateSnapshot(pixels, frame.Width, frame.Height);
    }

    private void EnsureDevice(int width, int height)
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
        if (_texture is not null && _width == width && _height == height) return;
        DisposeFrameResources();
        _width = width; _height = height;
        var factory = _device.ResourceFactory;
        _texture = factory.CreateTexture(TextureDescription.Texture2D((uint)width, (uint)height, 1, 1,
            Veldrid.PixelFormat.B8_G8_R8_A8_UNorm, TextureUsage.Sampled));
        _textureView = factory.CreateTextureView(_texture);
        _layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("Source", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)));
        _set = factory.CreateResourceSet(new ResourceSetDescription(_layout, _textureView, _device.PointSampler));
        var vertices = new[]
        {
            new Vertex(-1, -1, 0, 1), new Vertex(-1, 1, 0, 0),
            new Vertex(1, -1, 1, 1), new Vertex(1, 1, 1, 0)
        };
        _vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(vertices.Length * Marshal.SizeOf<Vertex>()), BufferUsage.VertexBuffer));
        _device.UpdateBuffer(_vertexBuffer, 0, vertices);
        const string vertex = "#version 450\nlayout(location=0) in vec2 Position; layout(location=1) in vec2 TexCoord; layout(location=0) out vec2 fsin_TexCoord; void main(){ gl_Position=vec4(Position,0,1); fsin_TexCoord=TexCoord; }";
        const string fragment = "#version 450\nlayout(set=0,binding=0) uniform texture2D Source; layout(set=0,binding=1) uniform sampler SourceSampler; layout(location=0) in vec2 fsin_TexCoord; layout(location=0) out vec4 fsout_Color; void main(){ fsout_Color=texture(sampler2D(Source,SourceSampler),fsin_TexCoord); }";
        _shaders = factory.CreateFromSpirv(
            new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertex), "main"),
            new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragment), "main"));
        var shaderSet = new ShaderSetDescription(
            [new VertexLayoutDescription(new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))], _shaders);
        _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
            BlendStateDescription.SingleOverrideBlend, DepthStencilStateDescription.Disabled,
            RasterizerStateDescription.CullNone, PrimitiveTopology.TriangleStrip, shaderSet,
            [_layout], _device.MainSwapchain.Framebuffer.OutputDescription));
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
        _textureView?.Dispose(); _textureView = null;
        _texture?.Dispose(); _texture = null;
        _vertexBuffer?.Dispose(); _vertexBuffer = null;
        if (_shaders is not null) foreach (var shader in _shaders) shader.Dispose();
        _shaders = null;
    }

    public new void Dispose()
    {
        DisposeFrameResources();
        _commands?.Dispose();
        _device?.Dispose();
        base.Dispose();
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly record struct Vertex(float X, float Y, float U, float V);
}
