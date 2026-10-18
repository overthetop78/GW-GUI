using GWGUI.Emulation.Acorn.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Acorn.Emulators.Common.Exceptions;
using System.IO;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Acorn.Emulators.Common.Interop.Functions;

internal static class DiskControlFunctions
{
    internal static T RequiredDelegate<T>(nint pointer) where T : Delegate => pointer == nint.Zero ? throw new InvalidOperationException(DiskControlErrors.Incomplete) : Marshal.GetDelegateForFunctionPointer<T>(pointer);

    internal static string? ReadText<T>(T? getter, int index) where T : Delegate
    => GWGUI.Emulation.Functions.ExternalCoreEnvironmentFunctions.ReadDiskText(getter, index, DiskControlConstants.TextBufferSize);
}

