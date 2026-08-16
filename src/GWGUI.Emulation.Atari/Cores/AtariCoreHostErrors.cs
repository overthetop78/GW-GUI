namespace GWGUI.Emulation.Atari.Cores;

internal static class AtariCoreHostErrors
{
    internal const string InvalidConfiguration = "The Atari host configuration is invalid.";
    internal const string NotInitialized = "The Atari host is not initialized.";
    internal const string AlreadyInitialized = "The Atari core process is already initialized.";
    internal const string ExecutableMissing = "The GW GUI executable used to host the Atari core was not found.";
    internal const string ProcessStartFailed = "The Atari core host process could not be started.";
    internal const string ProcessUnavailable = "The Atari core process is no longer available.";
    internal const string ProcessNotInitialized = "The Atari core process is not initialized.";
    internal const string SharedVideoUnavailable = "The shared Atari video buffer is unavailable.";
    internal const string ResponseUnavailable = "The Atari host response is unavailable.";
    internal const string ResponseTimeout = "The Atari core process did not answer within the allowed time and was stopped.";
    internal const string RequestCancelled = "Communication with the Atari core process was cancelled and the process was stopped.";
    internal const string CommunicationFailed = "Communication with the Atari core process failed.";
    internal const string ProtocolVersionMismatchFormat = "The Atari host protocol version {0} is not supported; expected {1}.";
    internal const string UnknownCommandFormat = "Unknown Atari host command {0}.";
    internal const string InvalidResponseLengthFormat = "The Atari core process sent invalid response length {0}.";
    internal const string ProcessExitSuffixFormat = " It exited with code {0}.";
}
