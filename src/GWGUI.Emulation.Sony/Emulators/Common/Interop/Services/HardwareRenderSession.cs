using CommonCoreHostFunctions = GWGUI.Emulation.Sony.Common.Functions.CoreHostFunctions;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

internal sealed class HardwareRenderSession : IDisposable
{
    private OpenGlHardwareRenderContext? _hardwareRenderContext;
    private ExternalCoreApi.HardwareContextReset? _coreContextReset;
    private ExternalCoreApi.HardwareContextReset? _coreContextDestroy;
    private bool _hardwareContextReset;

    internal bool Configure(nint data)
    {
        if (data == nint.Zero) return false;
        var callback = Marshal.PtrToStructure<ExternalCoreApi.HardwareRenderCallback>(data);
        if (callback.ContextType is not (OpenGlHardwareRenderContext.OpenGlContextType
            or OpenGlHardwareRenderContext.OpenGlCoreContextType)) return false;
        var context = new OpenGlHardwareRenderContext(callback);
        try
        {
            Dispose();
            if (!context.MakeCurrentForCore()) throw new InvalidOperationException();
            _coreContextReset = CommonCoreHostFunctions.EnvironmentProcessing.OptionalDelegate<ExternalCoreApi.HardwareContextReset>(callback.ContextReset);
            _coreContextDestroy = CommonCoreHostFunctions.EnvironmentProcessing.OptionalDelegate<ExternalCoreApi.HardwareContextReset>(callback.ContextDestroy);
            callback.GetCurrentFramebuffer = context.CurrentFramebufferCallback;
            callback.GetProcAddress = context.ProcAddressCallback;
            Marshal.StructureToPtr(callback, data, false);
            _hardwareRenderContext = context;
            return true;
        }
        finally
        {
            if (!ReferenceEquals(_hardwareRenderContext, context)) context.Dispose();
        }
    }

    internal void Reset()
    {
        if (_hardwareRenderContext?.MakeCurrentForCore() != true) return;
        _hardwareContextReset = true;
        _coreContextReset?.Invoke();
    }

    internal void NotifyDestroy()
    {
        if (!_hardwareContextReset) return;
        _hardwareContextReset = false;
        if (_hardwareRenderContext?.MakeCurrentForCore() == true)
            _coreContextDestroy?.Invoke();
    }

    internal bool TryReadFramebuffer(int width, int height, out byte[] pixels)
    {
        pixels = [];
        return _hardwareRenderContext?.TryReadFramebuffer(width, height, out pixels) == true;
    }

    internal void Resize(int width, int height) => _hardwareRenderContext?.EnsureFramebuffer(width, height);

    public void Dispose()
    {
        try
        {
            NotifyDestroy();
        }
        finally
        {
            try { _hardwareRenderContext?.Dispose(); }
            finally
            {
                _hardwareRenderContext = null;
                _coreContextReset = _coreContextDestroy = null;
                _hardwareContextReset = false;
            }
        }
    }
}
