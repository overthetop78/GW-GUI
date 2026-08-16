namespace GWGUI.Emulation.Atari;

public enum AtariErrorKind
{
    Core,
    Firmware,
    Content,
    Option,
    Host,
    State
}

public enum AtariErrorCode
{
    CoreNotFound,
    CoreRejected,
    FirmwareMissing,
    FirmwareInvalid,
    ContentNotFound,
    ContentUnsupported,
    OptionInvalid,
    HostProtocolFailure,
    StateInvalid,
    StateIncompatible
}

public sealed class AtariEmulationException : Exception
{
    public AtariEmulationException(AtariErrorKind kind, AtariErrorCode code, string message,
        IReadOnlyDictionary<string, string>? context = null, Exception? innerException = null)
        : base(message, innerException)
    {
        Kind = kind;
        Code = code;
        Context = context ?? new Dictionary<string, string>();
    }

    public AtariErrorKind Kind { get; }
    public AtariErrorCode Code { get; }
    public IReadOnlyDictionary<string, string> Context { get; }
}
