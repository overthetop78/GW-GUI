namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

internal static partial class FirmwareFunctions
{
    internal static ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFileAsync(IEmulatorFirmwareAdapter adapter, MachineConfiguration configuration, string path, CancellationToken cancellationToken) => adapter.ScanFirmwareFileAsync(configuration, path, cancellationToken);

    internal static FirmwareConfiguration Select(EmulationFirmwareCandidate candidate)
    {
        if (candidate.Compatibility == EmulationFirmwareCompatibility.Incompatible
            || candidate.DestinationFieldId != SettingsConstants.Firmware)
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareCannotBeSelected);
        return new(FirmwareCategory.SystemRom, candidate.Path, true);
    }
}
