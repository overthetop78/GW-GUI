using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Interop.Contracts;

namespace GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Interop.Functions;
internal static class CoreFunctions
{
    internal static ExternalCoreExports ResolveExports(ExternalCoreLibrary library) => library.ResolveExports(requireLoadGameSpecial: true);
}