using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Services;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Exceptions;

namespace GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Services;

internal sealed class OpenGlHardwareRenderContext(ExternalCoreApi.HardwareRenderCallback callback)
    : EmulationOpenGlHardwareRenderContext(callback, CoreExceptions.HostConfigurationInvalid);
