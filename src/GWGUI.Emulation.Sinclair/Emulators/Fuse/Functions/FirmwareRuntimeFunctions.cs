using System.IO;
using GWGUI.Emulation.Sinclair.Emulators.Fuse.Constants;
using FirmwareCopy = GWGUI.Emulation.Sinclair.Emulators.Common.Interop.Functions.FirmwareRuntimeFunctions;

namespace GWGUI.Emulation.Sinclair.Emulators.Fuse.Functions;

internal static class FirmwareRuntimeFunctions
{
    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        var directory = Path.Combine(systemDirectory, FirmwareRuntimeConstants.SystemDirectory);
        foreach (var role in FirmwareFunctions.Roles(configuration.Model)) FirmwareCopy.CopyVerified(configuration, role, Path.Combine(directory, FirmwareFunctions.FileName(role)));
    }
}
