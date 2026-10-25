using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Propriétaire d’une bibliothèque native chargée pour une session ou une inspection de cœur.</summary>
/// <remarks>Le créateur possède la bibliothèque. Les délégués résolus en empruntent le code : leurs appels
/// et callbacks doivent être arrêtés avant Dispose. La session déinitialise le cœur, détache ses callbacks
/// puis libère la bibliothèque dans son finally ; elle ne libère jamais une bibliothèque reçue en emprunt.</remarks>
public interface IEmulationCoreLibrary : IDisposable
{
    /// <summary>Résout un export obligatoire sous la signature de délégué fournie par l’adaptateur.</summary>
    /// <typeparam name="T">Signature native exacte, convention d’appel comprise, déclarée par le contrat d’interopérabilité.</typeparam>
    /// <param name="exportName">Nom invariant de l’export recherché dans la bibliothèque chargée.</param>
    /// <returns>Délégué empruntant la bibliothèque ; sa durée d’utilisation est bornée par celle du propriétaire.</returns>
    /// <remarks>Les exports absents et les appels après Dispose conservent leurs exceptions existantes.</remarks>
    T Resolve<T>(string exportName) where T : Delegate;

    /// <summary>Résout la table commune d’exports du cœur sous les signatures publiques existantes.</summary>
    /// <param name="requireLoadGameSpecial">True pour exiger aussi l’export de chargement de sous-système ; false pour laisser ce délégué à null.</param>
    /// <returns>Table de délégués empruntant la bibliothèque, sans transfert de sa propriété.</returns>
    ExternalCoreExports ResolveExports(bool requireLoadGameSpecial);
}
