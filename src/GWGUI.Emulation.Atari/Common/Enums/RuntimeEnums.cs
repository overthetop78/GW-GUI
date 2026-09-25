namespace GWGUI.Emulation.Atari.Common.Enums;

internal enum EnvironmentLanguage : uint
{
    English = 0, Japanese = 1, French = 2, Spanish = 3, German = 4, Italian = 5, Dutch = 6,
    PortugueseBrazil = 7, PortuguesePortugal = 8, Russian = 9, Korean = 10, ChineseTraditional = 11,
    ChineseSimplified = 12, Polish = 14, Vietnamese = 15, Arabic = 16, Greek = 17, Turkish = 18,
    Hebrew = 21, Finnish = 23, Indonesian = 24, Swedish = 25, Ukrainian = 26, Czech = 27,
    Hungarian = 31, Norwegian = 34, Thai = 36
}

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

public enum RuntimeRegion { Ntsc, Pal }

public enum ShortcutAvailability { Available, Unavailable }
