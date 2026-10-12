namespace GWGUI.Emulation.Wenquxing.Common.Exceptions;

internal static class CoreHostErrors
{
    internal const string InvalidConfiguration = "The emulation configuration is invalid.";
    internal const string NotInitialized = "The emulation host is not initialized.";
    internal const string AlreadyInitialized = "The emulation host is already initialized.";
    internal const string ExecutableMissing = "The emulation host executable is missing.";
    internal const string ProcessStartFailed = "The emulation host process could not start.";
    internal const string ProcessUnavailable = "The emulation host process is unavailable.";
    internal const string ProcessNotInitialized = "The emulation host process is not initialized.";
    internal const string SharedVideoUnavailable = "The shared video transport is unavailable.";
    internal const string ResponseUnavailable = "The emulation host response is unavailable.";
    internal const string ResponseTimeout = "The emulation host response timed out.";
    internal const string RequestCancelled = "The emulation host request was cancelled.";
    internal const string CommunicationFailed = "The emulation host communication failed.";
    internal const string ProtocolVersionMismatchFormat = "Protocol version {0} does not match {1}.";
    internal const string UnknownCommandFormat = "Unknown emulation host command: {0}.";
    internal const string InvalidResponseLengthFormat = "Invalid emulation host response length: {0}.";
    internal const string ProcessExitSuffixFormat = " Exit code: {0}.";
}
