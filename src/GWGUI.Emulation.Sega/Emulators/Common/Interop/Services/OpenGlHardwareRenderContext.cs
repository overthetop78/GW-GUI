using CommonCoreHostFunctions = GWGUI.Emulation.Sega.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Exceptions;

namespace GWGUI.Emulation.Sega.Emulators.Common.Interop.Services;

/// <summary>Adaptation du propriétaire graphique commun aux erreurs du cœur.</summary>
/// <remarks>Le contexte est obtenu par Common. La session attend l’arrêt du cœur puis libère ce propriétaire dans son finally sur le thread graphique.</remarks>
internal sealed class OpenGlHardwareRenderContext(ExternalCoreApi.HardwareRenderCallback callback) : IDisposable
{
    internal const int OpenGlContextType = ExternalCoreApiConstants.OpenGlContextType;
    internal const int OpenGlCoreContextType = ExternalCoreApiConstants.OpenGlCoreContextType;
    private readonly IEmulationHardwareRenderContext _context = CommonCoreHostFunctions.NativeResources.CreateHardwareRenderContext(callback, CoreExceptions.HostConfigurationInvalid);

    internal nint CurrentFramebufferCallback => _context.CurrentFramebufferCallback;
    internal nint ProcAddressCallback => _context.ProcAddressCallback;
    internal int ContextType => _context.ContextType;
    internal uint VersionMajor => _context.VersionMajor;
    internal uint VersionMinor => _context.VersionMinor;
    internal bool TryReadFramebuffer(int width, int height, out byte[] pixels) => _context.TryReadFramebuffer(width, height, out pixels);
    internal bool MakeCurrentForCore() => _context.MakeCurrentForCore();
    internal void EnsureFramebuffer(int width, int height) => _context.EnsureFramebuffer(width, height);
    public void Dispose() => _context.Dispose();
}
