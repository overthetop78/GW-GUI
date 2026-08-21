using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

internal static class AtariMessageFunctions
{
    internal static Exception Translate(Exception error, AtariMachineConfiguration configuration)
    {
        if (error is not AtariEmulationException atariError) return error;
        if (atariError.Code != AtariErrorCode.ContentRequired)
            return new EmulationMessageException(new EmulationMessage(
                Category(atariError.Category), MessageCode(atariError.Code),
                EmulationMessageSeverity.Error, EmulationMessageTarget.Dialog,
                new EmulationMachineMessageContext(configuration.MachineId)), error);
        var required = AtariCompatibilityCatalog.Get(configuration.Model).Media
            .Where(rule => rule.Availability == AtariMediaAvailability.Available
                && rule.Category != AtariMediaCategory.Directory)
            .Select(rule => rule.Category switch
            {
                AtariMediaCategory.Floppy => EmulationMediaCategory.FloppyDrive,
                AtariMediaCategory.HardDisk => EmulationMediaCategory.HardDisk,
                AtariMediaCategory.CompactDisc => EmulationMediaCategory.CompactDiscDrive,
                AtariMediaCategory.Cartridge => EmulationMediaCategory.CartridgeSlot,
                AtariMediaCategory.Cassette => EmulationMediaCategory.CassetteDrive,
                _ => throw new ArgumentOutOfRangeException(nameof(rule))
            })
            .Distinct()
            .ToArray();
        return new EmulationMessageException(new EmulationMessage(
            EmulationMessageCategory.Media,
            EmulationMessageCode.RequiredMediaMissing,
            EmulationMessageSeverity.Error,
            EmulationMessageTarget.Dialog,
            new EmulationRequiredMachineMediaMessageContext(configuration.MachineId, required)), error);
    }

    private static EmulationMessageCategory Category(AtariErrorCategory category) => category switch
    {
        AtariErrorCategory.Core or AtariErrorCategory.Host => EmulationMessageCategory.Emulator,
        AtariErrorCategory.Firmware => EmulationMessageCategory.Firmware,
        AtariErrorCategory.Content => EmulationMessageCategory.Media,
        AtariErrorCategory.Option => EmulationMessageCategory.Machine,
        AtariErrorCategory.State => EmulationMessageCategory.SavedState,
        _ => EmulationMessageCategory.Machine
    };

    private static EmulationMessageCode MessageCode(AtariErrorCode code) => code switch
    {
        AtariErrorCode.CoreNotFound => EmulationMessageCode.EmulatorNotInstalled,
        AtariErrorCode.CoreRejected => EmulationMessageCode.EmulatorRejected,
        AtariErrorCode.FirmwareMissing => EmulationMessageCode.FirmwareMissing,
        AtariErrorCode.FirmwareInvalid => EmulationMessageCode.FirmwareIncompatible,
        AtariErrorCode.ContentNotFound => EmulationMessageCode.MediaNotFound,
        AtariErrorCode.ContentUnsupported => EmulationMessageCode.MediaUnsupported,
        AtariErrorCode.OptionInvalid => EmulationMessageCode.OptionInvalid,
        AtariErrorCode.HostProtocolFailure => EmulationMessageCode.HostCommunicationFailed,
        AtariErrorCode.StateInvalid => EmulationMessageCode.SavedStateInvalid,
        AtariErrorCode.StateIncompatible => EmulationMessageCode.SavedStateIncompatible,
        _ => EmulationMessageCode.MachineStartFailed
    };
}
