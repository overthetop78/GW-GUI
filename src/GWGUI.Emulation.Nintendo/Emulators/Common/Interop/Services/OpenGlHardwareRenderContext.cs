using System.Runtime.InteropServices;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Exceptions;

namespace GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Services;

internal sealed partial class OpenGlHardwareRenderContext : IDisposable
{
    private const string StaticWindowClass = "STATIC";
    private const string EmptyWindowTitle = "";
    private const string OpenGlLibrary = "opengl32.dll";
    private const string UserLibrary = "user32.dll";
    private const string GdiLibrary = "gdi32.dll";
    private const int WindowStylePopup = unchecked((int)0x80000000);
    private const int PixelFormatDrawToWindow = 0x00000004;
    private const int PixelFormatSupportOpenGl = 0x00000020;
    private const int PixelFormatDoubleBuffer = 0x00000001;
    private const byte PixelTypeRgba = 0;
    private const byte MainPlane = 0;
    private const uint GlPackAlignment = 0x0D05;
    private const uint GlBgra = 0x80E1;
    private const uint GlUnsignedByte = 0x1401;
    private const int GlReadBufferCommand = 0x0C02;
    private const int GlBack = 0x0405;
    internal const int OpenGlContextType = 1;
    private const int OpenGlVersionMajor = 3;
    private const int OpenGlVersionMinor = 0;
    private const int ContextWidth = 1;
    private const int ContextHeight = 1;
    private const int ColorBits = 32;
    private const int DepthBits = 24;
    private const int StencilBits = 8;
    private const int InvalidProcAddress = -1;
    private const int InvalidProcAddressOne = 1;
    private const int InvalidProcAddressTwo = 2;
    private const int InvalidProcAddressThree = 3;

    private readonly ExternalCoreApi.HardwareGetCurrentFramebuffer _framebufferCallback;
    private readonly ExternalCoreApi.HardwareGetProcAddress _procAddressCallback;
    private IntPtr _window;
    private IntPtr _deviceContext;
    private IntPtr _renderingContext;
    private bool _disposed;
    private readonly bool _bottomLeftOrigin;

    internal OpenGlHardwareRenderContext(ExternalCoreApi.HardwareRenderCallback callback)
    {
        _bottomLeftOrigin = callback.BottomLeftOrigin;
        _framebufferCallback = GetCurrentFramebuffer;
        _procAddressCallback = GetProcAddress;
        var initialized = false;
        try
        {
            CreateContext();
            ConfigureContext(callback);
            EnsureFramebuffer(InitialFramebufferSize, InitialFramebufferSize);
            initialized = true;
        }
        finally
        {
            if (!initialized) Dispose();
        }
    }

    internal nint CurrentFramebufferCallback => Marshal.GetFunctionPointerForDelegate(_framebufferCallback);
    internal nint ProcAddressCallback => Marshal.GetFunctionPointerForDelegate(_procAddressCallback);

    internal int ContextType => OpenGlContextType;
    internal uint VersionMajor => OpenGlVersionMajor;
    internal uint VersionMinor => OpenGlVersionMinor;

    internal bool TryReadFramebuffer(int width, int height, out byte[] pixels)
    {
        pixels = [];
        if (_disposed || _deviceContext == IntPtr.Zero || _renderingContext == IntPtr.Zero
            || width <= 0 || height <= 0 || !MakeCurrent()) return false;

        var byteCount = checked(width * height * sizeof(int));
        pixels = new byte[byteCount];
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try
        {
            BindFramebuffer(FramebufferTarget, _framebuffer);
            GlReadBuffer(ColorAttachment);
            GlPixelStorei(GlPackAlignment, 1);
            GlReadPixels(0, 0, width, height, GlBgra, GlUnsignedByte, handle.AddrOfPinnedObject());
            GlFinish();
        }
        finally
        {
            handle.Free();
        }

        if (_bottomLeftOrigin) FlipRows(pixels, width, height);
        return true;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { DeleteFramebuffer(); } finally { DestroyContext(); }
        GC.KeepAlive(_framebufferCallback);
        GC.KeepAlive(_procAddressCallback);
    }

    internal bool MakeCurrentForCore() => !_disposed && MakeCurrent();

    private void CreateContext()
    {
        _window = CreateWindowEx(0, StaticWindowClass, EmptyWindowTitle, WindowStylePopup,
            0, 0, ContextWidth, ContextHeight, IntPtr.Zero, IntPtr.Zero,
            GetModuleHandle(null), IntPtr.Zero);
        if (_window == IntPtr.Zero) throw new InvalidOperationException(CoreExceptions.HostConfigurationInvalid());
        _deviceContext = GetDC(_window);
        if (_deviceContext == IntPtr.Zero) throw new InvalidOperationException(CoreExceptions.HostConfigurationInvalid());

        var descriptor = new PixelFormatDescriptor
        {
            Size = (ushort)Marshal.SizeOf<PixelFormatDescriptor>(),
            Version = 1,
            Flags = PixelFormatDrawToWindow | PixelFormatSupportOpenGl | PixelFormatDoubleBuffer,
            PixelType = PixelTypeRgba,
            ColorBits = ColorBits,
            DepthBits = DepthBits,
            StencilBits = StencilBits,
            LayerType = MainPlane
        };
        var format = ChoosePixelFormat(_deviceContext, ref descriptor);
        if (format == 0 || !SetPixelFormat(_deviceContext, format, ref descriptor))
            throw new InvalidOperationException(CoreExceptions.HostConfigurationInvalid());
        _renderingContext = WglCreateContext(_deviceContext);
        if (_renderingContext == IntPtr.Zero || !MakeCurrent())
            throw new InvalidOperationException(CoreExceptions.HostConfigurationInvalid());
    }

