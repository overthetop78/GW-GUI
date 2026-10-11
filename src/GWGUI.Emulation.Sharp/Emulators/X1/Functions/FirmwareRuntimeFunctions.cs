using System.IO;
using GWGUI.Emulation.Sharp.Emulators.X1.Constants;
using SharedFirmwareRuntimeFunctions = GWGUI.Emulation.Sharp.Emulators.Common.Interop.Functions.FirmwareRuntimeFunctions;

namespace GWGUI.Emulation.Sharp.Emulators.X1.Functions;

internal static class FirmwareRuntimeFunctions
{
    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        var name = configuration.Model == MachineModel.X1 ? FirmwareRuntimeConstants.IplFileName : FirmwareRuntimeConstants.TurboIplFileName;
        SharedFirmwareRuntimeFunctions.CopyVerified(configuration, FirmwareCategory.Ipl, Path.Combine(systemDirectory, FirmwareRuntimeConstants.SystemDirectory, name));
    }
}
