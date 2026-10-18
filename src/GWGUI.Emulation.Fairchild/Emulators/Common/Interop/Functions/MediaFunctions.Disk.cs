using GWGUI.Emulation.Fairchild.Emulators.Common.Exceptions;
using GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Functions;

internal static class DiskControlFunctions
{
    internal static T RequiredDelegate<T>(nint pointer) where T : Delegate => GWGUI.Emulation.Functions.ExternalCoreEnvironmentFunctions.RequiredDelegate<T>(pointer, static () => new InvalidOperationException(DiskControlErrors.Incomplete));

    internal static string? ReadText<T>(T? getter, int index) where T : Delegate
    => GWGUI.Emulation.Functions.ExternalCoreEnvironmentFunctions.ReadDiskText(getter, index, DiskControlConstants.TextBufferSize);
}
