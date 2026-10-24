namespace GWGUI.Emulation.Panasonic.Common.Exceptions;

internal static class CoreHostErrors
{
    internal static string InvalidConfiguration => "The emulation configuration is invalid.";
    internal static string NotInitialized => "The emulation host is not initialized.";
    internal static string AlreadyInitialized => "The emulation host is already initialized.";
    internal static string ExecutableMissing => "The emulation host executable is missing.";
    internal static string ProcessStartFailed => "The emulation host process could not start.";
    internal static string ProcessUnavailable => "The emulation host process is unavailable.";
    internal static string ProcessNotInitialized => "The emulation host process is not initialized.";
    internal static string SharedVideoUnavailable => "The shared video transport is unavailable.";
    internal static string ResponseUnavailable => "The emulation host response is unavailable.";
    internal static string ResponseTimeout => "The emulation host response timed out.";
    internal static string RequestCancelled => "The emulation host request was cancelled.";
    internal static string CommunicationFailed => "The emulation host communication failed.";
    internal static string ProtocolVersionMismatchFormat => "Protocol version {0} does not match {1}.";
    internal static string UnknownCommandFormat => "Unknown emulation host command: {0}.";
    internal static string InvalidResponseLengthFormat => "Invalid emulation host response length: {0}.";
    internal static string ProcessExitSuffixFormat => " Exit code: {0}.";
}
