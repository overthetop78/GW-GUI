namespace GWGUI.Emulation.Sega.Common.Exceptions;

/// <summary>Messages utilisés lors du démarrage et des échanges avec un hôte de cœur.</summary>
/// <remarks>Les noms suivent les mêmes situations que dans Example. Les formats conservent leurs
/// paramètres ; les textes sont résolus au moment de chaque demande dans la culture d'interface courante.</remarks>
internal static class CoreHostErrors
{
    internal static string InvalidConfiguration => ExceptionText.Get("Emulation.Error.Core.HostConfigurationInvalid");
    internal static string NotInitialized => ExceptionText.Get("Emulation.Error.Core.HostNotInitialized");
    internal static string AlreadyInitialized => ExceptionText.Get("Emulation.Error.Core.ProcessAlreadyInitialized");
    internal static string ExecutableMissing => ExceptionText.Get("Emulation.Error.Core.HostExecutableNotFound");
    internal static string ProcessStartFailed => ExceptionText.Get("Emulation.Error.Core.ProcessStartFailed");
    internal static string SharedVideoUnavailable => ExceptionText.Get("Emulation.Error.Core.VideoBufferUnavailable");
    internal static string ProcessUnavailable => ExceptionText.Get("Emulation.Error.Core.ProcessUnavailable");
    internal static string ProcessNotInitialized => ExceptionText.Get("Emulation.Error.Core.ProcessNotInitialized");
    internal static string ResponseTimeout => ExceptionText.Get("Emulation.Error.Core.ProcessTimeout");
    internal static string CommunicationFailed => ExceptionText.Get("Emulation.Error.Core.ProcessCommunicationFailed");
    internal static string ResponseUnavailable => ExceptionText.Get("Emulation.Error.Core.HostResponseUnavailable");
    internal static string InvalidResponseLengthFormat => ExceptionText.Get("Emulation.Error.Core.InvalidResponseLength");
    internal static string UnknownCommandFormat => ExceptionText.Get("Emulation.Error.Core.UnknownHostCommand");
}