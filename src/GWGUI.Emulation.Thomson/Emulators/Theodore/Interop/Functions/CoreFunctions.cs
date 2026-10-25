using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Thomson.Emulators.Theodore.Interop.Contracts;

namespace GWGUI.Emulation.Thomson.Emulators.Theodore.Interop.Functions;
internal static class CoreFunctions
{
    internal static ExternalCoreExports ResolveExports(IEmulationCoreLibrary library) => library.ResolveExports(requireLoadGameSpecial: true);
}