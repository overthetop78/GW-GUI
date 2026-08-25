using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Atari.Functions;

internal static class AtariDiskControlFunctions
{
    internal static T RequiredDelegate<T>(nint pointer) where T : Delegate => pointer == nint.Zero
        ? throw new InvalidOperationException(AtariDiskControlErrors.Incomplete)
        : Marshal.GetDelegateForFunctionPointer<T>(pointer);

    internal static T? OptionalDelegate<T>(nint pointer) where T : Delegate => pointer == nint.Zero
        ? null
        : Marshal.GetDelegateForFunctionPointer<T>(pointer);

    internal static string? ReadText<T>(T? getter, int index) where T : Delegate
    {
        if (getter is null || index < AtariDiskControlConstants.FirstImageIndex) return null;
        var buffer = Marshal.AllocHGlobal(AtariDiskControlConstants.TextBufferSize);
        try
        {
            var success = getter switch
            {
                ExternalCoreApi.GetImagePath path => path((uint)index, buffer,
                    AtariDiskControlConstants.TextBufferSize),
                ExternalCoreApi.GetImageLabel label => label((uint)index, buffer,
                    AtariDiskControlConstants.TextBufferSize),
                _ => false
            };
            return success ? Marshal.PtrToStringUTF8(buffer) : null;
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }
}
