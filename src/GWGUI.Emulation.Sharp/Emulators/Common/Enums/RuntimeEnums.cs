namespace GWGUI.Emulation.Sharp.Emulators.Common.Enums;

public enum ErrorCategory
{
    Core,
    Firmware,
    Content,
    Option,
    Host,
    State
}

public enum ErrorCode
{
    CoreNotFound,
    CoreRejected,
    FirmwareMissing,
    FirmwareInvalid,
    ContentNotFound,
    ContentRequired,
    ContentUnsupported,
    OptionInvalid,
    HostProtocolFailure,
    StateInvalid,
    StateIncompatible
}
