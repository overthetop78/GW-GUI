namespace GWGUI.Emulation.Sega.Common.Exceptions;

/// <summary>Messages utilisés lors du démarrage et des échanges avec un hôte de cœur.</summary>
/// <remarks>Les noms suivent les mêmes situations que dans Example. Les formats conservent leurs
/// paramètres ; les textes sont résolus au moment de chaque demande dans la culture d'interface courante.</remarks>
internal static class CoreHostErrors
{
    internal static string InvalidConfiguration => ExceptionText.Get(CoreErrorResourceKeys.HostConfigurationInvalid);
    internal static string NotInitialized => ExceptionText.Get(CoreErrorResourceKeys.HostNotInitialized);
    internal static string AlreadyInitialized => ExceptionText.Get(CoreErrorResourceKeys.ProcessAlreadyInitialized);
    internal static string ExecutableMissing => ExceptionText.Get(CoreErrorResourceKeys.HostExecutableNotFound);
    internal static string ProcessStartFailed => ExceptionText.Get(CoreErrorResourceKeys.ProcessStartFailed);
    internal static string SharedVideoUnavailable => ExceptionText.Get(CoreErrorResourceKeys.VideoBufferUnavailable);
    internal static string ProcessUnavailable => ExceptionText.Get(CoreErrorResourceKeys.ProcessUnavailable);
    internal static string ProcessNotInitialized => ExceptionText.Get(CoreErrorResourceKeys.ProcessNotInitialized);
    internal static string ResponseTimeout => ExceptionText.Get(CoreErrorResourceKeys.ProcessTimeout);
    internal static string CommunicationFailed => ExceptionText.Get(CoreErrorResourceKeys.ProcessCommunicationFailed);
    internal static string ResponseUnavailable => ExceptionText.Get(CoreErrorResourceKeys.HostResponseUnavailable);
    internal static string InvalidResponseLengthFormat => ExceptionText.Get(CoreErrorResourceKeys.InvalidResponseLength);
    internal static string UnknownCommandFormat => ExceptionText.Get(CoreErrorResourceKeys.UnknownHostCommand);
}