    private void DestroyContext()
    {
        try
        {
            if (_renderingContext != IntPtr.Zero)
            {
                try
                {
                    if (!WglMakeCurrent(IntPtr.Zero, IntPtr.Zero))
                        throw new InvalidOperationException(CoreExceptions.HostConfigurationInvalid());
                }
                finally
                {
                    if (!WglDeleteContext(_renderingContext))
                        throw new InvalidOperationException(CoreExceptions.HostConfigurationInvalid());
                    _renderingContext = IntPtr.Zero;
                }
            }
        }
        finally
        {
            try
            {
                if (_deviceContext != IntPtr.Zero)
                {
                    ReleaseDC(_window, _deviceContext);
                    _deviceContext = IntPtr.Zero;
                }
            }
            finally
            {
                if (_window != IntPtr.Zero)
                {
                    var window = _window;
                    DestroyWindow(window);
                    if (IsWindow(window)) throw new InvalidOperationException(CoreExceptions.HostConfigurationInvalid());
                    _window = IntPtr.Zero;
                }
            }
        }
    }

    [DllImport(UserLibrary)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindow(nint window);

    private bool MakeCurrent() => WglMakeCurrent(_deviceContext, _renderingContext);

    private nint GetCurrentFramebuffer() => (nint)_framebuffer;

    private static nint GetProcAddress(nint symbol)
    {
        if (symbol == IntPtr.Zero) return IntPtr.Zero;
        var name = Marshal.PtrToStringAnsi(symbol);
        if (string.IsNullOrWhiteSpace(name)) return IntPtr.Zero;
        var address = WglGetProcAddress(name);
        return IsInvalidProcAddress(address)
            ? GetProcAddress(GetModuleHandle(OpenGlLibrary), name)
            : address;
    }

    private static bool IsInvalidProcAddress(nint address) => address == IntPtr.Zero
        || address == new IntPtr(InvalidProcAddress)
        || address == new IntPtr(InvalidProcAddressOne)
        || address == new IntPtr(InvalidProcAddressTwo)
        || address == new IntPtr(InvalidProcAddressThree);

    private static void FlipRows(byte[] pixels, int width, int height)
    {
        var rowSize = checked(width * sizeof(int));
        var row = new byte[rowSize];
        for (var top = 0; top < height / 2; top++)
        {
            var bottom = height - top - 1;
            pixels.AsSpan(top * rowSize, rowSize).CopyTo(row);
            pixels.AsSpan(bottom * rowSize, rowSize).CopyTo(pixels.AsSpan(top * rowSize, rowSize));
            row.AsSpan().CopyTo(pixels.AsSpan(bottom * rowSize, rowSize));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PixelFormatDescriptor
    {
        internal ushort Size, Version;
        internal int Flags;
        internal byte PixelType, ColorBits, RedBits, RedShift, GreenBits, GreenShift,
            BlueBits, BlueShift, AlphaBits, AlphaShift, AccumBits, AccumRedBits,
            AccumGreenBits, AccumBlueBits, AccumAlphaBits, DepthBits, StencilBits,
            AuxBuffers, LayerType, Reserved;
        internal int LayerMask, VisibleMask, DamageMask;
    }

    [DllImport(UserLibrary, CharSet = CharSet.Unicode)] private static extern IntPtr CreateWindowEx(int exStyle, string className,
        string windowName, int style, int x, int y, int width, int height, IntPtr parent,
        IntPtr menu, IntPtr instance, IntPtr parameter);
    [DllImport(UserLibrary)] private static extern IntPtr GetDC(IntPtr window);
    [DllImport(UserLibrary)] private static extern int ReleaseDC(IntPtr window, IntPtr deviceContext);
    [DllImport(UserLibrary)] private static extern bool DestroyWindow(IntPtr window);
    [DllImport(Kernel32Library)] private static extern IntPtr GetModuleHandle(string? moduleName);
    [DllImport(GdiLibrary)] private static extern int ChoosePixelFormat(IntPtr deviceContext,
        ref PixelFormatDescriptor descriptor);
    [DllImport(GdiLibrary)] private static extern bool SetPixelFormat(IntPtr deviceContext,
        int format, ref PixelFormatDescriptor descriptor);
    [DllImport(OpenGlLibrary, EntryPoint = "wglCreateContext")] private static extern IntPtr WglCreateContext(IntPtr deviceContext);
    [DllImport(OpenGlLibrary, EntryPoint = "wglMakeCurrent")] private static extern bool WglMakeCurrent(IntPtr deviceContext, IntPtr context);
    [DllImport(OpenGlLibrary, EntryPoint = "wglDeleteContext")] private static extern bool WglDeleteContext(IntPtr context);
    [DllImport(OpenGlLibrary, EntryPoint = "wglGetProcAddress", CharSet = CharSet.Ansi)] private static extern IntPtr WglGetProcAddress(string name);
    [DllImport(OpenGlLibrary, EntryPoint = "glReadBuffer")] private static extern void GlReadBuffer(int mode);
    [DllImport(OpenGlLibrary, EntryPoint = "glPixelStorei")] private static extern void GlPixelStorei(uint name, int value);
    [DllImport(OpenGlLibrary, EntryPoint = "glReadPixels")] private static extern void GlReadPixels(int x, int y, int width, int height, uint format, uint type, IntPtr pixels);
    [DllImport(OpenGlLibrary, EntryPoint = "glFinish")] private static extern void GlFinish();
    [DllImport(Kernel32Library)] private static extern IntPtr GetProcAddress(IntPtr module, string name);

    private const string Kernel32Library = "kernel32.dll";
}
