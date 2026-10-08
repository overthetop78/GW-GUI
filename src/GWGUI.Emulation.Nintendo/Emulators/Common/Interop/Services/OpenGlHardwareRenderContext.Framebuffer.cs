using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Services;

internal sealed partial class OpenGlHardwareRenderContext
{
    private const int SingleGraphicsObject = 1;
    private const int TextureBaseLevel = 0;
    private const int TextureBorderWidth = 0;
    private const int AttributeListTerminator = 0;
    private const uint FramebufferTarget = 0x8D40;
    private const uint RenderbufferTarget = 0x8D41;
    private const uint TextureTarget = 0x0DE1;
    private const int ColorAttachment = 0x8CE0;
    private const uint DepthStencilAttachment = 0x821A;
    private const uint Depth24Stencil8 = 0x88F0;
    private const int Rgba8 = 0x8058;
    private const uint Rgba = 0x1908;
    private const uint FramebufferComplete = 0x8CD5;
    private const int InitialFramebufferSize = 2048;
    private const int ContextMajorVersionAttribute = 0x2091;
    private const int ContextMinorVersionAttribute = 0x2092;
    private const int ContextProfileMaskAttribute = 0x9126;
    private const int CoreProfileMask = 1;
    internal const int OpenGlCoreContextType = 3;
    private const string CreateContextAttributesFunction = "wglCreateContextAttribsARB";
    private const string GenFramebuffersFunction = "glGenFramebuffers";
    private const string BindFramebufferFunction = "glBindFramebuffer";
    private const string FramebufferTextureFunction = "glFramebufferTexture2D";
    private const string CheckFramebufferFunction = "glCheckFramebufferStatus";
    private const string GenRenderbuffersFunction = "glGenRenderbuffers";
    private const string BindRenderbufferFunction = "glBindRenderbuffer";
    private const string RenderbufferStorageFunction = "glRenderbufferStorage";
    private const string FramebufferRenderbufferFunction = "glFramebufferRenderbuffer";
    private const string DeleteFramebuffersFunction = "glDeleteFramebuffers";
    private const string DeleteRenderbuffersFunction = "glDeleteRenderbuffers";
    private uint _framebuffer, _colorTexture, _depthBuffer;
    private int _framebufferWidth, _framebufferHeight;
    private BindObject? _bindFramebuffer;
    private BindObject BindFramebuffer => _bindFramebuffer ??= Function<BindObject>(BindFramebufferFunction);

    internal void EnsureFramebuffer(int width, int height)
    {
        if (_framebuffer != 0 && width == _framebufferWidth && height == _framebufferHeight) return;
        if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (!MakeCurrent()) throw new InvalidOperationException();
        DeleteFramebuffer();
        Function<GenerateObjects>(GenFramebuffersFunction)(SingleGraphicsObject, out _framebuffer);
        BindFramebuffer(FramebufferTarget, _framebuffer);
        GlGenTextures(SingleGraphicsObject, out _colorTexture);
        GlBindTexture(TextureTarget, _colorTexture);
        GlTexImage2D(TextureTarget, TextureBaseLevel, Rgba8, width, height, TextureBorderWidth, Rgba, GlUnsignedByte, nint.Zero);
        Function<AttachTexture>(FramebufferTextureFunction)(FramebufferTarget, (uint)ColorAttachment, TextureTarget, _colorTexture, TextureBaseLevel);
        Function<GenerateObjects>(GenRenderbuffersFunction)(SingleGraphicsObject, out _depthBuffer);
        Function<BindObject>(BindRenderbufferFunction)(RenderbufferTarget, _depthBuffer);
        Function<AllocateRenderbuffer>(RenderbufferStorageFunction)(RenderbufferTarget, Depth24Stencil8, width, height);
        Function<AttachRenderbuffer>(FramebufferRenderbufferFunction)(FramebufferTarget, DepthStencilAttachment, RenderbufferTarget, _depthBuffer);
        if (Function<CheckFramebuffer>(CheckFramebufferFunction)(FramebufferTarget) != FramebufferComplete)
            throw new InvalidOperationException();
        _framebufferWidth = width; _framebufferHeight = height;
    }

    private void ConfigureContext(ExternalCoreApi.HardwareRenderCallback callback)
    {
        if (callback.ContextType != OpenGlCoreContextType) return;
        var address = WglGetProcAddress(CreateContextAttributesFunction);
        if (IsInvalidProcAddress(address)) throw new PlatformNotSupportedException();
        var create = Marshal.GetDelegateForFunctionPointer<CreateContextAttributes>(address);
        var modern = create(_deviceContext, nint.Zero,
            [ContextMajorVersionAttribute, checked((int)callback.VersionMajor),
                ContextMinorVersionAttribute, checked((int)callback.VersionMinor),
                ContextProfileMaskAttribute, CoreProfileMask, AttributeListTerminator]);
        if (modern == nint.Zero) throw new PlatformNotSupportedException();
        var adopted = false;
        try
        {
            if (!WglMakeCurrent(nint.Zero, nint.Zero) || !WglDeleteContext(_renderingContext))
                throw new InvalidOperationException();
            _renderingContext = modern;
            adopted = true;
            _bindFramebuffer = null;
            if (!MakeCurrent()) throw new InvalidOperationException();
        }
        finally
        {
            if (!adopted) WglDeleteContext(modern);
        }
    }

    private void DeleteFramebuffer()
    {
        if (_renderingContext == nint.Zero) return;
        MakeCurrent();
        try
        {
            if (_framebuffer != 0) Function<DeleteObjects>(DeleteFramebuffersFunction)(SingleGraphicsObject, ref _framebuffer);
        }
        finally
        {
            try { if (_depthBuffer != 0) Function<DeleteObjects>(DeleteRenderbuffersFunction)(SingleGraphicsObject, ref _depthBuffer); }
            finally
            {
                if (_colorTexture != 0) GlDeleteTextures(SingleGraphicsObject, ref _colorTexture);
                _framebuffer = _depthBuffer = _colorTexture = 0;
            }
        }
    }

    private static T Function<T>(string name) where T : Delegate
    {
        var pointer = WglGetProcAddress(name);
        if (IsInvalidProcAddress(pointer)) throw new PlatformNotSupportedException(name);
        return Marshal.GetDelegateForFunctionPointer<T>(pointer);
    }
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate nint CreateContextAttributes(nint dc, nint shared, int[] attributes);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate void GenerateObjects(int count, out uint handle);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate void DeleteObjects(int count, ref uint handle);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate void BindObject(uint target, uint handle);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate void AttachTexture(uint target, uint attachment, uint textureTarget, uint texture, int level);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate uint CheckFramebuffer(uint target);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate void AllocateRenderbuffer(uint target, uint format, int width, int height);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate void AttachRenderbuffer(uint target, uint attachment, uint renderbufferTarget, uint renderbuffer);
    [DllImport(OpenGlLibrary, EntryPoint = "glGenTextures")] private static extern void GlGenTextures(int count, out uint texture);
    [DllImport(OpenGlLibrary, EntryPoint = "glDeleteTextures")] private static extern void GlDeleteTextures(int count, ref uint texture);
    [DllImport(OpenGlLibrary, EntryPoint = "glBindTexture")] private static extern void GlBindTexture(uint target, uint texture);
    [DllImport(OpenGlLibrary, EntryPoint = "glTexImage2D")] private static extern void GlTexImage2D(uint target, int level, int format, int width, int height, int border, uint externalFormat, uint type, nint pixels);
}
