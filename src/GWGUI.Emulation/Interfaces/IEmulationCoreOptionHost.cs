namespace GWGUI.Emulation.Interfaces;

/// <summary>Propriétaire des valeurs, de la visibilité et des chaînes natives d’un catalogue d’options de cœur.</summary>
/// <typeparam name="TOption">Description d’option du module, adaptée par les délégués reçus à la construction.</typeparam>
/// <remarks>Les tables natives reçues sont empruntées pendant les appels. Le propriétaire conserve ses copies
/// et les allocations de valeurs ; la session le libère dans son finally après la fin des callbacks natifs.
/// Les fonctions de présentation sont empruntées ; leur ordre d’appel et leurs exceptions sont conservés.</remarks>
public interface IEmulationCoreOptionHost<TOption> : IDisposable where TOption : class
{
    /// <summary>Catalogue courant, remplacé lors des changements de valeurs, de visibilité ou de présentation.</summary>
    IReadOnlyList<TOption> Catalog { get; }
    /// <summary>Vue empruntée des valeurs enregistrées, utilisée pour produire la configuration du module.</summary>
    IReadOnlyDictionary<string, string> DocumentValues { get; }
    /// <summary>Remplit une variable native empruntée avec l’adresse de sa valeur possédée ; renvoie false si elle est inconnue.</summary>
    bool ReturnValue(nint data);
    /// <summary>Applique la valeur d’une variable native empruntée si le catalogue l’accepte.</summary>
    bool SetNativeValue(nint data);
    /// <summary>Écrit puis consomme l’indicateur de modification dans le booléen natif emprunté.</summary>
    bool GetAndClearUpdated(nint data);
    /// <summary>Applique la visibilité d’une option depuis le descripteur natif emprunté et actualise le catalogue.</summary>
    bool ApplyVisibility(nint data);
    /// <summary>Conserve le callback natif de mise à jour d’affichage ; son code doit rester chargé jusqu’à Dispose.</summary>
    bool CaptureDisplayUpdate(nint data);
    /// <summary>Valide les valeurs configurées selon le catalogue et signale les rejets par le callback reçu.</summary>
    void ValidateConfiguredValues(Action<string, string> invalid);
    /// <summary>Applique une valeur acceptée, remplace sa chaîne native et notifie le callback d’affichage ; false en cas de rejet.</summary>
    bool TrySetValue(string key, string value);
    /// <summary>Installe les descriptions reçues avec les valeurs configurées et leur présentation, sans modifier les descriptions sources.</summary>
    void ReplaceCatalog(IReadOnlyList<TOption> options);
}
