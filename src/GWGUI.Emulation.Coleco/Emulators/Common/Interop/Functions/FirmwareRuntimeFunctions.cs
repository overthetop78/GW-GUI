using GWGUI.Emulation.Coleco.Emulators.BlueMSX.Functions;
using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Constants;
using System.IO;

namespace GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions;

internal static class FirmwareRuntimeFunctions
{
    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        Copy(configuration, FirmwareCategory.Os7, FirmwareConstants.Os7Bytes, MediaRuntimeConstants.Os7FileName, systemDirectory);
        if (configuration.Model == MachineModel.Adam)
        {
            Copy(configuration, FirmwareCategory.Eos, FirmwareConstants.EosBytes, MediaRuntimeConstants.EosFileName, systemDirectory);
            Copy(configuration, FirmwareCategory.SmartWriter, FirmwareConstants.SmartWriterBytes, MediaRuntimeConstants.SmartWriterFileName, systemDirectory);
        }
        if (configuration.Core == Emulator.BlueMSX)
            GWGUI.Emulation.Coleco.Emulators.BlueMSX.Functions.SystemFilesFunctions.Prepare(configuration, systemDirectory);
    }

    private static void Copy(MachineConfiguration configuration, FirmwareCategory category, int length, string name, string directory)
    {
        var firmware = configuration.Firmwares.SingleOrDefault(value => value.Category == category);
        if (firmware is null || !File.Exists(firmware.Path))
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareMissing, ErrorMessages.RequiredFirmwareMissing);
        if (new FileInfo(firmware.Path).Length != length)
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareFileUnreadable);
        var source = Path.GetFullPath(firmware.Path);
        var destination = Path.Combine(directory, name);
        if (!string.Equals(source, Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase))
            File.Copy(source, destination, overwrite: true);
    }
}
