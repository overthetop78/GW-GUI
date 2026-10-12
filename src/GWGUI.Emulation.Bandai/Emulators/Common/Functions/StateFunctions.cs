using GWGUI.Emulation.Bandai.Emulators.Common.Interop.Contracts;
namespace GWGUI.Emulation.Bandai.Emulators.Common.Functions;

internal static class StateFunctions
{
    internal static bool IsAvailable(ExternalCoreExports exports)
    {
        var size = exports.GetSerializedSize();
        return size > nuint.Zero && size <= SavedStateConstants.MaximumStateSize;
    }
}
