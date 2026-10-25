using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;

internal static class SubsystemContracts
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool LoadGameSpecial(uint type, nint gameInfos, nuint count);
}
