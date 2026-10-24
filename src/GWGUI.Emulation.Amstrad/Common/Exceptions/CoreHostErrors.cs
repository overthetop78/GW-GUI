namespace GWGUI.Emulation.Amstrad.Common.Exceptions;

/// <summary>Messages utilisés lors du démarrage et des échanges avec un hôte de cœur.</summary>
/// <remarks>Les noms suivent les mêmes situations que dans Example. Les formats conservent leurs
/// paramètres ; les textes sont résolus au moment de chaque demande dans la culture d'interface courante.</remarks>
internal static class CoreHostErrors
{
    internal static string InvalidConfiguration => ExceptionText.Get(CommonExceptionsConstants.HostConfigurationInvalidResourceKey);
    internal static string NotInitialized => ExceptionText.Get(CommonExceptionsConstants.HostNotInitializedResourceKey);
    internal static string AlreadyInitialized => ExceptionText.Get(CommonExceptionsConstants.ProcessAlreadyInitializedResourceKey);
    internal static string ExecutableMissing => ExceptionText.Get(CommonExceptionsConstants.HostExecutableNotFoundResourceKey);
    internal static string ProcessStartFailed => ExceptionText.Get(CommonExceptionsConstants.ProcessStartFailedResourceKey);
    internal static string SharedVideoUnavailable => ExceptionText.Get(CommonExceptionsConstants.VideoBufferUnavailableResourceKey);
    internal static string ProcessUnavailable => ExceptionText.Get(CommonExceptionsConstants.ProcessUnavailableResourceKey);
    internal static string ProcessNotInitialized => ExceptionText.Get(CommonExceptionsConstants.ProcessNotInitializedResourceKey);
    internal static string ResponseTimeout => ExceptionText.Get(CommonExceptionsConstants.ProcessTimeoutResourceKey);
    internal static string CommunicationFailed => ExceptionText.Get(CommonExceptionsConstants.ProcessCommunicationFailedResourceKey);
    internal static string ResponseUnavailable => ExceptionText.Get(CommonExceptionsConstants.HostResponseUnavailableResourceKey);
    internal static string InvalidResponseLengthFormat => ExceptionText.Get(CommonExceptionsConstants.InvalidResponseLengthResourceKey);
    internal static string UnknownCommandFormat => ExceptionText.Get(CommonExceptionsConstants.UnknownHostCommandResourceKey);
}