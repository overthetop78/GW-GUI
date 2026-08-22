using GWGUI.App.Constants.Rendering.Emulation;
using GWGUI.App.Functions.Rendering.Emulation;
using GWGUI.App.Interfaces.Rendering.Emulation;
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
    public FrameworkElement View => this;
    public BitmapSource? Snapshot => _snapshot;
    public EmulationVideoRenderer Renderer => EmulationVideoRenderer.OpenGL;
    public IntPtr InputHandle => _hwnd;

    internal OpenGlVideoSurface() => Focusable = true;

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
        return new HandleRef(this, _hwnd);
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        if (_context != IntPtr.Zero) { WglMakeCurrent(IntPtr.Zero, IntPtr.Zero); WglDeleteContext(_context); _context = IntPtr.Zero; }
        if (_dc != IntPtr.Zero) { ReleaseDC(hwnd.Handle, _dc); _dc = IntPtr.Zero; }
        NativeVideoWindowFunctions.Destroy(hwnd.Handle);
    }

    public void Present(VideoFrame frame)
    {
        if (_context == IntPtr.Zero || _hwnd == IntPtr.Zero) return;
        var pixels = EmulationVideoPixelFunctions.ToBgra32(frame);
        var clientSize = NativeVideoWindowFunctions.GetClientSize(_hwnd);
        WglMakeCurrent(_dc, _context);
        GlViewport(0, 0, clientSize.Width, clientSize.Height);
        GlClearColor(0, 0, 0, 1); GlClear(OpenGlVideoConstants.ColorBufferBit);
        GlRasterPos2f(-1, 1);
        GlPixelZoom((float)clientSize.Width / frame.Width, -(float)clientSize.Height / frame.Height);
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try { GlDrawPixels(frame.Width, frame.Height, OpenGlVideoConstants.Bgra, OpenGlVideoConstants.UnsignedByte, handle.AddrOfPinnedObject()); }
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
    [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);
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
