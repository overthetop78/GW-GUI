using GWGUI.Emulation.Apple.Emulators.Common.Exceptions;
using GWGUI.Emulation.Apple.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Interop;
using System.IO;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Apple.Emulators.Common.Interop.Functions;

internal static class DiskControlFunctions
{
    internal static T RequiredDelegate<T>(nint pointer) where T : Delegate => pointer == nint.Zero ? throw new InvalidOperationException(DiskControlErrors.Incomplete) : Marshal.GetDelegateForFunctionPointer<T>(pointer);

    internal static T? OptionalDelegate<T>(nint pointer) where T : Delegate => pointer == nint.Zero ? null : Marshal.GetDelegateForFunctionPointer<T>(pointer);

    internal static string? ReadText<T>(T? getter, int index) where T : Delegate
    => GWGUI.Emulation.Functions.ExternalCoreEnvironmentFunctions.ReadDiskText(getter, index, DiskControlConstants.TextBufferSize);
}
