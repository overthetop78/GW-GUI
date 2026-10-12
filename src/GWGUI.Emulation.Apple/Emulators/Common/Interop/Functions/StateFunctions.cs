using GWGUI.Emulation.Apple.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Interop;
namespace GWGUI.Emulation.Apple.Emulators.Common.Interop.Functions;

internal static class StateFunctions
{
    internal static bool IsAvailable(ExternalCoreExports exports)
    {
        var size = exports.GetSerializedSize();
        return size > nuint.Zero && size <= SavedStateConstants.MaximumStateSize;
    }
}

