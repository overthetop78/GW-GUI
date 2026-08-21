namespace GWGUI.Emulation;

public enum EmulationMessageCode
{
    UntranslatedEmulatorMessage,
    EmulatorNotInstalled,
    EmulatorRejected,
    EmulatorUpdateBlocked,
    RequiredMediaMissing,
    FirmwareMissing,
    FirmwareIncompatible,
    MediaNotFound,
    MediaUnsupported,
    OptionInvalid,
    HostCommunicationFailed,
    MachineStartFailed,
    MediaOperationFailed,
    SavedStateInvalid,
    SavedStateIncompatible,
    SavedStateOperationFailed
}
