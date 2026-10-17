using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Watara.Emulators.Potator.Interop.Contracts;

namespace GWGUI.Emulation.Watara.Emulators.Potator.Interop.Functions;
internal static class CoreFunctions
{
    internal static ExternalCoreExports ResolveExports(ExternalCoreLibrary library) => library.ResolveExports(requireLoadGameSpecial: true);
}