using System.Runtime.InteropServices;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Sega.Emulators.Common.Interop.Services;

internal sealed partial class ExternalHostCallbacks
{
    private OpenGlHardwareRenderContext? _hardwareRenderContext;
    private ExternalCoreApi.HardwareContextReset? _coreContextReset;
    private ExternalCoreApi.HardwareContextReset? _coreContextDestroy;
    private bool _hardwareContextReset;

    private bool ConfigureHardwareRender(nint data)
    {
        if (data == nint.Zero) return false;
        var callback = Marshal.PtrToStructure<ExternalCoreApi.HardwareRenderCallback>(data);
        if (callback.ContextType is not (OpenGlHardwareRenderContext.OpenGlContextType
            or OpenGlHardwareRenderContext.OpenGlCoreContextType)) return false;
        var context = new OpenGlHardwareRenderContext(callback);
        try
        {
            DestroyHardwareContext();
            if (!context.MakeCurrentForCore()) throw new InvalidOperationException();
            _coreContextReset = callback.ContextReset == nint.Zero ? null
                : Marshal.GetDelegateForFunctionPointer<ExternalCoreApi.HardwareContextReset>(callback.ContextReset);
            _coreContextDestroy = callback.ContextDestroy == nint.Zero ? null
                : Marshal.GetDelegateForFunctionPointer<ExternalCoreApi.HardwareContextReset>(callback.ContextDestroy);
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

    internal void ResetHardwareContext()
    {
        if (_hardwareRenderContext?.MakeCurrentForCore() != true) return;
        _coreContextReset?.Invoke();
        _hardwareContextReset = true;
    }

    internal void NotifyHardwareContextDestroy()
    {
        if (!_hardwareContextReset) return;
        _hardwareContextReset = false;
        if (_hardwareRenderContext?.MakeCurrentForCore() == true)
            _coreContextDestroy?.Invoke();
    }

    internal void DestroyHardwareContext()
    {
        try
        {
            NotifyHardwareContextDestroy();
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
