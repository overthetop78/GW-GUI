using GWGUI.Emulation.Sharp.Emulators.Common.Interop.Functions;
using System.IO;
using GWGUI.Emulation.Sharp.Emulators.Px68k.Constants;
using SharedFirmwareRuntimeFunctions = GWGUI.Emulation.Sharp.Emulators.Common.Interop.Functions.FirmwareRuntimeFunctions;

namespace GWGUI.Emulation.Sharp.Emulators.Px68k.Functions;

internal static class FirmwareRuntimeFunctions
{
    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        var directory = Path.Combine(systemDirectory, FirmwareRuntimeConstants.SystemDirectory);
        SharedFirmwareRuntimeFunctions.CopyVerified(configuration, FirmwareCategory.Ipl, Path.Combine(directory, FirmwareRuntimeConstants.IplFileName));
        SharedFirmwareRuntimeFunctions.CopyVerified(configuration, FirmwareCategory.Font, Path.Combine(directory, FirmwareRuntimeConstants.FontFileName));
    }
}
