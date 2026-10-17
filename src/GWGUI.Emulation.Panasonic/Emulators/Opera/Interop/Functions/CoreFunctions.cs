using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Panasonic.Emulators.Opera.Interop.Contracts;

namespace GWGUI.Emulation.Panasonic.Emulators.Opera.Interop.Functions;
internal static class CoreFunctions
{
    internal static ExternalCoreExports ResolveExports(ExternalCoreLibrary library) => library.ResolveExports(requireLoadGameSpecial: true);
}