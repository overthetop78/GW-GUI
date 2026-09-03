using GWGUI.App.Constants.Rendering.Emulation;
using GWGUI.App.Factories.Rendering.Emulation;
using GWGUI.App.Functions.Rendering.Emulation;
using GWGUI.App.Interfaces.Rendering.Emulation;
using GWGUI.App.Rendering.Emulation.Processing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GWGUI.Emulation;

namespace GWGUI.App.Rendering.Emulation.Surfaces;

internal sealed class OpenGlVideoSurface : HwndHost, IEmulationVideoSurface
{
    private IntPtr _hwnd;
    private IntPtr _dc;
    private IntPtr _context;
    private WriteableBitmap? _snapshot;
    private VideoFrame? _snapshotSourceFrame;
    private EmulationVideoProcessingSize _snapshotOutputSize;
    private EmulationVideoProcessingConfiguration _snapshotConfiguration =
        EmulationVideoProcessingConfigurationFunctions.Normalize(null);
    private readonly IEmulationVideoProcessingPipeline _videoProcessingPipeline;
    private readonly SoftwareEmulationVideoProcessingPipeline _snapshotPipeline = new();
    private OpenGlVideoProcessingProgram? _program;
    private uint _texture;
    private uint _historyTexture;
    private ActiveTextureDelegate? _activeTexture;
    private bool _hasHistory;
    private int _historyWidth;
    private int _historyHeight;
    private TimeSpan _historyTimestamp;
    private long _historySequence;
    private EmulationVideoProcessingConfiguration _videoProcessing =
        EmulationVideoProcessingConfigurationFunctions.Normalize(null);
    public FrameworkElement View => this;
    public BitmapSource? Snapshot => EnsureSnapshot();
    public EmulationVideoRenderer Renderer => EmulationVideoRenderer.OpenGL;
    public IntPtr InputHandle => _hwnd;
    public EmulationVideoProcessingConfiguration VideoProcessing => _videoProcessing;
    public Task<BitmapSource?> CaptureSnapshotAsync() =>
        EmulationVideoSnapshotFunctions.CreateAsync(_snapshotSourceFrame,
            _snapshotConfiguration, _snapshotOutputSize);

    internal OpenGlVideoSurface()
    {
        Focusable = true;
        _videoProcessingPipeline =
            EmulationVideoProcessingPipelineFactory.Create(EmulationVideoRenderer.OpenGL);
    }

