namespace GWGUI.Emulation.Example.Common.Exceptions;

/// <summary>Signale qu’un cœur attend la sélection d’un média pour poursuivre son chargement.</summary>
/// <param name="message">Diagnostic de la demande de média, conservé dans Exception.Message.</param>
/// <remarks>Ce type permet au transport de reconnaître une demande de contenu au lieu de la confondre avec
/// une erreur générale. Le récepteur peut restituer cette demande sans remplacer la cause par un autre diagnostic.
/// L’exception ne monte aucun média et ne prend possession d’aucune ressource.</remarks>
internal sealed class CoreMediaRequiredException(string message) : Exception(message);
