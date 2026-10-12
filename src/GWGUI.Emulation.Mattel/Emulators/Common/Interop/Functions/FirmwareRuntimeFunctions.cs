using System.IO;

namespace GWGUI.Emulation.Mattel.Emulators.Common.Interop.Functions;

internal static class FirmwareRuntimeFunctions
{
    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        Directory.CreateDirectory(systemDirectory);
        foreach (var role in FirmwareFunctions.Roles(configuration.Model))
        {
            var firmware = configuration.Firmwares.SingleOrDefault(item => item.Category == role);
            if (firmware is null || !File.Exists(firmware.Path))
                throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareMissing, ErrorMessages.RequiredFirmwareMissing);
            var candidate = FirmwareFunctions.ScanFileAsync(firmware.Path, configuration.Model, configuration.Core, CancellationToken.None).AsTask().GetAwaiter().GetResult();
            if (candidate.Compatibility != EmulationFirmwareCompatibility.Compatible
                || candidate.DestinationFieldId != FirmwareFunctions.Field(role))
                throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareFileUnreadable);
            var source = Path.GetFullPath(firmware.Path);
            var target = Path.Combine(systemDirectory, role == FirmwareCategory.Executive ? MediaRuntimeConstants.ExecutiveFileName : MediaRuntimeConstants.GraphicsFileName);
            if (!source.Equals(Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase))
                File.Copy(source, target, overwrite: true);
        }
    }
}
