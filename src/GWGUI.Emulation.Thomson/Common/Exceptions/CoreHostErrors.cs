namespace GWGUI.Emulation.Thomson.Common.Exceptions;

internal static class CoreHostErrors
{
    internal static string InvalidConfiguration => ExceptionText.Get(CoreErrorResourceKeys.InvalidConfiguration);
    internal static string NotInitialized => ExceptionText.Get(CoreErrorResourceKeys.NotInitialized);
    internal static string AlreadyInitialized => ExceptionText.Get(CoreErrorResourceKeys.AlreadyInitialized);
    internal static string ExecutableMissing => ExceptionText.Get(CoreErrorResourceKeys.ExecutableMissing);
    internal static string ProcessStartFailed => ExceptionText.Get(CoreErrorResourceKeys.ProcessStartFailed);
    internal static string ProcessUnavailable => ExceptionText.Get(CoreErrorResourceKeys.ProcessUnavailable);
    internal static string ProcessNotInitialized => ExceptionText.Get(CoreErrorResourceKeys.ProcessNotInitialized);
    internal static string SharedVideoUnavailable => ExceptionText.Get(CoreErrorResourceKeys.SharedVideoUnavailable);
    internal static string ResponseUnavailable => ExceptionText.Get(CoreErrorResourceKeys.ResponseUnavailable);
    internal static string ResponseTimeout => ExceptionText.Get(CoreErrorResourceKeys.ResponseTimeout);
    internal static string RequestCancelled => ExceptionText.Get(CoreErrorResourceKeys.RequestCancelled);
    internal static string CommunicationFailed => ExceptionText.Get(CoreErrorResourceKeys.CommunicationFailed);
    internal static string ProtocolVersionMismatchFormat => ExceptionText.Get(CoreErrorResourceKeys.ProtocolVersionMismatchFormat);
    internal static string UnknownCommandFormat => ExceptionText.Get(CoreErrorResourceKeys.UnknownCommandFormat);
    internal static string InvalidResponseLengthFormat => ExceptionText.Get(CoreErrorResourceKeys.InvalidResponseLengthFormat);
    internal static string ProcessExitSuffixFormat => ExceptionText.Get(CoreErrorResourceKeys.ProcessExitSuffixFormat);
}
