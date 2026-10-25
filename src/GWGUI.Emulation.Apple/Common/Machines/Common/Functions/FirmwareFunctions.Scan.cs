namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

internal static partial class FirmwareFunctions
{

    internal static FirmwareConfiguration Select(EmulationFirmwareCandidate candidate)
    {
        if (candidate.Compatibility == EmulationFirmwareCompatibility.Incompatible
            || candidate.DestinationFieldId != SettingsConstants.Firmware)
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareCannotBeSelected);
        return new(FirmwareCategory.SystemRom, candidate.Path, true);
    }
}
