using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GWGUI.Emulation;
using Veldrid;
using Veldrid.SPIRV;

namespace GWGUI.App.Rendering;

internal interface IEmulationVideoSurface : IDisposable
{
    FrameworkElement View { get; }
    BitmapSource? Snapshot { get; }
    void Present(VideoFrame frame);
}

internal static class EmulationVideoSurfaceFactory
{
    internal static IEmulationVideoSurface Create(EmulationVideoRenderer renderer) => renderer switch
    {
        EmulationVideoRenderer.Direct3D11 => new VeldridVideoSurface(GraphicsBackend.Direct3D11),
        EmulationVideoRenderer.Vulkan => new VeldridVideoSurface(GraphicsBackend.Vulkan),
        EmulationVideoRenderer.OpenGL => new OpenGlVideoSurface(),
        _ => new WpfVideoSurface()
    };
}

internal static class EmulationVideoPixels
{
    internal static byte[] ToBgra32(VideoFrame frame)
    {
        var pitch = checked(frame.Width * 4);
        var source = frame.Pixels.Span;
        var destination = GC.AllocateUninitializedArray<byte>(checked(pitch * frame.Height));
        for (var y = 0; y < frame.Height; y++)
        {
            var sourceRow = source.Slice(checked(y * frame.Pitch), frame.Pitch);
            var destinationRow = destination.AsSpan(checked(y * pitch), pitch);
            if (frame.PixelFormat == EmulationPixelFormat.Xrgb8888)
            {
                sourceRow[..Math.Min(sourceRow.Length, pitch)].CopyTo(destinationRow);
                for (var x = 0; x < frame.Width; x++) destinationRow[x * 4 + 3] = 255;
                continue;
            }
            for (var x = 0; x < frame.Width; x++)
            {
                var value = sourceRow[x * 2] | sourceRow[x * 2 + 1] << 8;
                var offset = x * 4;
                destinationRow[offset] = (byte)((value & 0x1f) * 255 / 31);
                destinationRow[offset + 1] = (byte)(((value >> 5) & 0x3f) * 255 / 63);
                destinationRow[offset + 2] = (byte)(((value >> 11) & 0x1f) * 255 / 31);
                destinationRow[offset + 3] = 255;
            }
        }
        return destination;
    }
}

internal sealed class WpfVideoSurface : IEmulationVideoSurface
{
    private readonly Image _image = new() { Stretch = Stretch.Fill, Focusable = true };
    private WriteableBitmap? _bitmap;
    public FrameworkElement View => _image;
    public BitmapSource? Snapshot => _bitmap;

    public void Present(VideoFrame frame)
    {
        var pixels = EmulationVideoPixels.ToBgra32(frame);
        var pitch = checked(frame.Width * 4);
        if (_bitmap is null || _bitmap.PixelWidth != frame.Width || _bitmap.PixelHeight != frame.Height)
        {
            _bitmap = new WriteableBitmap(frame.Width, frame.Height, 96, 96, PixelFormats.Bgra32, null);
            _image.Source = _bitmap;
        }
        _bitmap.WritePixels(new Int32Rect(0, 0, frame.Width, frame.Height), pixels, pitch, 0);
    }

