using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Services;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Exceptions;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

internal sealed class OpenGlHardwareRenderContext(ExternalCoreApi.HardwareRenderCallback callback)
    : EmulationOpenGlHardwareRenderContext(callback, CoreExceptions.HostConfigurationInvalid);
