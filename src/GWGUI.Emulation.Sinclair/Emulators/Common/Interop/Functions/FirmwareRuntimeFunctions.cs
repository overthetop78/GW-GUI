using System.IO;

namespace GWGUI.Emulation.Sinclair.Emulators.Common.Interop.Functions;

internal static class FirmwareRuntimeFunctions
{
    internal static void Prepare(MachineConfiguration configuration, string systemDirectory) => ((LibretroMachineFactory)CoreCatalog.CreateAdapter(configuration.Core)).PrepareFirmware(configuration, systemDirectory);

    internal static void CopyVerified(MachineConfiguration configuration, FirmwareCategory role, string target)
    {
        var firmware = configuration.Firmwares.SingleOrDefault(item => item.Category == role);
        if (firmware is null)
        {
            if (FirmwareFunctions.IsRequired(configuration.Model, role)) throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareMissing, ErrorMessages.RequiredFirmwareMissing);
            return;
        }
        if (!File.Exists(firmware.Path)) throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareMissing, ErrorMessages.FirmwareFileMissing);
        var candidate = FirmwareFunctions.ScanFileAsync(firmware.Path, configuration.Model, configuration.Core, CancellationToken.None).AsTask().GetAwaiter().GetResult();
        if (candidate.Compatibility is not (EmulationFirmwareCompatibility.Compatible or EmulationFirmwareCompatibility.PartiallyCompatible) || candidate.DestinationFieldId != FirmwareFunctions.Field(role)) throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareFileUnreadable);
        var source = Path.GetFullPath(firmware.Path);
        var destination = Path.GetFullPath(target);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        if (!string.Equals(source, destination, StringComparison.OrdinalIgnoreCase)) File.Copy(source, destination, overwrite: true);
    }
}