    public void Dispose() { }
}

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
    private WriteableBitmap? _snapshot;

    internal VeldridVideoSurface(GraphicsBackend backend)
    {
        _backend = backend;
        Focusable = true;
    }

    public FrameworkElement View => this;
    public BitmapSource? Snapshot => _snapshot;

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        _hwnd = CreateWindowEx(0, "STATIC", string.Empty, WsChild | WsVisible | WsClipChildren | WsClipSiblings,
            0, 0, Math.Max(1, (int)ActualWidth), Math.Max(1, (int)ActualHeight), hwndParent.Handle,
            IntPtr.Zero, GetModuleHandle(null), IntPtr.Zero);
        if (_hwnd == IntPtr.Zero) throw new InvalidOperationException("Unable to create the emulation video window.");
        return new HandleRef(this, _hwnd);
    }

    protected override void DestroyWindowCore(HandleRef hwnd) => DestroyWindow(hwnd.Handle);

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        if (_device is not null && ActualWidth >= 1 && ActualHeight >= 1)
            _device.ResizeMainWindow((uint)ActualWidth, (uint)ActualHeight);
    }

    public void Present(VideoFrame frame)
    {
        if (_hwnd == IntPtr.Zero) return;
        EnsureDevice(frame.Width, frame.Height);
        var pixels = EmulationVideoPixels.ToBgra32(frame);
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
            var source = SwapchainSource.CreateWin32(_hwnd, GetModuleHandle(null));
            var swapchain = new SwapchainDescription(source, (uint)Math.Max(1, ActualWidth),
                (uint)Math.Max(1, ActualHeight), null, false, true);
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
    private const int WsChild = 0x40000000, WsVisible = 0x10000000, WsClipChildren = 0x02000000, WsClipSiblings = 0x04000000;
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateWindowEx(int exStyle, string className, string windowName, int style,
        int x, int y, int width, int height, IntPtr parent, IntPtr menu, IntPtr instance, IntPtr parameter);
    [DllImport("user32.dll")] private static extern bool DestroyWindow(IntPtr hwnd);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr GetModuleHandle(string? name);
}

internal sealed class OpenGlVideoSurface : HwndHost, IEmulationVideoSurface
{
    private IntPtr _hwnd;
    private IntPtr _dc;
    private IntPtr _context;
    private WriteableBitmap? _snapshot;
    public FrameworkElement View => this;
    public BitmapSource? Snapshot => _snapshot;

