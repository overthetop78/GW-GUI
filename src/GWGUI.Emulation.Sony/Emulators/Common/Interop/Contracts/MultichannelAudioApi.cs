using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;

internal static class MultichannelAudioApi
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate nuint BatchInt16(nint data, nuint frames, uint channels, uint layout);

    [StructLayout(LayoutKind.Sequential)]
    internal struct Callback
    {
        internal nint Int16;
        internal nint Float;
    }
}
