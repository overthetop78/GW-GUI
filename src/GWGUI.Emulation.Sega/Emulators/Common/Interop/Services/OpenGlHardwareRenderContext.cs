using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Services;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Exceptions;

namespace GWGUI.Emulation.Sega.Emulators.Common.Interop.Services;

internal sealed class OpenGlHardwareRenderContext(ExternalCoreApi.HardwareRenderCallback callback)
    : EmulationOpenGlHardwareRenderContext(callback, CoreExceptions.HostConfigurationInvalid);