    internal OpenGlVideoSurface() => Focusable = true;

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        _hwnd = CreateWindowEx(0, "STATIC", string.Empty, WsChild | WsVisible | WsClipChildren | WsClipSiblings,
            0, 0, Math.Max(1, (int)ActualWidth), Math.Max(1, (int)ActualHeight), hwndParent.Handle,
            IntPtr.Zero, GetModuleHandle(null), IntPtr.Zero);
        if (_hwnd == IntPtr.Zero) throw new InvalidOperationException("Unable to create the OpenGL video window.");
        _dc = GetDC(_hwnd);
        var descriptor = new PixelFormatDescriptor
        {
            Size = (ushort)Marshal.SizeOf<PixelFormatDescriptor>(), Version = 1,
            Flags = PfdDrawToWindow | PfdSupportOpenGl | PfdDoubleBuffer,
            PixelType = PfdTypeRgba, ColorBits = 32, DepthBits = 0, LayerType = PfdMainPlane
        };
        var format = ChoosePixelFormat(_dc, ref descriptor);
        if (format == 0 || !SetPixelFormat(_dc, format, ref descriptor))
            throw new InvalidOperationException("OpenGL could not select a compatible pixel format.");
        _context = WglCreateContext(_dc);
        if (_context == IntPtr.Zero) throw new InvalidOperationException("OpenGL could not create a rendering context.");
        return new HandleRef(this, _hwnd);
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        if (_context != IntPtr.Zero) { WglMakeCurrent(IntPtr.Zero, IntPtr.Zero); WglDeleteContext(_context); _context = IntPtr.Zero; }
        if (_dc != IntPtr.Zero) { ReleaseDC(hwnd.Handle, _dc); _dc = IntPtr.Zero; }
        DestroyWindow(hwnd.Handle);
    }

    public void Present(VideoFrame frame)
    {
        if (_context == IntPtr.Zero || ActualWidth < 1 || ActualHeight < 1) return;
        var pixels = EmulationVideoPixels.ToBgra32(frame);
        WglMakeCurrent(_dc, _context);
        GlViewport(0, 0, Math.Max(1, (int)ActualWidth), Math.Max(1, (int)ActualHeight));
        GlClearColor(0, 0, 0, 1); GlClear(GlColorBufferBit);
        GlRasterPos2f(-1, 1);
        GlPixelZoom((float)ActualWidth / frame.Width, -(float)ActualHeight / frame.Height);
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try { GlDrawPixels(frame.Width, frame.Height, GlBgra, GlUnsignedByte, handle.AddrOfPinnedObject()); }
        finally { handle.Free(); }
        SwapBuffers(_dc);
        if (_snapshot is null || _snapshot.PixelWidth != frame.Width || _snapshot.PixelHeight != frame.Height)
            _snapshot = new WriteableBitmap(frame.Width, frame.Height, 96, 96, PixelFormats.Bgra32, null);
        _snapshot.WritePixels(new Int32Rect(0, 0, frame.Width, frame.Height), pixels, frame.Width * 4, 0);
    }

    public new void Dispose() => base.Dispose();

    [StructLayout(LayoutKind.Sequential)]
    private struct PixelFormatDescriptor
    {
        public ushort Size, Version; public uint Flags; public byte PixelType, ColorBits, RedBits, RedShift,
            GreenBits, GreenShift, BlueBits, BlueShift, AlphaBits, AlphaShift, AccumBits, AccumRedBits,
            AccumGreenBits, AccumBlueBits, AccumAlphaBits, DepthBits, StencilBits, AuxBuffers, LayerType, Reserved;
        public uint LayerMask, VisibleMask, DamageMask;
    }
    private const int WsChild = 0x40000000, WsVisible = 0x10000000, WsClipChildren = 0x02000000, WsClipSiblings = 0x04000000;
    private const uint PfdDrawToWindow = 4, PfdSupportOpenGl = 32, PfdDoubleBuffer = 1;
    private const byte PfdTypeRgba = 0, PfdMainPlane = 0;
    private const uint GlColorBufferBit = 0x4000, GlBgra = 0x80E1, GlUnsignedByte = 0x1401;
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern IntPtr CreateWindowEx(int exStyle, string className, string windowName, int style, int x, int y, int width, int height, IntPtr parent, IntPtr menu, IntPtr instance, IntPtr parameter);
    [DllImport("user32.dll")] private static extern bool DestroyWindow(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr GetModuleHandle(string? name);
    [DllImport("gdi32.dll")] private static extern int ChoosePixelFormat(IntPtr dc, ref PixelFormatDescriptor descriptor);
    [DllImport("gdi32.dll")] private static extern bool SetPixelFormat(IntPtr dc, int format, ref PixelFormatDescriptor descriptor);
    [DllImport("gdi32.dll")] private static extern bool SwapBuffers(IntPtr dc);
    [DllImport("opengl32.dll", EntryPoint = "wglCreateContext")] private static extern IntPtr WglCreateContext(IntPtr dc);
    [DllImport("opengl32.dll", EntryPoint = "wglMakeCurrent")] private static extern bool WglMakeCurrent(IntPtr dc, IntPtr context);
    [DllImport("opengl32.dll", EntryPoint = "wglDeleteContext")] private static extern bool WglDeleteContext(IntPtr context);
    [DllImport("opengl32.dll", EntryPoint = "glViewport")] private static extern void GlViewport(int x, int y, int width, int height);
    [DllImport("opengl32.dll", EntryPoint = "glClearColor")] private static extern void GlClearColor(float red, float green, float blue, float alpha);
    [DllImport("opengl32.dll", EntryPoint = "glClear")] private static extern void GlClear(uint mask);
    [DllImport("opengl32.dll", EntryPoint = "glRasterPos2f")] private static extern void GlRasterPos2f(float x, float y);
    [DllImport("opengl32.dll", EntryPoint = "glPixelZoom")] private static extern void GlPixelZoom(float x, float y);
    [DllImport("opengl32.dll", EntryPoint = "glDrawPixels")] private static extern void GlDrawPixels(int width, int height, uint format, uint type, IntPtr pixels);
}
