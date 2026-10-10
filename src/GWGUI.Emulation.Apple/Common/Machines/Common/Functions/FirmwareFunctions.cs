using System.IO;
using MiniVmacConstants = GWGUI.Emulation.Apple.Emulators.MiniVmac.Constants.EmulatorConstants;

namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

internal static partial class FirmwareFunctions
{
    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        Directory.CreateDirectory(systemDirectory);
        if (configuration.Core == Emulator.AppleWin) return;
        var firmware = configuration.Firmwares.SingleOrDefault(item => item.Category == FirmwareCategory.SystemRom);
        if (firmware is null || !File.Exists(firmware.Path))
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareMissing,
                ErrorMessages.RequiredFirmwareMissing);
        var source = Path.GetFullPath(firmware.Path);
        if (new FileInfo(source).Length < MiniVmacConstants.MinimumRomBytes)
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid,
                ErrorMessages.FirmwareFileUnreadable);
        var destination = Path.Combine(systemDirectory, MiniVmacConstants.RomFileName);
        if (!string.Equals(source, Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase))
            File.Copy(source, destination, overwrite: true);
    }
}