    public void SetVideoProcessing(EmulationVideoProcessingConfiguration configuration)
    {
        _videoProcessing = EmulationVideoProcessingConfigurationFunctions.Normalize(configuration);
        _snapshot = null;
    }

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        _hwnd = NativeVideoWindowFunctions.Create(
            hwndParent.Handle, (int)ActualWidth, (int)ActualHeight);
        if (_hwnd == IntPtr.Zero) throw new InvalidOperationException("Unable to create the OpenGL video window.");
        _dc = GetDC(_hwnd);
        var descriptor = new PixelFormatDescriptor
        {
            Size = (ushort)Marshal.SizeOf<PixelFormatDescriptor>(), Version = 1,
            Flags = OpenGlVideoConstants.PixelFormatDrawToWindow |
                OpenGlVideoConstants.PixelFormatSupportOpenGl |
                OpenGlVideoConstants.PixelFormatDoubleBuffer,
            PixelType = OpenGlVideoConstants.PixelTypeRgba,
            ColorBits = 32,
            DepthBits = 0,
            LayerType = OpenGlVideoConstants.MainPlane
        };
        var format = ChoosePixelFormat(_dc, ref descriptor);
        if (format == 0 || !SetPixelFormat(_dc, format, ref descriptor))
            throw new InvalidOperationException("OpenGL could not select a compatible pixel format.");
        _context = WglCreateContext(_dc);
        if (_context == IntPtr.Zero) throw new InvalidOperationException("OpenGL could not create a rendering context.");
        WglMakeCurrent(_dc, _context);
        _activeTexture = LoadActiveTexture();
        _program = new OpenGlVideoProcessingProgram();
        _activeTexture(OpenGlVideoConstants.Texture0);
        GlGenTextures(1, out _texture);
        GlBindTexture(OpenGlVideoConstants.Texture2D, _texture);
        GlTexParameteri(OpenGlVideoConstants.Texture2D,
            OpenGlVideoConstants.TextureWrapS, OpenGlVideoConstants.Clamp);
        GlTexParameteri(OpenGlVideoConstants.Texture2D,
            OpenGlVideoConstants.TextureWrapT, OpenGlVideoConstants.Clamp);
        _activeTexture(OpenGlVideoConstants.Texture1);
        GlGenTextures(1, out _historyTexture);
        GlBindTexture(OpenGlVideoConstants.Texture2D, _historyTexture);
        GlTexParameteri(OpenGlVideoConstants.Texture2D,
            OpenGlVideoConstants.TextureWrapS, OpenGlVideoConstants.Clamp);
        GlTexParameteri(OpenGlVideoConstants.Texture2D,
            OpenGlVideoConstants.TextureWrapT, OpenGlVideoConstants.Clamp);
        _activeTexture(OpenGlVideoConstants.Texture0);
        WglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
        return new HandleRef(this, _hwnd);
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        if (_context != IntPtr.Zero)
        {
            WglMakeCurrent(_dc, _context);
            _program?.Dispose(); _program = null;
            if (_texture != 0) { GlDeleteTextures(1, ref _texture); _texture = 0; }
            if (_historyTexture != 0)
            {
                GlDeleteTextures(1, ref _historyTexture);
                _historyTexture = 0;
            }
            _activeTexture = null;
            ResetHistory();
            WglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
            WglDeleteContext(_context);
            _context = IntPtr.Zero;
        }
        if (_dc != IntPtr.Zero) { ReleaseDC(hwnd.Handle, _dc); _dc = IntPtr.Zero; }
        NativeVideoWindowFunctions.Destroy(hwnd.Handle);
    }

    public void Present(VideoFrame frame)
    {
        if (_context == IntPtr.Zero || _hwnd == IntPtr.Zero) return;
        var clientSize = NativeVideoWindowFunctions.GetClientSize(_hwnd);
        var outputSize = new EmulationVideoProcessingSize(
            Math.Max(1, clientSize.Width), Math.Max(1, clientSize.Height));
        var processed = frame;
        byte[] pixels;
        var pixelOffset = 0;
        if (frame.PixelFormat == EmulationPixelFormat.Xrgb8888
            && frame.Pitch == checked(frame.Width * 4)
            && MemoryMarshal.TryGetArray(frame.Pixels, out var sourceSegment)
            && sourceSegment.Array is not null)
        {
            pixels = sourceSegment.Array;
            pixelOffset = sourceSegment.Offset;
        }
        else pixels = EmulationVideoPixelFunctions.ToBgra32(frame);
        WglMakeCurrent(_dc, _context);
        var gpuConfiguration = _videoProcessing;
        var fixedPixel = gpuConfiguration.DisplayTechnology == EmulationVideoDisplayTechnology.FixedPixel;
        var temporalDisplay = gpuConfiguration.DisplayTechnology != EmulationVideoDisplayTechnology.Normal
            || gpuConfiguration.Temporal.GeneralPersistence > 0
            || gpuConfiguration.Temporal.MotionBlur > 0;
        var hasHistory = temporalDisplay && _hasHistory
            && _historyWidth == processed.Width && _historyHeight == processed.Height
            && (fixedPixel ? frame.Timestamp >= _historyTimestamp : frame.Sequence >= _historySequence);
        var elapsedMilliseconds = hasHistory ? (frame.Timestamp - _historyTimestamp).TotalMilliseconds : 0;
        var filter = gpuConfiguration.Sampling is EmulationVideoSampling.Bilinear
            or EmulationVideoSampling.SharpBilinear
            or EmulationVideoSampling.Hq2x
            or EmulationVideoSampling.Hq3x
            or EmulationVideoSampling.Hq4x
            or EmulationVideoSampling.SuperTwoXSai
            or EmulationVideoSampling.SuperEagle
            ? OpenGlVideoConstants.Linear : OpenGlVideoConstants.Nearest;
        if (hasHistory)
        {
            _activeTexture!(OpenGlVideoConstants.Texture1);
            GlBindTexture(OpenGlVideoConstants.Texture2D, _historyTexture);
            GlTexParameteri(OpenGlVideoConstants.Texture2D, OpenGlVideoConstants.TextureMinFilter, filter);
            GlTexParameteri(OpenGlVideoConstants.Texture2D, OpenGlVideoConstants.TextureMagFilter, filter);
        }
        _activeTexture!(OpenGlVideoConstants.Texture0);
        GlBindTexture(OpenGlVideoConstants.Texture2D, _texture);
        GlTexParameteri(OpenGlVideoConstants.Texture2D, OpenGlVideoConstants.TextureMinFilter, filter);
        GlTexParameteri(OpenGlVideoConstants.Texture2D, OpenGlVideoConstants.TextureMagFilter, filter);
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try
        {
            GlTexImage2D(OpenGlVideoConstants.Texture2D, 0,
                (int)OpenGlVideoConstants.Rgba, processed.Width, processed.Height, 0,
                OpenGlVideoConstants.Bgra, OpenGlVideoConstants.UnsignedByte,
                IntPtr.Add(handle.AddrOfPinnedObject(), pixelOffset));
        }
        finally { handle.Free(); }        GlViewport(0, 0, clientSize.Width, clientSize.Height);
        GlClearColor(0, 0, 0, 1); GlClear(OpenGlVideoConstants.ColorBufferBit);
        _program!.Use(gpuConfiguration, processed.Width, processed.Height,
            Math.Max(1, clientSize.Width), Math.Max(1, clientSize.Height),
            hasHistory, elapsedMilliseconds, frame.Sequence);
        GlEnable(OpenGlVideoConstants.Texture2D);
        GlBegin(OpenGlVideoConstants.Quads);
        GlTexCoord2f(0, 1); GlVertex2f(-1, -1);
        GlTexCoord2f(0, 0); GlVertex2f(-1, 1);
        GlTexCoord2f(1, 0); GlVertex2f(1, 1);
        GlTexCoord2f(1, 1); GlVertex2f(1, -1);
        GlEnd();
        _program.Stop();
        SwapBuffers(_dc);
        if (temporalDisplay)
        {
            (_texture, _historyTexture) = (_historyTexture, _texture);
            _hasHistory = true;
            _historyWidth = processed.Width;
            _historyHeight = processed.Height;
            _historyTimestamp = frame.Timestamp;
            _historySequence = frame.Sequence;
        }
        else ResetHistory();
        _snapshotSourceFrame = frame;
        _snapshotOutputSize = outputSize;
        _snapshotConfiguration = _videoProcessing;
        _snapshot = null;
        WglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
    }

    public new void Dispose()
    {
        _videoProcessingPipeline.Dispose();
        _snapshotPipeline.Dispose();
        base.Dispose();
    }

    private void ResetHistory()
    {
        _hasHistory = false;
        _historyWidth = 0;
        _historyHeight = 0;
        _historyTimestamp = TimeSpan.Zero;
        _historySequence = 0;
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

    private static ActiveTextureDelegate LoadActiveTexture()
    {
        var address = WglGetProcAddress("glActiveTexture");
        if (address == IntPtr.Zero || address == new IntPtr(1) || address == new IntPtr(2)
            || address == new IntPtr(3) || address == new IntPtr(-1))
            address = WglGetProcAddress("glActiveTextureARB");
        if (address == IntPtr.Zero || address == new IntPtr(1) || address == new IntPtr(2)
            || address == new IntPtr(3) || address == new IntPtr(-1))
            throw new InvalidOperationException("OpenGL multitexturing is unavailable.");
        return Marshal.GetDelegateForFunctionPointer<ActiveTextureDelegate>(address);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PixelFormatDescriptor
    {
        public ushort Size, Version; public uint Flags; public byte PixelType, ColorBits, RedBits, RedShift,
            GreenBits, GreenShift, BlueBits, BlueShift, AlphaBits, AlphaShift, AccumBits, AccumRedBits,
            AccumGreenBits, AccumBlueBits, AccumAlphaBits, DepthBits, StencilBits, AuxBuffers, LayerType, Reserved;
        public uint LayerMask, VisibleMask, DamageMask;
    }
    [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);
    [DllImport("gdi32.dll")] private static extern int ChoosePixelFormat(IntPtr dc, ref PixelFormatDescriptor descriptor);
    [DllImport("gdi32.dll")] private static extern bool SetPixelFormat(IntPtr dc, int format, ref PixelFormatDescriptor descriptor);
    [DllImport("gdi32.dll")] private static extern bool SwapBuffers(IntPtr dc);
    [DllImport("opengl32.dll", EntryPoint = "wglCreateContext")] private static extern IntPtr WglCreateContext(IntPtr dc);
    [DllImport("opengl32.dll", EntryPoint = "wglMakeCurrent")] private static extern bool WglMakeCurrent(IntPtr dc, IntPtr context);
    [DllImport("opengl32.dll", EntryPoint = "wglDeleteContext")] private static extern bool WglDeleteContext(IntPtr context);
    [DllImport("opengl32.dll", EntryPoint = "wglGetProcAddress", CharSet = CharSet.Ansi)]
    private static extern IntPtr WglGetProcAddress(string name);
    [DllImport("opengl32.dll", EntryPoint = "glViewport")] private static extern void GlViewport(int x, int y, int width, int height);
    [DllImport("opengl32.dll", EntryPoint = "glClearColor")] private static extern void GlClearColor(float red, float green, float blue, float alpha);
    [DllImport("opengl32.dll", EntryPoint = "glClear")] private static extern void GlClear(uint mask);
    [DllImport("opengl32.dll", EntryPoint = "glGenTextures")] private static extern void GlGenTextures(int count, out uint texture);
    [DllImport("opengl32.dll", EntryPoint = "glDeleteTextures")] private static extern void GlDeleteTextures(int count, ref uint texture);
    [DllImport("opengl32.dll", EntryPoint = "glBindTexture")] private static extern void GlBindTexture(uint target, uint texture);
    [DllImport("opengl32.dll", EntryPoint = "glTexParameteri")] private static extern void GlTexParameteri(uint target, uint name, uint value);
    [DllImport("opengl32.dll", EntryPoint = "glTexImage2D")] private static extern void GlTexImage2D(uint target, int level, int internalFormat, int width, int height, int border, uint format, uint type, IntPtr pixels);
    [DllImport("opengl32.dll", EntryPoint = "glEnable")] private static extern void GlEnable(uint capability);
    [DllImport("opengl32.dll", EntryPoint = "glBegin")] private static extern void GlBegin(uint mode);
    [DllImport("opengl32.dll", EntryPoint = "glEnd")] private static extern void GlEnd();
    [DllImport("opengl32.dll", EntryPoint = "glTexCoord2f")] private static extern void GlTexCoord2f(float x, float y);
    [DllImport("opengl32.dll", EntryPoint = "glVertex2f")] private static extern void GlVertex2f(float x, float y);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate void ActiveTextureDelegate(uint texture);
}
