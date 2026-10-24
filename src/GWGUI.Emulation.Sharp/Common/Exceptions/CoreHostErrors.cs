namespace GWGUI.Emulation.Sharp.Common.Exceptions;

internal static class CoreHostErrors
{
    internal static string InvalidConfiguration => HostFailure();
    internal static string NotInitialized => HostFailure();
    internal static string AlreadyInitialized => HostFailure();
    internal static string ExecutableMissing => ExceptionText.Get(CoreErrorResourceKeys.HostExecutableMissing);
    internal static string ProcessStartFailed => HostFailure();
    internal static string ProcessUnavailable => HostFailure();
    internal static string ProcessNotInitialized => HostFailure();
    internal static string SharedVideoUnavailable => HostFailure();
    internal static string ResponseUnavailable => HostFailure();
    internal static string ResponseTimeout => HostFailure();
    internal static string RequestCancelled => HostFailure();
    internal static string CommunicationFailed => HostFailure();
    internal static string ProtocolVersionMismatchFormat => HostFailure();
    internal static string UnknownCommandFormat => HostFailure();
    internal static string InvalidResponseLengthFormat => HostFailure();
    internal static string ProcessExitSuffixFormat => ExceptionText.Get(CoreErrorResourceKeys.Details);

    private static string HostFailure() => ExceptionText.Get(CoreErrorResourceKeys.HostProtocolFailure);
}
