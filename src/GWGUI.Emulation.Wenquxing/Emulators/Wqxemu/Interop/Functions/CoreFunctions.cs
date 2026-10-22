using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Interop.Contracts;

namespace GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Interop.Functions;
internal static class CoreFunctions
{
    internal static ExternalCoreExports ResolveExports(IEmulationCoreLibrary library) => library.ResolveExports(requireLoadGameSpecial: true);
}