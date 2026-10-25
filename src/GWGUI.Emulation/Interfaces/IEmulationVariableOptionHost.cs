namespace GWGUI.Emulation.Interfaces;

/// <summary>Propriétaire d’options déclarées par le protocole natif de variables et leurs choix textuels.</summary>
/// <typeparam name="TOption">Description d’option produite par les délégués du module.</typeparam>
/// <remarks>Ce protocole correspond aux commandes réellement utilisées par le cœur. Le propriétaire copie
/// les descriptions et conserve les chaînes de valeurs natives jusqu’à leur remplacement ou Dispose.
/// La session le libère après le dernier callback dans son finally ; les tables natives reçues restent empruntées.</remarks>
public interface IEmulationVariableOptionHost<TOption> : IDisposable
{
    /// <summary>Copie des descriptions actuellement enregistrées, avec leurs valeurs sélectionnées.</summary>
    IReadOnlyList<TOption> Options { get; }
    /// <summary>Copie la table native empruntée, terminée par une clé nulle, en validant les choix ; false si son adresse est nulle.</summary>
    bool Register(nint data);
    /// <summary>Remplit la variable native empruntée avec sa valeur possédée ; false si la clé est absente.</summary>
    bool ReturnValue(nint data);
    /// <summary>Remplace une valeur acceptée et sa chaîne native ; une valeur refusée conserve l’erreur existante.</summary>
    void SetValue(string key, string value);
    /// <summary>Écrit puis consomme l’indicateur de modification dans le booléen natif emprunté.</summary>
    bool ReturnUpdated(nint data);
}
