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
    /// <summary>Publie les transitions des touches vers le callback clavier enregistré par le cœur.</summary>
    /// <param name="snapshot">Instantané emprunté ; le propriétaire conserve une copie des touches après publication.</param>
    /// <remarks>Les relâchements précèdent les pressions. Sans callback, aucun événement natif n’est émis.</remarks>
    void SetInput(EmulationInputSnapshot snapshot);
    /// <summary>Latence minimale demandée par le cœur, en millisecondes ; zéro signifie aucune demande.</summary>
    int MinimumAudioLatencyMilliseconds { get; }
    /// <summary>Transmet le remplissage de la sortie audio au callback enregistré par le cœur.</summary>
    /// <param name="status">État emprunté de la sortie physique ; null indique une sortie indisponible.</param>
    /// <remarks>Le callback natif reste emprunté. Cette publication ne crée ni ne ferme la sortie audio.</remarks>
    void PublishAudioBufferStatus(AudioOutputBufferStatus? status);
    /// <summary>Applique cadence, fréquence audio et rapport d’image annoncés par le cœur aux propriétaires empruntés.</summary>
    void ApplyAvInfo(ExternalCoreApi.SystemAvInfo info);
}
