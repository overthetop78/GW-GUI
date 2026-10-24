namespace GWGUI.Emulation.Sony.Common.Exceptions;

/// <summary>Messages utilisés lors du démarrage et des échanges avec un hôte de cœur.</summary>
/// <remarks>Les noms suivent les mêmes situations que dans Example. Les formats conservent leurs
/// paramètres ; les textes sont résolus au moment de chaque demande dans la culture d'interface courante.</remarks>
internal static class CoreHostErrors
{
    internal static string InvalidConfiguration => ExceptionText.Get(CoreExceptionsResourceConstants.HostConfigurationInvalid);
    internal static string NotInitialized => ExceptionText.Get(CoreExceptionsResourceConstants.HostNotInitialized);
    internal static string UnknownCommandFormat => ExceptionText.Get(CoreExceptionsResourceConstants.UnknownHostCommand);
}