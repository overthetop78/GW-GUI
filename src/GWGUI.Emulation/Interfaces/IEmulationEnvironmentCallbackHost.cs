using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Propriétaire du callback d’environnement et de ses options, chemins et système de fichiers virtuel.</summary>
/// <remarks>Les propriétaires audio et vidéo reçus restent empruntés : leur appelant les libère séparément.
/// Cet hôte possède les options créées par sa fabrique, les chaînes de chemins et le VFS. La session arrête
/// les appels du cœur avant Dispose dans son finally. Les délégués natifs fournis par le cœur restent empruntés.</remarks>
public interface IEmulationEnvironmentCallbackHost : IDisposable
{
    /// <summary>Callback négociant les commandes réellement prises en charge ; false pour une commande non gérée.</summary>
    ExternalCoreApi.EnvironmentCallback Callback { get; }
    /// <summary>Callback clavier annoncé par le cœur, ou null ; son code doit rester chargé pendant les notifications.</summary>
    ExternalCoreApi.KeyboardEvent? Keyboard { get; }
    /// <summary>Descriptions courantes des options déclarées par le cœur.</summary>
    IReadOnlyList<EmulationEnabledOption> Options { get; }
    /// <summary>Diagnostics textuels possédés par l’hôte et vidés lors de Dispose.</summary>
    List<string> Diagnostics { get; }
    /// <summary>Prise en charge du démarrage sans contenu annoncée par le cœur.</summary>
    bool SupportsNoGame { get; }
    /// <summary>Cadence annoncée en frames par seconde, utilisée par la boucle de session.</summary>
    double FramesPerSecond { get; }
    /// <summary>Première erreur interceptée par le callback d’environnement ou de journalisation.</summary>
    Exception? Failure { get; }
    /// <summary>Applique une valeur d’option avec les validations et la notification du propriétaire d’options.</summary>
    void SetOption(string key, string value);
    /// <summary>Applique cadence, fréquence audio et rapport d’image annoncés par le cœur aux propriétaires empruntés.</summary>
    void ApplyAvInfo(ExternalCoreApi.SystemAvInfo info);
}
