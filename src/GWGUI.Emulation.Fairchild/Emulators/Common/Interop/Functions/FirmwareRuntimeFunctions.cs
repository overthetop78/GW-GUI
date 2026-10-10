using System.IO;

namespace GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Functions;

internal static class FirmwareRuntimeFunctions
{
    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        foreach (var firmware in configuration.Firmwares)
        {
            if (!File.Exists(firmware.Path))
                throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareMissing, ErrorMessages.RequiredFirmwareMissing);
            if (new FileInfo(firmware.Path).Length != MediaRuntimeConstants.BiosBytes)
                throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareFileUnreadable);
            var source = Path.GetFullPath(firmware.Path);
            var destination = Path.Combine(systemDirectory, MediaRuntimeConstants.FileName(configuration.Model, firmware.Category));
            if (!string.Equals(source, Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase))
                File.Copy(source, destination, overwrite: true);
        }
        // Missing BIOS parts are handled by the current core's experimental HLE implementation.
    }
}
