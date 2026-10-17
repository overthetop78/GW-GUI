using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Thomson.Emulators.Theodore.Interop.Contracts;

namespace GWGUI.Emulation.Thomson.Emulators.Theodore.Interop.Functions;
internal static class CoreFunctions
{
    internal static ExternalCoreExports ResolveExports(ExternalCoreLibrary library) => library.ResolveExports(requireLoadGameSpecial: true);
}