using System.IO;

namespace GWGUI.Emulation.Epoch.Emulators.Common.Interop.Functions;

internal static class FirmwareRuntimeFunctions
{
    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        if (configuration.Core == Emulator.PD777) return;
        var firmware = configuration.Firmwares.SingleOrDefault(value => value.Category == FirmwareCategory.Console);
        if (firmware is null || !File.Exists(firmware.Path))
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareMissing, ErrorMessages.RequiredFirmwareMissing);
        if (new FileInfo(firmware.Path).Length != MediaRuntimeConstants.ConsoleFirmwareBytes)
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareFileUnreadable);
        var source = Path.GetFullPath(firmware.Path);
        var destination = Path.Combine(systemDirectory, MediaRuntimeConstants.ConsoleFirmwareFileName);
        if (!string.Equals(source, Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase))
            File.Copy(source, destination, overwrite: true);
        // The core applies its native BIOS checksum policy, including the configured custom-BIOS option.
    }
}
